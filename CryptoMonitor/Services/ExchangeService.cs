using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;

namespace CryptoMonitor.Services;

public class ExchangeService
{
    private readonly IEnumerable<ICexExchange> _cexExchanges;
    private readonly IEnumerable<IDexExchange> _dexExchanges;
    
    public ExchangeService(IEnumerable<ICexExchange> cexExchanges, IEnumerable<IDexExchange> dexExchanges)
    {
        _cexExchanges = cexExchanges;
        _dexExchanges = dexExchanges;
    }
    
    public async Task<List<object>> GetAllPricesAsync(string baseCurrency, string quoteCurrency)
    {
        var results = new List<object>();

        foreach (var exchange in _cexExchanges)
        {
            try
            {
                var price = await exchange.GetLastPriceAsync(baseCurrency, quoteCurrency);
                if (price.HasValue)
                {
                    results.Add(new { Exchange = exchange.GetType().Name, Price = price.Value });
                }
            }
            catch
            {
            }
        }

        foreach (var exchange in _dexExchanges)
        {
            try
            {
                var price = await exchange.GetLastPriceAsync(baseCurrency, quoteCurrency);
                if (price.HasValue)
                {
                    results.Add(new { Exchange = exchange.GetType().Name, Price = price.Value });
                }
            }
            catch
            {
            }
        }

        return results;
    }
     
}