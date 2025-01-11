using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Вспомогательный класс для работы с HMAC SHA-256.
/// </summary>
public static class HMACSHA256Helper
{
    /// <summary>
    /// Вычисляет HMAC SHA-256 подпись для данных с использованием секретного ключа.
    /// </summary>
    /// <param name="data">Данные для подписи.</param>
    /// <param name="secretKey">Секретный ключ.</param>
    /// <returns>HMAC SHA-256 подпись в виде строки в нижнем регистре.</returns>
    public static string ComputeSignature(string data, string secretKey)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}