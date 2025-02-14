
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

builder.Services.AddSingleton<IAbstractExchangeFactory<IExchange, CexEnum>, CexFactory>();
builder.Services.AddSingleton<IAbstractExchangeFactory<IExchange, DexEnum>, DexFactory>();

var serviceProvider = builder.Services.BuildServiceProvider();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

var cexFactory = serviceProvider.GetRequiredService<IAbstractExchangeFactory<IExchange, CexEnum>>();
var dexFactory = serviceProvider.GetRequiredService<IAbstractExchangeFactory<IExchange, DexEnum>>();
var binanceExchange = cexFactory.CreateExchange(CexEnum.Binance);
var kuCoinExchange = cexFactory.CreateExchange(CexEnum.KuCoin);
var raydiumExchange = dexFactory.CreateExchange(DexEnum.Raydium);
var uniswapExchange = dexFactory.CreateExchange(DexEnum.Uniswap);

await binanceExchange.StartClientAsync();
await kuCoinExchange.StartClientAsync();
await raydiumExchange.StartClientAsync();
await uniswapExchange.StartClientAsync();

Console.WriteLine("sdsd");

app.Run();




