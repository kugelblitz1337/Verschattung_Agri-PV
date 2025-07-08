using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Threading.Tasks;
using ALKISService.Repository;

namespace ALKISService.Services
{

    public interface IZipDownloader
    {
        Task<(string zipPath, string checksum, bool exists)> DownloadZipAsync(string url, string zipName,
            string marking,
            string markingKey,
            string downloadDir, string xmlName);
        void UnzipAndMoveXml(string zipPath, string xmlName,string extractDir, string downloadDir);
    }
    public class ZipDownloader : IZipDownloader
    {
        private readonly IMarkingRepository _markingRepository;
        private readonly IFlurstueckRepository _flurstueckRepository;

        public ZipDownloader(IMarkingRepository markingRepository, IFlurstueckRepository flurstueckRepository)
        {
            _markingRepository = markingRepository;
            _flurstueckRepository = flurstueckRepository;
        }
        public async Task<(string zipPath, string checksum, bool exists)> DownloadZipAsync(
            string url,
            string zipName,
            string marking, 
            string markingKey,
            string downloadDir,
            string xmlName)
        {
            string zielpfad = Path.Combine(downloadDir, zipName);

            using var httpClient = new HttpClient();
            HttpResponseMessage response = null;
            try
            {
                response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    if (markingKey != null && marking != null)
                        _markingRepository.SetError(markingKey, marking, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                    return (null, null, false);
                }
            }
            catch (Exception ex)
            {
                if (markingKey != null && marking != null)
                    _markingRepository.SetError(markingKey, marking, ex.Message);
                return (null, null, false);
            }

            await using (var fs = new FileStream(zielpfad, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }

            string checksum = GetFileChecksum(zielpfad);

            if (_markingRepository.ChecksumExists(checksum, xmlName))
            {
                File.Delete(zielpfad);
                return (null, checksum, true);
            }
            else
            {
                //Wenn Checksum noch nicht existiert Datensätze löschen
                _flurstueckRepository.Delete(xmlName);
            }

            UnzipAndMoveXml(zielpfad, xmlName, downloadDir + $"\\{zipName.Replace(".zip", "")}", downloadDir);

            // Nach Download: Checksumme eintragen
            if (markingKey != null && marking != null)
                _markingRepository.InsertChecksum(checksum, markingKey, marking, xmlName);

            // (Optional: Erfolgreicher Download -> Error-Flag löschen)
            if (markingKey != null && marking != null)
                _markingRepository.SetError(markingKey, marking, null);

            return (zielpfad, checksum, false);
        }


        public void UnzipAndMoveXml(string zipPath, string xmlName, string extractDir, string downloadDir)
        {
            ZipFile.ExtractToDirectory(zipPath, extractDir, true);
            string xmlQuelle = Path.Combine(extractDir, xmlName);
            if (File.Exists(xmlQuelle))
            {
                string ziel = Path.Combine(downloadDir, xmlName);
                File.Move(xmlQuelle, ziel, true);
                Directory.Delete(extractDir, true);
            }
            File.Delete(zipPath);
        }

        private static string GetFileChecksum(string path)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(path);
            byte[] hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

}
