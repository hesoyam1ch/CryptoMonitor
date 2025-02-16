using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;

namespace CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

public interface ICexExchange : IExchange
{
    Task UnsubscribeWebSocketConnectionsAsync();
    Task GetLastPriceRestAsync(string baseCurrency, string quoteCurrency) ;

}