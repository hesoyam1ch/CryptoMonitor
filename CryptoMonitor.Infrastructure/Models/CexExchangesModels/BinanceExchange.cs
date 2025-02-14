using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

namespace CryptoMonitor.Infrastructure.Models.CexExchangesModels;

[ExchangeType(CexEnum.Binance)]
public class BinanceExchange : ICexExchange
{
    public string Name { get; init; }
    private BinanceRestClient _restClient;

    public BinanceExchange()
    {
        Name = "binance";
        _restClient = new BinanceRestClient();
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

    public async Task GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    {
        var exchangeSymbolsInfo = await _restClient.SpotApi.ExchangeData.GetExchangeInfoAsync();
        var availableCurrency = exchangeSymbolsInfo?.Data?.Symbols;
        var pairCurrency = availableCurrency
            .First(s => s.BaseAsset.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase) && 
                        s.QuoteAsset.Equals(quoteCurrency,StringComparison.OrdinalIgnoreCase));
        

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

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }
}