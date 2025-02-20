﻿using Newtonsoft.Json;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ на запрос возврата средств.
    /// </summary>
    public class RefundResponse
    {
        /// <summary>
        /// Тип ответа (например, "success").
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// ID возврата.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
