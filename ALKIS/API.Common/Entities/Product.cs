using API.Common.DTO;
using API.Common.Enum;
using API.Common.Interfaces;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Entities
{
    /// <summary>
    /// Entität von Produkt
    /// </summary>
    public class Product : IEntity
    {
        private string _name;
        private string _description;
        private decimal _price;
        private decimal _basicPriceUnit;
        private string _articleNumber;
        private string _pictureURL;
        private string _articleURL;

        public Product()
        {
            // Setzen Sie die Standardwerte nur, wenn die Eigenschaften null sind
            _name = "";
            _description = "";
            _price = 0m;
            _basicPriceUnit = 0m;
            _articleNumber = "";
            _pictureURL = "";
            _articleURL = "";
        }
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name
        {
            get { return _name; }
            set { _name = value ?? ""; } // Setzen Sie den Standardwert nur, wenn value null ist
        }

        [Required]
        public string Description
        {
            get { return _description; }
            set { _description = value ?? ""; } // Setzen Sie den Standardwert nur, wenn value null ist
        }

        [Required]
        public decimal Price
        {
            get { return _price; }
            set { _price = value == null ? 0m : value; } // Setzen Sie den Standardwert nur, wenn value null ist
        }

        [Required]
        public decimal BasicPriceUnit
        {
            get { return _basicPriceUnit; }
            set { _basicPriceUnit = value == null ? 0m : value; } // Setzen Sie den Standardwert nur, wenn value null ist
        }

        [Required]
        public string ArticleNumber
        {
            get { return _articleNumber; }
            set { _articleNumber = value ?? ""; } // Setzen Sie den Standardwert nur, wenn value null ist
        }

        [Required]
        public string PictureURL
        {
            get { return _pictureURL; }
            set { _pictureURL = value ?? ""; } // Setzen Sie den Standardwert nur, wenn value null ist
        }

        [Required]
        public string ArticleURL
        {
            get { return _articleURL; }
            set { _articleURL = value ?? ""; } // Setzen Sie den Standardwert nur, wenn value null ist
        }

    }
}
