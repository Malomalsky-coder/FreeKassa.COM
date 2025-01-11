using System;

namespace FreeKassa.COM.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при ошибке 400 (Bad Request).
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string ErrorMessage { get; }

        public BadRequestException(string message) : base($"Ошибка 400: {message}")
        {
            ErrorMessage = message;
        }
    }
}
