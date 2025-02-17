using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

namespace CryptoMonitor.Infrastructure.ExchangeImplementation.DexExchange;

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


    public Task<decimal?> GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    {
        throw new NotImplementedException();
    }

  
}