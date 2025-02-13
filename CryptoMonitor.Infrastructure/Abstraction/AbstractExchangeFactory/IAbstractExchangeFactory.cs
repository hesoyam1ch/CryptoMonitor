using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;

public interface IAbstractExchangeFactory<T, TEnum> where T : IExchange
{
    T CreateExchange(TEnum exchangeType);
}
