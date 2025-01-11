using Newtonsoft.Json;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ от API в случае ошибки.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Тип ответа (например, "error").
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
