# FreeKassa.COM
> Неофициальная реализация интеграции к сервису [FreeKassa.com](https://FreeKassa.com "Перейти на сайт FreeKassa.com")

## О сервисе
FreeKassa — это система онлайн-платежей, которая позволяет интернет-магазинам и другим коммерческим ресурсам принимать платежи через различные методы, включая карты и электронные деньги. Она предлагает более ста способов оплаты и подходит как для частных лиц, так и для бизнеса.

## Фукнционал и его реализация
|Описание|Состояние|
|-|:-:|
|Список заказов|☑️|
|Создать заказ и получить ссылку на оплату|☑️|
|Возврат|☑️|
|Список выплат|❌|
|Создать выплату|❌|
|Получение баланса|❌|
|Получение списка доступных платежных систем|☑️|
|Проверка доступности платежной системы для оплаты|☑️|
|Получение списка доступных платежных систем для вывода|❌|
|Получение списка Ваших магазинов|❌|

## Пример использования
> Консольное приложение
``` csharp
var freeKassaService = new FreeKassaService(new HttpClient(), "YOUR_API_KEY", "YOUR_SHOP_ID");

try
{
    var request = new CreateOrderRequest
    {
         PaymentSystemId = 1,
         Email = "user@example.com",
         Amount = 100.50m,
         Currency = "RUB"
     };
      
     var paymentUrl = await freeKassaService.CreateOrderAsync(request);
     Console.WriteLine($"Ссылка на оплату: {paymentUrl}");
}
catch (BadRequestException ex)
{
    Console.WriteLine($"Ошибка 400: {ex.Message}");
}
catch (UnauthorizedException)
{
    Console.WriteLine("Ошибка 401: Неавторизованный доступ.");
}
catch (ServerErrorException)
{
    Console.WriteLine("Ошибка 500: Внутренняя ошибка сервера.");
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла ошибка: {ex.Message}");
}
```

> Пример подключения в Web приложение
``` csharp
builder.Services.AddHttpClient<IFreeKassaService, FreeKassaService>((provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri("https://api.freekassa.com/");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseDefaultCredentials = true
});

builder.Services.AddScoped<IFreeKassaService, FreeKassaService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var httpClient = provider.GetRequiredService<HttpClient>();
    return new FreeKassaService(httpClient, configuration["FreeKassa:ApiKey"], configuration["FreeKassa:ShopId"]);
});
```

## Пожертвование
![Купить мне кофе](https://s.iimg.su/s/11/wYvtLkHlymFwTUfR39s06J3dRdaHKqKCW9urBN0s.png) [Купите мне кофе](https://donate.stream/malomalsky "Купить кофе")
