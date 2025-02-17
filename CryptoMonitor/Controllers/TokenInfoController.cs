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
    
    [HttpGet("getAllTokenPrice")]
    public async Task<IActionResult> GetPriceFromAllExchange([FromQuery] string baseTokenCurrency, string quoteTokenCurrency)
    {
        try
        {    
            var result = await _exchangeService.GetAllPricesAsync(baseTokenCurrency,quoteTokenCurrency);
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    
    }
    
}