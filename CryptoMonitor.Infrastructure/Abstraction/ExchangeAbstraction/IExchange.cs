namespace CryptoMonitor.Infrastructure.Abstraction;

public interface IExchange
{
    string Name { get; set; }

    Task StartClient();
    Task TestConnection();
    
}