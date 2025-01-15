using System;

namespace FreeKassa.COM.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при неизвестном ответе от сервиса (FreeKassa.com).
    /// </summary>
    public class ApiResponseException : Exception
    {
        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string ErrorMessage { get; }

        public ApiResponseException(string message) : base($"Ошибка 400: {message}")
        {
            ErrorMessage = message;
        }
    }
}
