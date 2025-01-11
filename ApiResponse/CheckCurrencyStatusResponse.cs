using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ на запрос проверки статуса валюты.
    /// </summary>
    public class CheckCurrencyStatusResponse
    {
        /// <summary>
        /// Тип ответа (например, "success").
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
