using System;
using System.Collections.Generic;
using System.Text;

namespace FreeKassa.COM.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при ошибке 401 (Unauthorized).
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("Ошибка 401: Неавторизованный запрос.")
        {
        }
    }
}
