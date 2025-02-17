using CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using CryptoMonitor.Infrastructure.ExchangeImplementation.CexExchanges;
using CryptoMonitor.Infrastructure.ExchangeImplementation.DexExchange;
using CryptoMonitor.Services;


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
builder.Services.AddScoped<ExchangeService>();


var serviceProvider = builder.Services.BuildServiceProvider();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();

// var cexFactory = serviceProvider.GetRequiredService<IAbstractCexExchangeFactory<CexEnum>>();
// var dexFactory = serviceProvider.GetRequiredService<IAbstractDexExchangeFactory<DexEnum>>();
// var binanceExchange = cexFactory.CreateCexExchange(CexEnum.Binance);
// var kuCoinExchange = cexFactory.CreateCexExchange(CexEnum.KuCoin);
// var raydiumExchange = dexFactory.CreateDexExchange(DexEnum.Raydium);
// var uniswapExchange = dexFactory.CreateDexExchange(DexEnum.Uniswap);
//
// await binanceExchange.StartClientAsync();
// try
// {
//     var kikResu = await kuCoinExchange.GetLastPriceAsync("ETH","BTC");
//     var kikRess = await kuCoinExchange.GetLastPriceAsync("ETH","USDT");
//     var kikResudd4 = await kuCoinExchange.GetLastPriceAsync("SOL","USDT");
//     var priceResult = await binanceExchange.GetLastPriceAsync("ETH" ,"BTC");
//     await binanceExchange.UnsubscribeWebSocketConnectionsAsync();
//     await kuCoinExchange.UnsubscribeWebSocketConnectionsAsync();
//     var btcUNI = await uniswapExchange.GetLastPriceAsync("BTC","ETH");
//     var btcUfdNI = await uniswapExchange.GetLastPriceAsync("BTC","USDT");
//     var btcUNId = await uniswapExchange.GetLastPriceAsync("ETH","BTC");
//     var EthUNI = await uniswapExchange.GetLastPriceAsync("ETH","USDT");
//     var sold = await uniswapExchange.GetLastPriceAsync("ETH","SOL");
//
//     Console.WriteLine("sds");
// }
// catch (Exception e)
// {
//     Console.WriteLine(e);
// }



app.Run();




