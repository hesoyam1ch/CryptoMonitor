using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Infrastructure.Models.DexExchangeModels;

public class UniswapExchange : IDexExchange
{
    public string Name { get; set; }

    public Task StartClient()
    {
        throw new NotImplementedException();
    }

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }
}