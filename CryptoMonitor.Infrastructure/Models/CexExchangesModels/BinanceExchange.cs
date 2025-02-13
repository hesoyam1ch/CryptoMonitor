using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Infrastructure.Models.CexExchangesModels;

public class BinanceExchange : ICexExchange
{
    private BinanceRestClient _restClient;
    public string Name { get; set; }

    public BinanceExchange()
    {
        _restClient = new BinanceRestClient();
    }

    public BinanceExchange SetApiCreds(ApiCredentials apiCredentials)
    {
        _restClient.SetApiCredentials(apiCredentials);
        return this;
    }
    public async Task StartClient()
    {
        await _restClient.SpotApi.ExchangeData.PingAsync();
        
    }

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }
}