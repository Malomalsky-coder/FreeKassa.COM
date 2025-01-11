using System;

namespace FreeKassa.COM.ApiRequest
{
    /// <summary>
    /// Тело запроса для создания заказа.
    /// </summary>
    public class CreateOrderRequest
    {
        public int ShopId { get; set; }
        public long Nonce { get; set; }
        public string Signature { get; set; }
        public int PaymentSystemId { get; set; }
        public string Email { get; set; }
        public string Ip { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string? PaymentId { get; set; }
        public string? Tel { get; set; }
        public string? SuccessUrl { get; set; }
        public string? FailureUrl { get; set; }
        public string? NotificationUrl { get; set; }

        public void Validate()
        {
            if (ShopId == 0) throw new ArgumentException("ID магазина является обязательным параметром.");
            if (Nonce == 0) throw new ArgumentException("Уникальный ID запроса является обязательным параметром.");
            if (string.IsNullOrEmpty(Signature)) throw new ArgumentException("Подпись запроса является обязательным параметром.");
            if (PaymentSystemId == 0) throw new ArgumentException("ID платежной системы является обязательным параметром.");
            if (string.IsNullOrEmpty(Email)) throw new ArgumentException("Email покупателя является обязательным параметром.");
            if (string.IsNullOrEmpty(Ip)) throw new ArgumentException("IP покупателя является обязательным параметром.");
            if (Amount <= 0) throw new ArgumentException("Сумма оплаты должна быть больше нуля.");
            if (string.IsNullOrEmpty(Currency)) throw new ArgumentException("Валюта оплаты является обязательным параметром.");
        }
    }
}
