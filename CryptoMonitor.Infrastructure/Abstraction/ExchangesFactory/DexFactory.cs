using CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

public enum DexEnum
{
    None = 0,
    Uniswap = 1,
    Raydium = 2,
}

public class DexFactory : IAbstractExchangeFactory<IExchange, DexEnum>
{
    private readonly Dictionary<DexEnum, IDexExchange> _exchanges;
    public DexFactory(IEnumerable<IDexExchange> exchanges)
    {
        _exchanges = exchanges.ToDictionary(e => GetExchangeType<DexEnum>(e));
    }
    public IExchange CreateExchange(DexEnum exchangeType)
    {
        if (_exchanges.TryGetValue(exchangeType, out var exchange))
        {
            return exchange;
        }

        throw new ArgumentException($"Invalid DEX type: {exchangeType}", nameof(exchangeType));
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