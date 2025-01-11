using FreeKassa.COM.ApiRequest;
using FreeKassa.COM.ApiResponse;
using System.Threading.Tasks;

namespace FreeKassa.COM
{
    /// <summary>
    /// Контракт сервиса FreeKassa.com
    /// </summary>
    public interface IFreeKassaService
    {
        /// <summary>
        /// Получает список заказов.
        /// </summary>
        Task<OrdersResponse> GetOrdersAsync(GetOrdersRequest request);

        /// <summary>
        /// Создает новый заказ и возвращает ссылку на оплату.
        /// </summary>
        Task<string> CreateOrderAsync(CreateOrderRequest request);

        /// <summary>
        /// Выполняет возврат средств для указанного заказа.
        /// </summary>
        //Task<object> RefundOrderAsync(object request);

        /// <summary>
        /// Получает список выплат.
        /// </summary>
        //Task<object> GetWithdrawalsAsync();

        /// <summary>
        /// Создает новую выплату.
        /// </summary>
        //Task<object> CreateWithdrawalAsync(object request);

        /// <summary>
        /// Получает текущий баланс.
        /// </summary>
        //Task<object> GetBalanceAsync();

        /// <summary>
        /// Получает список доступных платежных систем.
        /// </summary>
        Task<CurrenciesResponse> GetCurrenciesAsync();

        /// <summary>
        /// Проверяет доступность указанной платежной системы для оплаты.
        /// </summary>
        Task<bool> CheckCurrencyStatusAsync();

        /// <summary>
        /// Получает список доступных платежных систем для вывода средств.
        /// </summary>
        //Task<object> GetWithdrawalsCurrenciesAsync();

        /// <summary>
        /// Получает список магазинов, связанных с аккаунтом.
        /// </summary>
        //Task<object> GetShopsAsync();
    }
}
