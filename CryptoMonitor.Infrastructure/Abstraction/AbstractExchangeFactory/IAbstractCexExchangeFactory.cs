using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;

public interface IAbstractCexExchangeFactory<CexEnum> 
{
    ICexExchange CreateCexExchange(CexEnum exchangeType);
}
