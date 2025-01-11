using System;
using System.Collections.Generic;
using System.Text;

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
        public string Type { get; set; }

        /// <summary>
        /// Идентификатор созданного заказа.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Хеш-код заказа, используемый для проверки целостности данных.
        /// </summary>
        public string OrderHash { get; set; }

        /// <summary>
        /// URL-адрес для перенаправления клиента на страницу оплаты.
        /// </summary>
        public string Location { get; set; }
    }
}
