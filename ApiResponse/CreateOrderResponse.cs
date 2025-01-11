using Newtonsoft.Json;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ на запрос создания заказа.
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>
        /// Тип ответа (например, "success").
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// ID созданного заказа.
        /// </summary>
        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        /// <summary>
        /// Хэш заказа.
        /// </summary>
        [JsonProperty("orderHash")]
        public string OrderHash { get; set; }

        /// <summary>
        /// URL для оплаты заказа.
        /// </summary>
        [JsonProperty("location")]
        public string PaymentUrl { get; set; }
    }
}
