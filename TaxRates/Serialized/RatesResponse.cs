using System.Text.Json.Serialization;

namespace TaxRates.Serialized
{
    public class RatesResponse
    {
        [JsonPropertyName("Limsa Lominsa")]
        public int LimsaRate { get; set; }

        [JsonPropertyName("Gridania")]
        public int GridaniaRate { get; set; }

        [JsonPropertyName("Ul'dah")]
        public int UldahRate { get; set; }

        [JsonPropertyName("Ishgard")]
        public int IshgardRate { get; set; }

        [JsonPropertyName("Kugane")]
        public int KuganeRate { get; set; }

        [JsonPropertyName("Crystarium")]
        public int CrystariumRate { get; set; }

        [JsonPropertyName("Old Sharlayan")]
        public int SharlayanRate { get; set; }
    }
}
