using System;
using System.Collections.Generic;
using System.Text;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ от API, содержащий список валют.
    /// </summary>
    public class CurrencyListResponse
    {
        /// <summary>
        /// Тип ответа (например, "success").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Список валют.
        /// </summary>
        public List<Currency> Currencies { get; set; }
    }
}
