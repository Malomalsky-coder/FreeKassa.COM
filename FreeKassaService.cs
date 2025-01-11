using FreeKassa.COM.ApiRequest;
using FreeKassa.COM.ApiResponse;
using FreeKassa.COM.Exceptions;
using Newtonsoft.Json;
using System;
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
        /// <param name="request">Параметры запроса для фильтрации заказов.</param>
        /// <returns>Список заказов.</returns>
        /// <exception cref="BadRequestException">Ошибка 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Ошибка 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Другие ошибки.</exception>
        /// <example>
        /// Пример использования:
        /// <code>
        /// var request = new GetOrdersRequest
        /// {
        ///     OrderId = 123456789,
        ///     DateFrom = "2023-01-01",
        ///     DateTo = "2023-12-31",
        ///     Page = 1
        /// };
        ///
        /// try
        /// {
        ///     var ordersResponse = await _freeKassaService.GetOrdersAsync(request);
        ///     Console.WriteLine($"Найдено заказов: {ordersResponse.Orders.Count}");
        /// }
        /// catch (BadRequestException ex)
        /// {
        ///     Console.WriteLine($"Ошибка 400: {ex.Message}");
        /// }
        /// catch (UnauthorizedException)
        /// {
        ///     Console.WriteLine("Ошибка 401: Неверная авторизация.");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Ошибка: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public async Task<OrdersResponse> GetOrdersAsync(GetOrdersRequest request)
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
            var responseJson = JsonConvert.DeserializeObject<OrdersResponse>(responseBody);

            if (responseJson!.Type != "success")
            {
                throw new Exception("Неизвестный тип ответа.");
            }

            return responseJson;
        }

        /// <summary>
        /// Создает новый заказ.
        /// </summary>
        /// <param name="request">Параметры запроса для создания заказа.</param>
        /// <returns>URL для оплаты заказа.</returns>
        /// <exception cref="BadRequestException">Ошибка 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Ошибка 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Другие ошибки.</exception>
        /// <example>
        /// Пример использования:
        /// <code>
        /// var request = new CreateOrderRequest
        /// {
        ///     PaymentSystemId = 1,
        ///     Email = "test@example.com",
        ///     Ip = "127.0.0.1",
        ///     Amount = 100.50m,
        ///     Currency = "RUB",
        ///     SuccessUrl = "https://example.com/success",
        ///     FailureUrl = "https://example.com/failure"
        /// };
        ///
        /// try
        /// {
        ///     var paymentUrl = await _freeKassaService.CreateOrderAsync(request);
        ///     Console.WriteLine($"URL для оплаты: {paymentUrl}");
        /// }
        /// catch (BadRequestException ex)
        /// {
        ///     Console.WriteLine($"Ошибка 400: {ex.Message}");
        /// }
        /// catch (UnauthorizedException)
        /// {
        ///     Console.WriteLine("Ошибка 401: Неверная авторизация.");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Ошибка: {ex.Message}");
        /// }
        /// </code>
        /// </example>
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
                return orderResponse.PaymentUrl;
            }
            else
            {
                throw new Exception("Неизвестный тип ответа.");
            }
        }

        /// <summary>
        /// Выполняет возврат средств по заказу.
        /// </summary>
        /// <param name="orderId">Номер заказа Freekassa.</param>
        /// <param name="paymentId">Номер заказа в магазине.</param>
        /// <returns>Ответ от сервера с результатом возврата.</returns>
        /// <exception cref="BadRequestException">Ошибка 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Ошибка 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Другие ошибки.</exception>
        /// <example>
        /// Пример использования:
        /// <code>
        /// try
        /// {
        ///     var refundResponse = await _freeKassaService.RefundAsync(orderId: 123456789);
        ///     Console.WriteLine($"Возврат успешно выполнен. ID возврата: {refundResponse.Id}");
        /// }
        /// catch (BadRequestException ex)
        /// {
        ///     Console.WriteLine($"Ошибка 400: {ex.Message}");
        /// }
        /// catch (UnauthorizedException)
        /// {
        ///     Console.WriteLine("Ошибка 401: Неверная авторизация.");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Ошибка: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public async Task<RefundResponse> RefundAsync(int? orderId = null, string paymentId = null)
        {
            if (orderId == null && string.IsNullOrEmpty(paymentId))
            {
                throw new ArgumentException("Необходимо указать orderId или paymentId.");
            }

            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            var requestUri = new StringBuilder($"refund?shopId={_shopId}&nonce={nonce}&signature={signature}");

            if (orderId.HasValue)
            {
                requestUri.Append($"&orderId={orderId}");
            }

            if (!string.IsNullOrEmpty(paymentId))
            {
                requestUri.Append($"&paymentId={paymentId}");
            }

            var response = await _httpClient.PostAsync(requestUri.ToString(), null);

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
            var responseJson = JsonConvert.DeserializeObject<RefundResponse>(responseBody);

            if (responseJson!.Type != "success")
            {
                throw new Exception("Неизвестный тип ответа.");
            }

            return responseJson;
        }

        #endregion

        #region Выплаты
        #endregion

        #region Разное

        /// <summary>
        /// Проверяет статус валюты.
        /// </summary>
        /// <param name="currency">Код валюты для проверки.</param>
        /// <returns>Статус валюты.</returns>
        /// <exception cref="BadRequestException">Ошибка 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Ошибка 401 (Unauthorized).</exception>
        /// <exception cref="Exception">Другие ошибки.</exception>
        /// <example>
        /// Пример использования:
        /// <code>
        /// try
        /// {
        ///     var statusResponse = await _freeKassaService.CheckCurrencyStatusAsync("RUB");
        ///     Console.WriteLine($"Статус валюты RUB: {statusResponse.Status}");
        /// }
        /// catch (BadRequestException ex)
        /// {
        ///     Console.WriteLine($"Ошибка 400: {ex.Message}");
        /// }
        /// catch (UnauthorizedException)
        /// {
        ///     Console.WriteLine("Ошибка 401: Неверная авторизация.");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Ошибка: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public async Task<CurrenciesResponse> GetCurrenciesAsync()
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
            var responseJson = JsonConvert.DeserializeObject<CurrenciesResponse>(responseBody);

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
            var responseJson = JsonConvert.DeserializeObject<CheckCurrencyStatusResponse>(responseBody);
            return responseJson!.Type == "success";
        }

        #endregion
    }
}
