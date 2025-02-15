using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
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
        string uniSwapFactoryAddressV2 = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";
        string uniSwapFactoryAddressV3 = "0x1F98431c8aD98523631AE4a59f267346ea31F984";
        string uniAddress = "0x1f9840a85d5af5bf1d1762f925bdaddc4201f984";
        string wethAddress = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2";
        int[] feeAvaibleAmount = new[] { 100, 500, 3000, 10000 }; 
        
        var pairContractAddressV2 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunction>()
            .QueryAsync<string>(uniSwapFactoryAddressV2,
                new GetPairFunction() { TokenA = wethAddress, TokenB = uniAddress });


        string pairContractAddressV3 = "";
      //for fee..
             pairContractAddressV3 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunctionV3>()
                .QueryAsync<string>(uniSwapFactoryAddressV3,
                    new GetPairFunctionV3() { TokenA = wethAddress, TokenB = uniAddress ,FeeAmount =3000 });
        
        var filter = Event<PairSyncEventDTO>.GetEventABI()
            .CreateFilterInput(new[] {  pairContractAddressV3});

        var subscription = new EthLogsObservableSubscription(_webSocketClient);
        subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
        {
            try
            {
                EventLog<PairSyncEventDTO> decoded = Event<PairSyncEventDTO>.DecodeEvent(log);
                if (decoded != null)
                {
                    decimal reserve0 = Web3.Convert.FromWei(decoded.Event.Reserve0);
                    decimal reserve1 = Web3.Convert.FromWei(decoded.Event.Reserve1);
                    Console.WriteLine($@"Price={reserve0 / reserve1}");
                }
                else Console.WriteLine(@"Found not standard transfer log");
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Log Address: " + log.Address + @" is not a standard transfer log:", ex.Message);
            }
        });

        await _webSocketClient.StartAsync();
        subscription.GetSubscribeResponseAsObservable().Subscribe(id => Console.WriteLine($"Subscribed with id: {id}"));
        await subscription.SubscribeAsync(filter);

        while (true)
        {
            var handler = new EthBlockNumberObservableHandler(_webSocketClient);
            handler.GetResponseAsObservable().Subscribe(x => Console.WriteLine(x.Value));
            await handler.SendRequestAsync();
            await Task.Delay(500);
        }
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

    [Event("Sync")]
    class PairSyncEventDTO : IEventDTO
    {
        [Parameter("uint112", "reserve0")] public virtual BigInteger Reserve0 { get; set; }

        [Parameter("uint112", "reserve1", 2)] public virtual BigInteger Reserve1 { get; set; }
    }


    public partial class GetPairFunction : GetPairFunctionBase
    {
    }

    
    [Function("getPool", "address")]
    public class GetPairFunctionV3 : FunctionMessage
    {
        [Parameter("address", "tokenA", 1)] public  string TokenA { get; set; }
        [Parameter("address", "tokenB", 2)] public  string TokenB { get; set; }
        [Parameter("uint24", "fee", 3)] public  int FeeAmount { get; set; }
    }

    [Function("getPair", "address")]
    public class GetPairFunctionBase : FunctionMessage
    {
        [Parameter("address", "tokenA", 1)] public virtual string TokenA { get; set; }
        [Parameter("address", "tokenB", 2)] public virtual string TokenB { get; set; }
    }


    public partial class SwapEventDTO : SwapEventDTOBase
    {
    }

    [Event("Swap")]
    public class SwapEventDTOBase : IEventDTO
    {
        [Parameter("address", "sender", 1, true)]
        public virtual string Sender { get; set; }

        [Parameter("uint256", "amount0In", 2, false)]
        public virtual BigInteger Amount0In { get; set; }

        [Parameter("uint256", "amount1In", 3, false)]
        public virtual BigInteger Amount1In { get; set; }

        [Parameter("uint256", "amount0Out", 4, false)]
        public virtual BigInteger Amount0Out { get; set; }

        [Parameter("uint256", "amount1Out", 5, false)]
        public virtual BigInteger Amount1Out { get; set; }

        [Parameter("address", "to", 6, true)] public virtual string To { get; set; }
    }
}