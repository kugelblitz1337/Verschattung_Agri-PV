using API.Common.Enum;
using API.Common.LocationSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.DTO
{
    /// <summary>
    /// Produkt Transferklasse
    /// </summary>
    public class Product
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal BasicPriceUnit { get; set; }
        public string ArticleNumber { get; set; }
        public ArticleNumberType ArticleNumberType { get; set; }
        public string PictureURL { get; set; }
        public string ArticleURL { get; set; }
        public List<Supermarket> Supermarket { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("<title>Preisalarm</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ccc; border-radius: 5px; }");
            sb.AppendLine(".info { margin-bottom: 20px; }");
            sb.AppendLine(".info p { margin: 5px 0; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 20px; color: #666; }");
            sb.AppendLine(".supermarkets { margin-top: 20px; }");
            sb.AppendLine(".supermarket { margin-bottom: 10px; }");
            sb.AppendLine(".logo {max - width: 200px;margin - bottom: 20px;}");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine($"<h2>Preisalarm für folgendes Produkt:</h2>");
            sb.AppendLine($"<img src=\"{PictureURL}\" alt=\"Produktbild\" width=\"200\" height=\"200\">");
            sb.AppendLine($"<p>Name: {Name}</p>");
            sb.AppendLine($"<div class=\"info\">");
            sb.AppendLine($"<p>Beschreibung: <span>{Description}</span></p>");
            sb.AppendLine($"<p>Preis: <span>{Price:C}</span></p>");
            sb.AppendLine($"<p>Basispreis: <span>{BasicPriceUnit:C}</span></p>");
            sb.AppendLine($"<p>Artikelnummer: <span>{ArticleNumber}</span></p>");
            sb.AppendLine($"<p>Artikellink: <a href=\"{ArticleURL}\">{ArticleURL}</a></p>");
            sb.AppendLine($"<div class=\"supermarkets\">");
            sb.AppendLine($"<h3>Supermärkte:</h3>");
            sb.AppendLine($"<ul>");
            foreach (var supermarket in Supermarket)
            {
                sb.AppendLine($"<li class=\"supermarket\">{supermarket.Title} <b>{supermarket.Distance}km</b></li>");
            }
            sb.AppendLine($"</ul>");
            sb.AppendLine($"</div>");
            sb.AppendLine($"</div>");
            sb.AppendLine($"</body>");
            sb.AppendLine($"</html>");

            return sb.ToString();
        }
    }

}
