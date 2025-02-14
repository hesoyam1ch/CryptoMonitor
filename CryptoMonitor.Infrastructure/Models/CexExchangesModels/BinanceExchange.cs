using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

namespace CryptoMonitor.Infrastructure.Models.CexExchangesModels;

[ExchangeType(CexEnum.Binance)]
public class BinanceExchange : ICexExchange
{
    private BinanceRestClient _restClient;
    public string Name { get; init; }

    public BinanceExchange()
    {
        _restClient = new BinanceRestClient();
        Name = "binance";
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

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }
}