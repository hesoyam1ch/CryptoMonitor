using CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

public enum CexEnum
{
    None =0,
    Binance =1,
    KuCoin = 2,
}

public class CexFactory : IAbstractCexExchangeFactory<CexEnum>
{
    private readonly Dictionary<CexEnum, ICexExchange> _exchanges;
    
    public CexFactory(IEnumerable<ICexExchange> exchanges)
    {
        _exchanges = exchanges.ToDictionary(e => GetExchangeType<CexEnum>(e));
    }

    
    public ICexExchange CreateCexExchange(CexEnum exchangeType)
    {
        if (_exchanges.TryGetValue(exchangeType, out var exchange))
        {
            return exchange;
        }

        throw new ArgumentException($"Invalid CEX type: {exchangeType}", nameof(exchangeType));
    }

    
    private static TEnum GetExchangeType<TEnum>(IExchange exchange) where TEnum : Enum
    {
        var attribute = (ExchangeTypeAttribute?)Attribute.GetCustomAttribute(exchange.GetType(), typeof(ExchangeTypeAttribute));
        if (attribute?.ExchangeType is TEnum enumValue)
        {
            return enumValue;
        }
        throw new ArgumentException($"Exchange {exchange.GetType().Name} is missing ExchangeTypeAttribute or has an invalid type.");
    }


   
}