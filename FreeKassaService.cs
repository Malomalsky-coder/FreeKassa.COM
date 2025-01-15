using FreeKassa.COM.ApiRequest;
using FreeKassa.COM.ApiResponse;
using FreeKassa.COM.Exceptions;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

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

        public FreeKassaService(HttpClient httpClient, string apiKey, string shopId)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _shopId = shopId;
        }

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
        /// Получает список заказов на основе указанных параметров запроса.
        /// </summary>
        /// <param name="request">Параметры запроса для фильтрации заказов.</param>
        /// <returns>Ответ, содержащий список заказов.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если входные параметры не прошли валидацию.</exception>
        /// <exception cref="BadRequestException">Выбрасывается, если сервер вернул ошибку 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Выбрасывается, если сервер вернул ошибку 401 (Unauthorized).</exception>
        /// <exception cref="ServerErrorException">Выбрасывается, если сервер вернул ошибку 500 (Internal Server Error).</exception>
        /// <exception cref="ApiResponseException">Выбрасывается, если тип ответа не "success".</exception>
        /// <exception cref="Exception">Выбрасывается при возникновении других ошибок.</exception>
        /// <example>
        /// <code>
        /// var request = new GetOrdersRequest
        /// {
        ///     OrderId = 123,
        ///     DateFrom = "2023-01-01",
        ///     DateTo = "2023-12-31"
        /// };
        /// 
        /// var ordersResponse = await freeKassaService.GetOrdersAsync(request);
        /// Console.WriteLine($"Найдено заказов: {ordersResponse.Orders.Count}");
        /// </code>
        /// </example>
        public async Task<OrdersResponse> GetOrdersAsync(GetOrdersRequest request)
        {
            // Проверка входных параметров
            request.Validate();

            // Проверка входных параметров
            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            // Формируем URI
            var requestUri = new UriBuilder($"v1/orders")
            {
                Query = $"shopId={_shopId}&nonce={nonce}&signature={signature}"
            };

            // Добавляем необязательные параметры
            if (request.OrderId.HasValue)
            {
                requestUri.Query += $"&orderId={request.OrderId}";
            }

            if (!string.IsNullOrEmpty(request.PaymentId))
            {
                requestUri.Query += $"&paymentId={request.PaymentId}";
            }

            if (request.OrderStatus.HasValue)
            {
                requestUri.Query += $"&orderStatus={request.OrderStatus}";
            }

            if (!string.IsNullOrEmpty(request.DateFrom))
            {
                requestUri.Query += $"&dateFrom={request.DateFrom}";
            }

            if (!string.IsNullOrEmpty(request.DateTo))
            {
                requestUri.Query += $"&dateTo={request.DateTo}";
            }

            if (request.Page.HasValue)
            {
                requestUri.Query += $"&page={request.Page}";
            }

            HttpResponseMessage? response = null;

            try
            {
                // Отправка GET-запроса
                response = await _httpClient.GetAsync(requestUri.ToString());

                // Проверка статуса ответа
                response.EnsureSuccessStatusCode();

                // Десериализация успешного ответа
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<OrdersResponse>(responseBody);

                if (responseJson!.Type != "success")
                {
                    throw new ApiResponseException("Неизвестный тип ответа.");
                }

                return responseJson;
            }
            catch (HttpRequestException ex)
            {
                if (response != null)
                {
                    var statusCode = response.StatusCode;
                    var errorResponse = await response.Content.ReadAsStringAsync();

                    if (statusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                        throw new BadRequestException(error!.Message);
                    }
                    else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                    else if (statusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        throw new ServerErrorException();
                    }

                    throw new Exception($"Ошибка: {statusCode} - {errorResponse}");
                }
                else
                {
                    throw new Exception($"Ошибка HTTP: {ex.Message}");
                }
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка десериализации: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает новый заказ на основе указанных параметров запроса.
        /// </summary>
        /// <param name="request">Параметры запроса для создания заказа.</param>
        /// <returns>Ответ, содержащий ссылку на оплату.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если входные параметры не прошли валидацию.</exception>
        /// <exception cref="BadRequestException">Выбрасывается, если сервер вернул ошибку 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Выбрасывается, если сервер вернул ошибку 401 (Unauthorized).</exception>
        /// <exception cref="ServerErrorException">Выбрасывается, если сервер вернул ошибку 500 (Internal Server Error).</exception>
        /// <exception cref="ApiResponseException">Выбрасывается, если тип ответа не "success".</exception>
        /// <exception cref="Exception">Выбрасывается при возникновении других ошибок.</exception>
        /// <example>
        /// <code>
        /// var request = new CreateOrderRequest
        /// {
        ///     PaymentSystemId = 1,
        ///     Email = "user@example.com",
        ///     Amount = 100.50m,
        ///     Currency = "RUB"
        /// };
        /// 
        /// var paymentUrl = await freeKassaService.CreateOrderAsync(request);
        /// Console.WriteLine($"Ссылка на оплату: {paymentUrl}");
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
            var requestUri = new UriBuilder($"v1/orders/create")
            {
                Query = $"shopId={_shopId}&nonce={nonce}&signature={signature}&i={request.PaymentSystemId}&email={request.Email}&ip={request.Ip}&amount={request.Amount}&currency={request.Currency}"
            };

            // Добавляем необязательные параметры
            if (!string.IsNullOrEmpty(request.PaymentId))
            {
                requestUri.Query += $"&paymentId={request.PaymentId}";
            }

            if (!string.IsNullOrEmpty(request.Tel))
            {
                requestUri.Query += $"&tel={request.Tel}";
            }

            if (!string.IsNullOrEmpty(request.SuccessUrl))
            {
                requestUri.Query += $"&success_url={request.SuccessUrl}";
            }

            if (!string.IsNullOrEmpty(request.FailureUrl))
            {
                requestUri.Query += $"&failure_url={request.FailureUrl}";
            }

            if (!string.IsNullOrEmpty(request.NotificationUrl))
            {
                requestUri.Query += $"&notification_url={request.NotificationUrl}";
            }

            HttpResponseMessage? response = null;

            try
            {
                // Отправляем POST-запрос
                response = await _httpClient.PostAsync(requestUri.ToString(), null);

                // Проверяем статус ответа
                response.EnsureSuccessStatusCode();

                // Читаем и десериализуем ответ
                var responseBody = await response.Content.ReadAsStringAsync();
                var orderResponse = JsonConvert.DeserializeObject<CreateOrderResponse>(responseBody);
                
                if (orderResponse!.Type != "success")
                {
                    throw new ApiResponseException("Неизвестный тип ответа.");
                }

                // Возвращаем ссылку на оплату
                return orderResponse.PaymentUrl;
            }
            catch (HttpRequestException ex)
            {
                if (response != null)
                {
                    var statusCode = response.StatusCode;
                    var errorResponse = await response.Content.ReadAsStringAsync();

                    if (statusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                        throw new BadRequestException(error!.Message);
                    }
                    else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                    else if (statusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        throw new ServerErrorException();
                    }

                    throw new Exception($"Ошибка: {statusCode} - {errorResponse}");
                }
                else
                {
                    throw new Exception($"Ошибка HTTP: {ex.Message}");
                }
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка десериализации: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Выполняет возврат средств для указанного заказа.
        /// </summary>
        /// <param name="orderId">Идентификатор заказа (опционально).</param>
        /// <param name="paymentId">Идентификатор платежа (опционально).</param>
        /// <returns>Идентификатор возврата.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если не указаны orderId или paymentId.</exception>
        /// <exception cref="BadRequestException">Выбрасывается, если сервер вернул ошибку 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Выбрасывается, если сервер вернул ошибку 401 (Unauthorized).</exception>
        /// <exception cref="ServerErrorException">Выбрасывается, если сервер вернул ошибку 500 (Internal Server Error).</exception>
        /// <exception cref="ApiResponseException">Выбрасывается, если тип ответа не "success".</exception>
        /// <exception cref="Exception">Выбрасывается при возникновении других ошибок.</exception>
        /// <example>
        /// <code>
        /// var refundId = await freeKassaService.RefundOrderAsync(orderId: 123);
        /// Console.WriteLine($"Идентификатор возврата: {refundId}");
        /// </code>
        /// </example>
        public async Task<int> RefundOrderAsync(int? orderId = null, string? paymentId = null)
        {
            // Проверка входных параметров
            if (orderId == null && string.IsNullOrEmpty(paymentId))
            {
                throw new ArgumentException("Необходимо указать orderId и paymentId.");
            }

            // Проверка входных параметров
            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            // Формирование URI
            var requestUri = $"v1/order/refund?shopId={_shopId}&nonce={nonce}&signature={signature}&orderId={orderId}&paymentId={paymentId}";

            HttpResponseMessage? response = null;

            try
            {
                // Отправка POST-запроса
                response = await _httpClient.PostAsync(requestUri, null);

                // Проверка статуса ответа
                response.EnsureSuccessStatusCode();

                // Десериализация успешного ответа
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<RefundResponse>(responseBody);

                if (responseJson!.Type != "success")
                {
                    throw new ApiResponseException("Неизвестный тип ответа.");
                }

                return responseJson.Id;
            }
            catch (HttpRequestException ex)
            {
                if (response != null)
                {
                    var statusCode = response.StatusCode;
                    var errorResponse = await response.Content.ReadAsStringAsync();

                    if (statusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                        throw new BadRequestException(error!.Message);
                    }
                    else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                    else if (statusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        throw new ServerErrorException();
                    }

                    throw new Exception($"Ошибка: {statusCode} - {errorResponse}");
                }
                else
                {
                    throw new Exception($"Ошибка HTTP: {ex.Message}");
                }
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка десериализации: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}");
            }
        }

        #endregion

        #region Выплаты
        #endregion

        #region Разное

        /// <summary>
        /// Получает список доступных валют.
        /// </summary>
        /// <returns>Ответ, содержащий список валют.</returns>
        /// <exception cref="BadRequestException">Выбрасывается, если сервер вернул ошибку 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Выбрасывается, если сервер вернул ошибку 401 (Unauthorized).</exception>
        /// <exception cref="ServerErrorException">Выбрасывается, если сервер вернул ошибку 500 (Internal Server Error).</exception>
        /// <exception cref="ApiResponseException">Выбрасывается, если тип ответа не "success".</exception>
        /// <exception cref="Exception">Выбрасывается при возникновении других ошибок.</exception>
        /// <example>
        /// <code>
        /// var currenciesResponse = await freeKassaService.GetCurrenciesAsync();
        /// Console.WriteLine($"Доступные валюты: {string.Join(", ", currenciesResponse.Currencies)}");
        /// </code>
        /// </example>
        public async Task<CurrenciesResponse> GetCurrenciesAsync()
        {
            // Генерация подписи
            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            var requestUri = $"v1/currencies?shopId={_shopId}&nonce={nonce}&signature={signature}";

            HttpResponseMessage? response = null;

            try
            {
                // Отправка GET-запроса
                response = await _httpClient.GetAsync(requestUri.ToString());

                // Проверка статуса ответа
                response.EnsureSuccessStatusCode();

                // Десериализация успешного ответа
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<CurrenciesResponse>(responseBody);

                if (responseJson!.Type != "success")
                {
                    throw new ApiResponseException("Неизвестный тип ответа.");
                }

                return responseJson;
            }
            catch (HttpRequestException ex)
            {
                if (response != null)
                {
                    var statusCode = response.StatusCode;
                    var errorResponse = await response.Content.ReadAsStringAsync();

                    if (statusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                        throw new BadRequestException(error!.Message);
                    }
                    else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                    else if (statusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        throw new ServerErrorException();
                    }

                    throw new Exception($"Ошибка: {statusCode} - {errorResponse}");
                }
                else
                {
                    throw new Exception($"Ошибка HTTP: {ex.Message}");
                }
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка десериализации: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверяет статус валюты.
        /// </summary>
        /// <returns>Возвращает <c>true</c>, если статус валюты активен, иначе <c>false</c>.</returns>
        /// <exception cref="BadRequestException">Выбрасывается, если сервер вернул ошибку 400 (Bad Request).</exception>
        /// <exception cref="UnauthorizedException">Выбрасывается, если сервер вернул ошибку 401 (Unauthorized).</exception>
        /// <exception cref="ServerErrorException">Выбрасывается, если сервер вернул ошибку 500 (Internal Server Error).</exception>
        /// <exception cref="ApiResponseException">Выбрасывается, если тип ответа не "success".</exception>
        /// <exception cref="Exception">Выбрасывается при возникновении других ошибок.</exception>
        /// <example>
        /// <code>
        /// bool isCurrencyActive = await freeKassaService.CheckCurrencyStatusAsync();
        /// Console.WriteLine($"Статус валюты: {(isCurrencyActive ? "Активна" : "Неактивна")}");
        /// </code>
        /// </example>
        public async Task<bool> CheckCurrencyStatusAsync(int id)
        {
            // Генерация подписи
            long nonce = CurrentUnixTimeInMilliseconds();
            string signature = GenerateSignature(nonce);

            var requestUri = $"v1/currencies/{id}/status?shopId={_shopId}&nonce={nonce}&signature={signature}";

            HttpResponseMessage? response = null;

            try
            {
                // Отправка GET-запроса
                response = await _httpClient.GetAsync(requestUri.ToString());

                // Проверка статуса ответа
                response.EnsureSuccessStatusCode();

                // Десериализация успешного ответа
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<CheckCurrencyStatusResponse>(responseBody);

                // Возвращаем результат проверки
                return responseJson!.Type == "success";
            }
            catch (HttpRequestException ex)
            {
                if (response != null)
                {
                    var statusCode = response.StatusCode;
                    var errorResponse = await response.Content.ReadAsStringAsync();

                    if (statusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var error = JsonConvert.DeserializeObject<ApiErrorResponse>(errorResponse);
                        throw new BadRequestException(error!.Message);
                    }
                    else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                    else if (statusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        throw new ServerErrorException();
                    }

                    throw new Exception($"Ошибка: {statusCode} - {errorResponse}");
                }
                else
                {
                    throw new Exception($"Ошибка HTTP: {ex.Message}");
                }
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка десериализации: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}");
            }
        }

        #endregion
    }
}
