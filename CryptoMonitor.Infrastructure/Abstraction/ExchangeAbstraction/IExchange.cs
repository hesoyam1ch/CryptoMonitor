namespace CryptoMonitor.Infrastructure.Abstraction;

public interface IExchange
{
    string Name { get; init; }

    Task StartClientAsync();
    Task TestConnection();
    Task GetLastPriceAsync(string baseCurrency,string quoteCurrency);
    
}