using System;

namespace FreeKassa.COM.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при ошибке 500 (Server Error).
    /// </summary>
    public class ServerErrorException : Exception
    {
        public ServerErrorException() : base($"Ошибка 500: Ошибка на стороне сервера.")
        {
        }
    }
}
