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
    
    public async Task<List<object>> GetAllPricesAsync(decimal tokenAmount, string baseCurrency, string quoteCurrency)
    {
        var results = new List<object>();
        var allExchanges = _cexExchanges.Cast<IExchange>().Concat(_dexExchanges);

        foreach (var exchange in allExchanges)
        {
            try
            {
                var price = await exchange.GetLastPriceAsync(baseCurrency, quoteCurrency);
                if (price.HasValue)
                {
                    decimal totalPrice = price.Value * tokenAmount;
                    results.Add(new 
                    { 
                        Exchange = exchange.GetType().Name, 
                        PricePerToken = price.Value, 
                        TokenAmount = tokenAmount, 
                        TotalPrice = totalPrice, 
                        In = quoteCurrency 
                    });
                }
            }
            catch
            {
            }
        }

        return results;
    }

    public async Task<object> EstimatePriceAsync(decimal tokenAmount, string baseCurrency, string quoteCurrency)
    {
        try
        {
            var resultList = await GetAllPricesAsync(tokenAmount, baseCurrency, quoteCurrency);
            if (resultList == null || resultList.Count == 0)
                return new { Message = "No price data available" };

            var bestResult = resultList.OrderBy(x => (decimal)x.GetType().GetProperty("TotalPrice")!.GetValue(x)!).FirstOrDefault();

            return bestResult;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
    
    public async Task<List<object>> GetRatesAsync(string baseCurrency, string quoteCurrency)
    {
        var results = new List<object>();
        var allExchanges = _cexExchanges.Cast<IExchange>().Concat(_dexExchanges);

        foreach (var exchange in allExchanges)
        {
            try
            {
                var price = await exchange.GetLastPriceAsync(baseCurrency, quoteCurrency);
                if (price.HasValue)
                {
                    results.Add(new 
                    { 
                        Exchange = exchange.GetType().Name, 
                        PricePerToken = price.Value, 
                        In = quoteCurrency 
                    });
                }
            }
            catch
            {
            }
        }

        return results;
    }

     
}