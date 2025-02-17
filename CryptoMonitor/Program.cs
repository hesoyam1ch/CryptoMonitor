using CryptoMonitor.Infrastructure.Abstraction.AbstractExchangeFactory;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using CryptoMonitor.Infrastructure.ExchangeImplementation.CexExchanges;
using CryptoMonitor.Infrastructure.ExchangeImplementation.DexExchange;
using CryptoMonitor.Services;


var builder = WebApplication.CreateBuilder(args);

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


app.Run();




