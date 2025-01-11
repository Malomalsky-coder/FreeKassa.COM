using FreeKassa.COM.ApiRequest;
using FreeKassa.COM.ApiResponse;
using FreeKassa.COM.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FreeKassa.COM
{
    /// <summary>
    /// Реализация контракта.
    /// Все запросы отправляются на урл https://api.freekassa.com/v1/ в формате JSON. Перед началом работы необходимо получить API ключ на странице настроек в личном кабинете
    /// </summary>
    public class FreeKassaService : IFreeKassaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _shopId;

        public FreeKassaService(string apiKey, string shopId)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _shopId = shopId;
            _httpClient.BaseAddress = new Uri("https://api.freekassa.com/v1/");
        }

        //TODO: Список заказов - https://docs.freekassa.com/#operation/getOrders
        //TODO: Возврат - https://docs.freekassa.com/#operation/refundOrder
        //TODO: Список выплат - https://docs.freekassa.com/#operation/getWithdrawals
        //TODO: Создать выплату - https://docs.freekassa.com/#operation/createWithdrawal
        //TODO: Получение баланса - https://docs.freekassa.com/#operation/getBalance
        //TODO: Получение списка доступных платежных систем для вывода - https://docs.freekassa.com/#operation/getWithdrawalsCurrencies
        //TODO: Получение списка Ваших магазинов - https://docs.freekassa.com/#operation/getShops

        /// <summary>
        /// Получить текущее время UnixTime в миллисекундах
        /// </summary>
        /// <returns></returns>
        private long CurrentUnixTimeInMilliseconds()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalMilliseconds;
        }

        /// <summary>
        /// Генерирует подпись для уникального ID запроса.
        /// </summary>
        /// <param name="nonce">Уникальный ID запроса.</param>
        /// <returns>Подпись в виде строки в нижнем регистре.</returns>
        private string GenerateSignature(long nonce)
        {
            var data = $"{_shopId}|{nonce}";
            return HMACSHA256Helper.ComputeSignature(data, _apiKey);
        }

        #region Заказы

        /// <summary>
        /// Получает список заказов.
        /// </summary>
        /// <param name="request">Тело запроса.</param>
        /// <returns>Список заказов.</returns>
        /// <exception cref="BadRequestException">Возникает при ошибке 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Возникает при ошибке 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Возникает при других ошибках.</exception>
        public async Task<GetOrdersResponse> GetOrdersAsync(GetOrdersRequest request)
        {
            request.Validate();

            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            // Формируем URI с параметрами запроса
            var requestUri = new StringBuilder($"orders?shopId={_shopId}&nonce={nonce}&signature={signature}");

            // Добавляем необязательные параметры
            if (request.OrderId.HasValue)
            {
                requestUri.Append($"&orderId={request.OrderId}");
            }

            if (!string.IsNullOrEmpty(request.PaymentId))
            {
                requestUri.Append($"&paymentId={request.PaymentId}");
            }

            if (request.OrderStatus.HasValue)
            {
                requestUri.Append($"&orderStatus={request.OrderStatus}");
            }

            if (!string.IsNullOrEmpty(request.DateFrom))
            {
                requestUri.Append($"&dateFrom={request.DateFrom}");
            }

            if (!string.IsNullOrEmpty(request.DateTo))
            {
                requestUri.Append($"&dateTo={request.DateTo}");
            }

            if (request.Page.HasValue)
            {
                requestUri.Append($"&page={request.Page}");
            }

            // Отправляем GET-запрос
            var response = await _httpClient.GetAsync(requestUri.ToString());

            // Обрабатываем ответ
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                throw new BadRequestException(error!.Message);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<GetOrdersResponse>(responseBody);

            if (responseJson!.Type != "success")
            {
                throw new Exception("Неизвестный тип ответа.");
            }

            return responseJson;
        }

        /// <summary>
        /// Создаёт заказ и возвращает ссылку на оплату.
        /// </summary>
        /// <param name="request">Тело запроса.</param>
        /// <returns>Ссылка на оплату.</returns>
        /// <exception cref="BadRequestException">Возникает при ошибке 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Возникает при ошибке 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Возникает при других ошибках.</exception>
        public async Task<string> CreateOrderAsync(CreateOrderRequest request)
        {
            // Проверяем обязательные параметры
            request.Validate();

            // Генерируем подпись
            var nonce = CurrentUnixTimeInMilliseconds();
            var signature = GenerateSignature(nonce);

            // Формируем URI с параметрами запроса
            var requestUri = new StringBuilder($"orders/create?shopId={_shopId}&nonce={nonce}&signature={signature}&i={request.PaymentSystemId}&email={request.Email}&ip={request.Ip}&amount={request.Amount}&currency={request.Currency}");

            // Добавляем необязательные параметры
            if (!string.IsNullOrEmpty(request.PaymentId))
            {
                requestUri.Append($"&paymentId={request.PaymentId}");
            }

            if (!string.IsNullOrEmpty(request.Tel))
            {
                requestUri.Append($"&tel={request.Tel}");
            }

            if (!string.IsNullOrEmpty(request.SuccessUrl))
            {
                requestUri.Append($"&success_url={request.SuccessUrl}");
            }

            if (!string.IsNullOrEmpty(request.FailureUrl))
            {
                requestUri.Append($"&failure_url={request.FailureUrl}");
            }

            if (!string.IsNullOrEmpty(request.NotificationUrl))
            {
                requestUri.Append($"&notification_url={request.NotificationUrl}");
            }

            // Отправляем POST-запрос
            var response = await _httpClient.PostAsync(requestUri.ToString(), null);

            // Обрабатываем ответ
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                throw new BadRequestException(error!.Message);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка: {response.StatusCode}");
            }

            // Читаем и десериализуем ответ
            var responseBody = await response.Content.ReadAsStringAsync();
            var orderResponse = JsonConvert.DeserializeObject<CreateOrderResponse>(responseBody);

            // Возвращаем ссылку на оплату
            if (orderResponse!.Type == "success")
            {
                return orderResponse.Location;
            }
            else
            {
                throw new Exception("Неизвестный тип ответа.");
            }
        }

        #endregion

        #region Выплаты
        #endregion

        #region Разное

        /// <summary>
        /// Получает список валют, доступных для оплаты.
        /// </summary>
        /// <returns>Объект <see cref="CurrencyListResponse"/>, содержащий список валют.</returns>
        /// <exception cref="BadRequestException">Возникает при ошибке 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Возникает при ошибке 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Возникает при других ошибках.</exception>
        public async Task<CurrencyListResponse> GetCurrenciesAsync()
        {
            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            var requestUri = $"currencies?shopId={_shopId}&nonce={nonce}&signature={signature}";

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                throw new BadRequestException(error!.Message);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<CurrencyListResponse>(responseBody);

            if (responseJson!.Type != "success")
            {
                throw new Exception("Неизвестный тип ответа.");
            }

            return responseJson;
        }

        /// <summary>
        /// Проверяет статус валюты.
        /// </summary>
        /// <returns>True, если валюта доступна, False в противном случае.</returns>
        /// <exception cref="BadRequestException">Возникает при ошибке 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Возникает при ошибке 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Возникает при других ошибках.</exception>
        public async Task<bool> CheckCurrencyStatusAsync()
        {
            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            var requestUri = $"check?shopId={_shopId}&nonce={nonce}&signature={signature}";

            var response = await _httpClient.GetAsync(requestUri);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                throw new BadRequestException(error.Message);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

            return responseJson["type"] == "success";
        }

        #endregion
    }
}
