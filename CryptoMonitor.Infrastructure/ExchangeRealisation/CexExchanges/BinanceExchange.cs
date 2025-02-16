using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

namespace CryptoMonitor.Infrastructure.Models.CexExchangesModels;

[ExchangeType(CexEnum.Binance)]
public class BinanceExchange : ICexExchange
{
    public string Name { get; init; }
    private BinanceRestClient _restClient;
    private BinanceSocketClient _socketClient;
    private UpdateSubscription? _currentSubscription;
    private string _currentPair = "";
    private decimal _lastPrice;
    private TaskCompletionSource<decimal?> _priceUpdated;
    
    public BinanceExchange()
    {
        Name = "binance";
        _restClient = new BinanceRestClient();
        _socketClient = new BinanceSocketClient();
    }

    public BinanceExchange SetApiCreds(ApiCredentials apiCredentials)
    {
        _restClient.SetApiCredentials(apiCredentials);
        return this;
    }

    public async Task StartClientAsync()
    {
        var result = await _restClient.SpotApi.ExchangeData.PingAsync();

        Console.WriteLine($"{result.RequestBody}");
    }

    public async Task GetLastPriceRestAsync(string baseCurrency, string quoteCurrency) //can be abstract method  
    {
        var exchangeSymbolsInfo = await _restClient.SpotApi.ExchangeData.GetExchangeInfoAsync();
        var availableCurrency = exchangeSymbolsInfo?.Data?.Symbols;
        var pairCurrency = availableCurrency
            .First(s => s.BaseAsset.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase) &&
                        s.QuoteAsset.Equals(quoteCurrency, StringComparison.OrdinalIgnoreCase));


        if (pairCurrency != null)
        {
            var tickerResult = await _restClient.SpotApi.ExchangeData.GetTickerAsync(pairCurrency.Name);

            if (tickerResult.Success)
            {
                Console.WriteLine($"Price {pairCurrency.Name}: {tickerResult.Data.LastPrice}");
            }
            else
            {
                Console.WriteLine($"Can't get price for: {pairCurrency.Name}");
            }
        }
    }

    // public async Task<decimal?> GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    // {
    //     string pair = $"{baseCurrency}{quoteCurrency}".ToUpper();
    //
    //     if (_currentPair == pair && _currentSubscription != null)
    //     {
    //         Console.WriteLine($"Already subscribed to {pair}");
    //     }
    //
    //     if (_currentSubscription != null)
    //     {
    //         await _socketClient.UnsubscribeAsync(_currentSubscription);
    //         Console.WriteLine($"Unsubscribed from {_currentPair}");
    //     }
    //
    //     var subscribeResult = await _socketClient.SpotApi.ExchangeData.SubscribeToTickerUpdatesAsync(pair,
    //         data =>
    //         {
    //             Console.WriteLine($"Live Price Update for {pair}: {data.Data.LastPrice}");
    //             _lastPrice = data.Data.LastPrice;
    //             
    //         }
    //     );
    //
    //     if (subscribeResult.Success)
    //     {
    //         _currentSubscription = subscribeResult.Data;
    //         _currentPair = pair;
    //         Console.WriteLine($"Subscribed to {pair}");
    //     }
    //     else
    //     {
    //         Console.WriteLine($"Failed to subscribe to {pair}: {subscribeResult.Error}");
    //     }
    //
    //     return _lastPrice;
    // }

    public async Task<decimal?> GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    {
        string pair = $"{baseCurrency}{quoteCurrency}".ToUpper();
        if (_currentPair == pair && _currentSubscription != null)
        {
            Console.WriteLine($"Already subscribed to {pair}");
            return _lastPrice;
        }

        if (_currentSubscription != null)
        {
            await _socketClient.UnsubscribeAsync(_currentSubscription);
            Console.WriteLine($"Unsubscribed from {_currentPair}");
        }

        _priceUpdated = new TaskCompletionSource<decimal?>();

        var subscribeResult = await _socketClient.SpotApi.ExchangeData.SubscribeToTickerUpdatesAsync(pair,
            data =>
            {
                Console.WriteLine($"Live Price Update for {pair}: {data.Data.LastPrice}");
                _lastPrice = data.Data.LastPrice;

                _priceUpdated.TrySetResult(_lastPrice);
            }
        );

        if (subscribeResult.Success)
        {
            _currentSubscription = subscribeResult.Data;
            _currentPair = pair;
            Console.WriteLine($"Subscribed to {pair}");

            var completedTask = await Task.WhenAny(_priceUpdated.Task, Task.Delay(3000));
            if (completedTask == _priceUpdated.Task)
            {
                return _priceUpdated.Task.Result;
            }
            else
            {
                Console.WriteLine("Timeout waiting for price update.");
                return null;
            }
        }
        else
        {
            Console.WriteLine($"Failed to subscribe to {pair}: {subscribeResult.Error}");
            return null;
        }
    }
    public async Task CloseWebSocketAsync()
    {
        await _socketClient.UnsubscribeAllAsync();
    }


    public Task TestConnection()
    {
        throw new NotImplementedException();
    }
}