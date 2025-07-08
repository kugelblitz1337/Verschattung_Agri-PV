namespace API.Common.Interfaces
{
    public interface IAuthentificationHandler
    {
        /// <summary>
        /// Prüfen ob Authentifizierungstoken valide
        /// </summary>
        /// <param name="authToken">Authentifizierungstoken</param>
        /// <returns>true=erfolgreich, false=nicht erfolgreich</returns>
        bool ValidateAuthenticationToken(string authToken);
    }
}