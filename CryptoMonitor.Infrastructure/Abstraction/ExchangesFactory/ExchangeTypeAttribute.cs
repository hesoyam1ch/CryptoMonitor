namespace CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

[AttributeUsage(AttributeTargets.Class)]
public class ExchangeTypeAttribute : Attribute
{
    public object ExchangeType { get; }
    public ExchangeTypeAttribute(object exchangeType)
    {
        if (exchangeType is Enum)
        {
            ExchangeType = exchangeType;
        }
        else
        {
            throw new ArgumentException("No valid exchange type");
        }
    }
}