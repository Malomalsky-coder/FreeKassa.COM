using System;
using System.Collections.Generic;
using System.Text;

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
        public string Type { get; set; }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string Message { get; set; }
    }
}
