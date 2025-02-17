using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMonitor.Controllers;



[ApiController]
[Route("api/tokenInfo")]
public class TokenInfoController : ControllerBase
{
    private ExchangeService _exchangeService;
    public TokenInfoController(ExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }
    
    [HttpGet("getRates")]
    public async Task<IActionResult> GetRatesAsync([FromQuery]string baseTokenCurrency, string quoteTokenCurrency)
    {
        try
        {    
            var result = await _exchangeService.GetRatesAsync( baseTokenCurrency,quoteTokenCurrency);
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    
    }
    
    [HttpGet("estimate")]

    public async Task<IActionResult> EstimateAsync([FromQuery]decimal tokenAmount ,string baseTokenCurrency, string quoteTokenCurrency)
    {
        try
        {    
            var result = await _exchangeService.EstimatePriceAsync(tokenAmount, baseTokenCurrency,quoteTokenCurrency); 
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    
    }
    
}