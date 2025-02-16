using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects;

namespace CryptoMonitor.Infrastructure.Models.CexExchangesModels;

[ExchangeType(CexEnum.KuCoin)]
public class KuCoinExchange : ICexExchange
{
    private KucoinRestClient _restClient;
    public KucoinSocketClient _socketClient;
    private UpdateSubscription? _currentSubscription;
    private string _currentPair = "";
    private decimal _lastPrice;
    private TaskCompletionSource<decimal?> _priceUpdated;
    public string Name { get; init; }

    public KuCoinExchange()
    {
        _restClient = new KucoinRestClient();
        _socketClient = new KucoinSocketClient();
        Name = "kucoin";
    }

    public async Task StartClientAsync()
    {
        var result = await _restClient.SpotApi.ExchangeData.GetMarketsAsync();
        Console.WriteLine($"{result.RequestBody}");
    }

    public async Task<decimal?> GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    {
        var symbolsResult = await _restClient.SpotApi.ExchangeData.GetSymbolsAsync();
        if (!symbolsResult.Success)
        {
            throw new Exception($"Failed to get symbols: {symbolsResult.Error}");
        }

        var pairCurrency = symbolsResult.Data.FirstOrDefault(s => 
            s.BaseAsset.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase) &&
            s.QuoteAsset.Equals(quoteCurrency, StringComparison.OrdinalIgnoreCase));

        if (pairCurrency == null)
        {
            throw new Exception($"Trading pair {baseCurrency}-{quoteCurrency} not found on KuCoin.");
        }

        string pair = $"{pairCurrency.BaseAsset}-{pairCurrency.QuoteAsset}".ToUpper();

        if (_currentSubscription != null)
        {
            await _socketClient.UnsubscribeAsync(_currentSubscription);
            Console.WriteLine($"Unsubscribed from {_currentPair}");
        }

        _priceUpdated = new TaskCompletionSource<decimal?>();

        var subscribeResult = await _socketClient.SpotApi.SubscribeToTickerUpdatesAsync(pair,
            data =>
            {
                Console.WriteLine($"Live Price Update for {pair}: {data.Data.LastPrice}");
                _lastPrice = (decimal)data.Data.LastPrice;
                _priceUpdated.TrySetResult(_lastPrice);
            }
        );

        if (subscribeResult.Success)
        {
            _currentSubscription = subscribeResult.Data;
            _currentPair = pair;
            Console.WriteLine($"Subscribed to {pair}");
            
            var completedTask = await Task.WhenAny(_priceUpdated.Task, Task.Delay(5000));
            if (completedTask == _priceUpdated.Task)
            {
                return _priceUpdated.Task.Result;
            }
        }

        throw new Exception("Can't get price");
    }

    public async Task GetLastPriceRestAsync(string baseCurrency, string quoteCurrency) 
    {
        var exchangeSymbolsInfo =
            await _restClient.SpotApi.ExchangeData
                .GetSymbolsAsync(); // need to one request when client start, and caching data
        var availableCurrency = exchangeSymbolsInfo?.Data;
        var pairCurrency = availableCurrency
            .First(s => s.BaseAsset.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase) &&
                        s.QuoteAsset.Equals(quoteCurrency, StringComparison.OrdinalIgnoreCase));


        if (pairCurrency != null)
        {
            var tickerResult = await _restClient.SpotApi.ExchangeData.GetTickerAsync(pairCurrency.Name);

            if (tickerResult.Success)
            {
                Console.WriteLine($"Price {pairCurrency.Name}: {tickerResult.Data.LastPrice}");
               // return tickerResult.Data.LastPrice;
            }
            else
            {
                Console.WriteLine($"Can't get price for: {pairCurrency.Name}");
            }
        }

        throw new Exception();
    }

    public async Task UnsubscribeWebSocketConnectionsAsync()
    {
        await _socketClient.UnsubscribeAllAsync();
    }
}