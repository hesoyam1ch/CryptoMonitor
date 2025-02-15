using CryptoExchange.Net.Authentication;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects;

namespace CryptoMonitor.Infrastructure.Models.CexExchangesModels;

[ExchangeType(CexEnum.KuCoin)]
public class KuCoinExchange : ICexExchange
{
    private KucoinRestClient _restClient;
    public string Name { get; init; }
    public async  Task StartClientAsync()
    {
       var result =  await _restClient.SpotApi.ExchangeData.GetMarketsAsync();
       Console.WriteLine($"{result.RequestBody}");
    }

    public KuCoinExchange SetApiCreaditials(KucoinApiCredentials apiCredentials)
    {
        _restClient.SetApiCredentials(apiCredentials);
        return this;
    }
    public Task TestConnection()
    {
        throw new NotImplementedException();
    }

    public async Task GetLastPriceAsync(string baseCurrency,string quoteCurrency) //can be abstract realisation 
    {
        var exchangeSymbolsInfo = await _restClient.SpotApi.ExchangeData.GetSymbolsAsync(); // need to one request when client start, and caching data
        var availableCurrency = exchangeSymbolsInfo?.Data;
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

    public Task SubscribeAndRunAsync()
    {
        throw new NotImplementedException();
    }

    public KuCoinExchange()
    {
        _restClient = new KucoinRestClient();
        Name = "kucoin";
    }
    
}