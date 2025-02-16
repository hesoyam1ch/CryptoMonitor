using CryptoMonitor.Infrastructure.Abstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

namespace CryptoMonitor.Infrastructure.Models.DexExchangeModels;

[ExchangeType(DexEnum.Raydium)]
public class RaydiumExchange : IDexExchange
{
    public string Name { get; init; }

    public RaydiumExchange()
    {
        Name = "raydium";
    }
    public Task StartClientAsync()
    {
        throw new NotImplementedException();
    }

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }

    public Task GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    {
        throw new NotImplementedException();
    }

  
}