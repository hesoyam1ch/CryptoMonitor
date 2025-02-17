namespace CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

public interface IExchange
{
    string Name { get; init; }

    Task StartClientAsync();
    Task<decimal?> GetLastPriceAsync(string baseCurrency,string quoteCurrency);
}