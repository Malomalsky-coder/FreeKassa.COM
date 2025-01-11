using System;
using System.Collections.Generic;
using System.Text;

namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Ответ на запрос возврата средств.
    /// </summary>
    public class RefundResponse
    {
        /// <summary>
        /// Тип ответа.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// ID возврата.
        /// </summary>
        public int Id { get; set; }
    }
}
