using System.Numerics;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using CryptoMonitor.Infrastructure.ExchangeRealisation.Code;
using CryptoMonitor.Infrastructure.Models.DexExchangeModels.EthereumContractFunctions;
using Microsoft.Extensions.Configuration;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Web3;

namespace CryptoMonitor.Infrastructure.Models.DexExchangeModels;

[ExchangeType(DexEnum.Uniswap)]
public class UniswapExchange : IDexExchange
{
    private static readonly Dictionary<string, string> TokensSmartContract = new()
    {
        { "ETH", "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2" },
        { "USDC", "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48" },
        { "SOL", "0xD31a59c85aE9D8edEFeC411D448f90841571b89c" },
        { "WBTC", "0x2260FAC5E5542a773Aa44fBCfeDf7C193bc2C599" }
    };

    private const string uniSwapFactoryAddressV2 = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";
    private const string uniSwapFactoryAddressV3 = "0x1F98431c8aD98523631AE4a59f267346ea31F984";

    private string _webSocketUrl;
    private WebSocketClient _webSocketClient;
    private Web3 _web3Ws;
    public string Name { get; init; }

    public UniswapExchange(IConfiguration configuration)
    {
        _webSocketUrl = configuration.GetConnectionString("EthereumWssRpc");
        _webSocketClient = new WebSocketClient(_webSocketUrl);
        _web3Ws = new Web3(_webSocketUrl);
        Name = "Uniswap";
    }

    public async Task StartClientAsync()
    {
        string wethAddress = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2"; // to dict
        // string uniAddress = "0x1f9840a85d5af5bf1d1762f925bdaddc4201f984"; // to dict
        string usdcAddress = "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48";
        var baseTokenDecimals = await Utils.GetDecimalsForTokenAsync(wethAddress, _web3Ws);
        var quoteTokenDecimals = await Utils.GetDecimalsForTokenAsync(usdcAddress, _web3Ws);
        int[] feeAvaibleAmount = new[] { 100, 500, 3000, 10000 };

        var liqudityPoolAddressV2 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunction>()
            .QueryAsync<string>(uniSwapFactoryAddressV2,
                new GetPairFunction() { TokenA = wethAddress, TokenB = usdcAddress });

        string liqudityPoolAddressV3 = "";

        for (int i = 0; i < feeAvaibleAmount.Length; i++)
        {
            liqudityPoolAddressV3 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunctionV3>()
                .QueryAsync<string>(uniSwapFactoryAddressV3,
                    new GetPairFunctionV3()
                        { TokenA = wethAddress, TokenB = usdcAddress, FeeAmount = feeAvaibleAmount[i] });
            if (!string.IsNullOrWhiteSpace(liqudityPoolAddressV3) ||
                liqudityPoolAddressV3 != "0xA000000000000000000000000000000000000000")
            {
                break;
            }
        }


        await GetPriceFromPoolV3Async(liqudityPoolAddressV3, baseTokenDecimals, quoteTokenDecimals);
        await GetPriceFromPoolV2Async(liqudityPoolAddressV2, baseTokenDecimals, quoteTokenDecimals);
    }

    private async Task<(double inBaseToken, double inQuoteToken)> GetPriceFromPoolV3Async(string poolContractAddress,
        int baseTokenDecimals, int quoteTokenDecimals)
    {
        var slotResult = await _web3Ws.Eth.GetContractQueryHandler<SlotFunction>()
            .QueryAsync<Slot0OutputDTO>(poolContractAddress, new SlotFunction());

        (double priceToken1, double priceToken2) =
            Utils.CalculatePrices(slotResult.Tick, baseTokenDecimals, quoteTokenDecimals);
        return (priceToken1, priceToken2);
    }

    private async Task<(double inBaseToken, double inQuoteToken)> GetPriceFromPoolV2Async(string poolContractAddress,
        int baseTokenDecimals, int quoteTokenDecimals)
    {
        var contract = _web3Ws.Eth.GetContract(@"[
  {
    ""constant"": true,
    ""inputs"": [],
    ""name"": ""getReserves"",
    ""outputs"": [
      {
        ""internalType"": ""uint112"",
        ""name"": ""_reserve0"",
        ""type"": ""uint112""
      },
      {
        ""internalType"": ""uint112"",
        ""name"": ""_reserve1"",
        ""type"": ""uint112""
      },
      {
        ""internalType"": ""uint32"",
        ""name"": ""_blockTimestampLast"",
        ""type"": ""uint32""
      }
    ],
    ""payable"": false,
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""constant"": true,
    ""inputs"": [],
    ""name"": ""token0"",
    ""outputs"": [
      {
        ""internalType"": ""address"",
        ""name"": """",
        ""type"": ""address""
      }
    ],
    ""payable"": false,
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""constant"": true,
    ""inputs"": [],
    ""name"": ""token1"",
    ""outputs"": [
      {
        ""internalType"": ""address"",
        ""name"": """",
        ""type"": ""address""
      }
    ],
    ""payable"": false,
    ""stateMutability"": ""view"",
    ""type"": ""function""
  }
]
", poolContractAddress);
        var getReservesFunction = contract.GetFunction("getReserves");
        var reserves = await getReservesFunction.CallDeserializingToObjectAsync<ReservesDTO>();

        (double priceToken1, double priceToken2) =
            Utils.CalculatePrices(reserves.Reserve0, reserves.Reserve1, baseTokenDecimals, quoteTokenDecimals);

        return (priceToken1, priceToken2);
    }


    public Task TestConnection()
    {
        throw new NotImplementedException();
    }

    public Task<decimal?> GetLastPriceAsync(string baseCurrency, string quoteCurrency)
    {
        throw new NotImplementedException();
    }
    
}