using CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;

namespace CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

public enum DexEnum
{
    None =0,
    Uniswap =1,
    Raydium = 2,
}

public class DexFactory: IAbstractExchangeFactory<IExchange, DexEnum>
{
    public IExchange CreateExchange(DexEnum exchangeType)
    {
        throw new NotImplementedException();
    }
}