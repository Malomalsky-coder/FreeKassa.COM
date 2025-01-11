namespace FreeKassa.COM.ApiResponse
{
    /// <summary>
    /// Описание валюты.
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// Идентификатор валюты.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название валюты.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Минимальная сумма для операции с этой валютой.
        /// </summary>
        public decimal Min { get; set; }

        /// <summary>
        /// Максимальная сумма для операции с этой валютой.
        /// </summary>
        public decimal Max { get; set; }

        /// <summary>
        /// Код валюты (например, "RUB").
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Возможность обмена (1 — да, 0 — нет).
        /// </summary>
        public int CanExchange { get; set; }
    }
}
