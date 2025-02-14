using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

namespace CryptoMonitor.Infrastructure.Models.DexExchangeModels;

[ExchangeType(DexEnum.Uniswap)]
public class UniswapExchange : IDexExchange
{
    public string Name { get; init; }

    public Task StartClientAsync()
    {
        throw new NotImplementedException();
    }

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }
}