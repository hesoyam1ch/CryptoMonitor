using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using CryptoMonitor.Infrastructure.Models.DexExchangeModels.EthereumContractFunctions;
using Microsoft.Extensions.Configuration;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using BigInteger = System.Numerics.BigInteger;

namespace CryptoMonitor.Infrastructure.Models.DexExchangeModels;

[ExchangeType(DexEnum.Uniswap)]
public class UniswapExchange : IDexExchange
{
    private const string uniSwapFactoryAddressV2 = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";
    private const string uniSwapFactoryAddressV3 = "0x1F98431c8aD98523631AE4a59f267346ea31F984";

    public Dictionary<string, string> _tokenContracts;
    private string _webSocketUrl;
    private StreamingWebSocketClient _webSocketClient;
    private Web3 _web3Ws;
    public string Name { get; init; }

    public UniswapExchange(IConfiguration configuration)
    {
        _webSocketUrl = configuration.GetConnectionString("EthereumWssRpc");
        _webSocketClient = new StreamingWebSocketClient(_webSocketUrl);
        _web3Ws = new Web3(_webSocketUrl);
        Name = "Uniswap";
    }

    public async Task StartClientAsync()
    {
        string uniAddress = "0x1f9840a85d5af5bf1d1762f925bdaddc4201f984"; // to dict
        string wethAddress = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2"; // to dict

        int[] feeAvaibleAmount = new[] { 100, 500, 3000, 10000 };

        var pairContractAddressV2 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunction>()
            .QueryAsync<string>(uniSwapFactoryAddressV2,
                new GetPairFunction() { TokenA = wethAddress, TokenB = uniAddress });


        string pairContractAddressV3 = "";
        //for fee..
        pairContractAddressV3 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunctionV3>()
            .QueryAsync<string>(uniSwapFactoryAddressV3,
                new GetPairFunctionV3() { TokenA = wethAddress, TokenB = uniAddress, FeeAmount = 3000 });

        await GetPriceFromPoolV3(pairContractAddressV3);
    }

    private async Task GetPriceFromPoolV3(string poolContractAddress)
    {
        var slotValue = await _web3Ws.Eth.GetContractQueryHandler<SlotFunction>()
            .QueryAsync<Slot0OutputDTO>(poolContractAddress, new SlotFunction());
        BigInteger numerator = BigInteger.Pow(slotValue.SqrtPriceX96, 2);
        BigInteger denominator = BigInteger.Pow(2, 192);

        double price = Math.Pow(1.0001, slotValue.Tick);
        double reversePrice = 1 / price;
        double adjustedPrice = price * Math.Pow(10, 18 - 18);

        Console.WriteLine($"Exact Price (Uniswap V3): {price}");


        double poolFee = 0.003; // 0.25% для Uniswap (можна замінити на змінну для V3)
        double priceWithPoolFee = price * (1 + poolFee);
        double priceWithAdjuctedPoolFee = reversePrice * (1 + poolFee);
        Console.WriteLine($"Price with pool fee: {priceWithPoolFee}");
    }

    public Task TestConnection()
    {
        throw new NotImplementedException();
    }

    public Task GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    {
        throw new NotImplementedException();
    }

    public Task SubscribeAndRunAsync()
    {
        throw new NotImplementedException();
    }
}