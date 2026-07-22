using System.Text.Json.Serialization;

namespace MarbleWebProject.Models
{
    public class ProductAttributeList
    {
        public string AttributeName { get; set; }

        /// <summary>API alanı (variant / katalog özellik değeri).</summary>
        public string AttributeDetails { get; set; }

        public string AttributeOrder { get; set; }

        /// <summary>Eski view uyumu; JSON’da AttributeDetails ile aynı içeriği kullanır.</summary>
        [JsonIgnore]
        public string AttributeDescription
        {
            get => AttributeDetails;
            set => AttributeDetails = value;
        }
    }
}
