using Newtonsoft.Json;
using System.Collections.Generic;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ на запрос получения списка валют.
    /// </summary>
    public class CurrenciesResponse
    {
        /// <summary>
        /// Тип ответа (например, "success").
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Список валют.
        /// </summary>
        [JsonProperty("currencies")]
        public List<Currency> Currencies { get; set; }
    }

    /// <summary>
    /// Класс, представляющий валюту.
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// ID валюты.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Название валюты (например, "VISA").
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Код валюты (например, "RUB").
        /// </summary>
        [JsonProperty("currency")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Флаг, указывающий, включена ли валюта (1 — включена, 0 — выключена).
        /// </summary>
        [JsonProperty("is_enabled")]
        public int IsEnabled { get; set; }

        /// <summary>
        /// Флаг, указывающий, является ли валюта избранной (1 — избранная, 0 — не избранная).
        /// </summary>
        [JsonProperty("is_favorite")]
        public int IsFavorite { get; set; }
    }
}
