using CryptoMonitor.Infrastructure.Abstraction;
using CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using CryptoMonitor.Infrastructure.Models.CexExchangesModels;
using CryptoMonitor.Infrastructure.Models.DexExchangeModels;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<ICexExchange, BinanceExchange>();
builder.Services.AddSingleton<ICexExchange, KuCoinExchange>();

builder.Services.AddSingleton<IDexExchange, RaydiumExchange>();
builder.Services.AddSingleton<IDexExchange, UniswapExchange>();

builder.Services.AddSingleton<IAbstractCexExchangeFactory<CexEnum>, CexFactory>();
builder.Services.AddSingleton<IAbstractDexExchangeFactory<DexEnum>, DexFactory>();
;

var serviceProvider = builder.Services.BuildServiceProvider();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

var cexFactory = serviceProvider.GetRequiredService<IAbstractCexExchangeFactory<CexEnum>>();
var dexFactory = serviceProvider.GetRequiredService<IAbstractDexExchangeFactory<DexEnum>>();
var binanceExchange = cexFactory.CreateCexExchange(CexEnum.Binance);
var kuCoinExchange = cexFactory.CreateCexExchange(CexEnum.KuCoin);
var raydiumExchange = dexFactory.CreateDexExchange(DexEnum.Raydium);
var uniswapExchange = dexFactory.CreateDexExchange(DexEnum.Uniswap);

await binanceExchange.StartClientAsync(); 
var kikResu = await kuCoinExchange.GetLastPriceAsync("ETH","BTC");
var priceResult = await binanceExchange.GetLastPriceAsync("ETH" ,"BTC");
await binanceExchange.UnsubscribeWebSocketConnectionsAsync();
await kuCoinExchange.UnsubscribeWebSocketConnectionsAsync();


await uniswapExchange.StartClientAsync();

Console.WriteLine("sdsd");

app.Run();




