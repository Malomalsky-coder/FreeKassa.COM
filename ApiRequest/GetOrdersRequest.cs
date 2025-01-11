using System;
using System.Collections.Generic;
using System.Text;

namespace FreeKassa.COM.ApiRequest
{
    /// <summary>
    /// Тело запроса для получения списка заказов.
    /// </summary>
    public class GetOrdersRequest
    {
        /// <summary>
        /// Номер заказа Freekassa.
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Номер заказа в Вашем магазине.
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Статус заказа.
        /// </summary>
        public int? OrderStatus { get; set; }

        /// <summary>
        /// Дата с.
        /// </summary>
        public string DateFrom { get; set; }

        /// <summary>
        /// Дата по.
        /// </summary>
        public string DateTo { get; set; }

        /// <summary>
        /// Страница.
        /// </summary>
        public int? Page { get; set; }

        /// <summary>
        /// Валидирует запрос.
        /// </summary>
        public void Validate()
        {
            if (OrderId.HasValue && OrderId.Value < 0) throw new ArgumentException("OrderId не может быть отрицательным.", nameof(OrderId));
            if (OrderStatus.HasValue && (OrderStatus.Value < 0 || OrderStatus.Value > 5)) throw new ArgumentException("OrderStatus должен быть в диапазоне от 0 до 5.", nameof(OrderStatus));
            if (!string.IsNullOrEmpty(DateFrom) && !DateTime.TryParse(DateFrom, out _)) throw new ArgumentException("DateFrom должен быть в формате даты.", nameof(DateFrom));
            if (!string.IsNullOrEmpty(DateTo) && !DateTime.TryParse(DateTo, out _)) throw new ArgumentException("DateTo должен быть в формате даты.", nameof(DateTo));
            if (Page.HasValue && Page.Value < 1) throw new ArgumentException("Page не может быть меньше 1.", nameof(Page));
        }
    }
}
