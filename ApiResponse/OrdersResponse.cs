using Newtonsoft.Json;
using System.Collections.Generic;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ на запрос получения списка заказов.
    /// </summary>
    public class OrdersResponse
    {
        /// <summary>
        /// Тип ответа (например, "success").
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Список заказов.
        /// </summary>
        [JsonProperty("orders")]
        public List<Order> Orders { get; set; }
    }

    /// <summary>
    /// Заказ.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Номер заказа в магазине.
        /// </summary>
        public string MerchantOrderId { get; set; }

        /// <summary>
        /// Номер заказа Freekassa.
        /// </summary>
        public int FkOrderId { get; set; }

        /// <summary>
        /// Сумма заказа.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Валюта заказа.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Email клиента.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Номер счета.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Дата заказа.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Статус заказа.
        /// </summary>
        public int Status { get; set; }
    }
}
