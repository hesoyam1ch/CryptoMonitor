using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using Kucoin.Net.Clients;

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

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }

    public KuCoinExchange()
    {
        _restClient = new KucoinRestClient();
        Name = "kucoin";
    }
    
}