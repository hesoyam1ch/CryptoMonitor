using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;

public interface IAbstractDexExchangeFactory<DexEnum>  
{
    IDexExchange CreateDexExchange(DexEnum exchangeType);

}