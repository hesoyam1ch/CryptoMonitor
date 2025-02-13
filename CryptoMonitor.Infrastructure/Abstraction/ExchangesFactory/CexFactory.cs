using CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Models.CexExchangesModels;

namespace CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

public enum CexEnum
{
    None =0,
    Binance =1,
    KuCoin = 2,
}

public class CexFactory : IAbstractExchangeFactory<IExchange, CexEnum>
{
    private readonly Dictionary<CexEnum, ICexExchange> _exchanges;

    public CexFactory(IEnumerable<ICexExchange> exchanges)
    {
        _exchanges = exchanges.ToDictionary(e => (CexEnum)Enum.Parse(typeof(CexEnum), e.GetType().Name));
    }
    
    public IExchange CreateExchange(CexEnum exchangeType)
    {
        return exchangeType switch
        {
            CexEnum.Binance => new BinanceExchange(),
            _ => throw new ArgumentException("Invalid CEX type", nameof(exchangeType))
        };

    }

   
}