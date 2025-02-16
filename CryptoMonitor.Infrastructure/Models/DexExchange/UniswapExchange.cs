using System.Numerics;
using CryptoMonitor.Infrastructure.Abstraction.ExchangeAbstraction;
using CryptoMonitor.Infrastructure.Abstraction.ExchangesFactory;
using CryptoMonitor.Infrastructure.Models.DexExchangeModels.EthereumContractFunctions;
using Microsoft.Extensions.Configuration;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.Web3;


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
        string wethAddress = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2"; // to dict
        // string uniAddress = "0x1f9840a85d5af5bf1d1762f925bdaddc4201f984"; // to dict
        string usdcAddress = "0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48";
        var baseTokenDecimals = await GetDecimalsForTokenAsync(wethAddress);
        var quoteTokenDecimals = await GetDecimalsForTokenAsync(usdcAddress);
        int[] feeAvaibleAmount = new[] { 100, 500, 3000, 10000 };

        var liqudityPoolAddressV2 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunction>()
            .QueryAsync<string>(uniSwapFactoryAddressV2,
                new GetPairFunction() { TokenA = wethAddress, TokenB = usdcAddress });

        string liqudityPoolAddressV3 = "";

        for (int i = 0; i < feeAvaibleAmount.Length; i++)
        {
            liqudityPoolAddressV3 = await _web3Ws.Eth.GetContractQueryHandler<GetPairFunctionV3>()
                .QueryAsync<string>(uniSwapFactoryAddressV3,
                    new GetPairFunctionV3() { TokenA = wethAddress, TokenB = usdcAddress, FeeAmount = 3000 });
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
            CalculatePrices(slotResult.Tick, baseTokenDecimals, quoteTokenDecimals);
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
            CalculatePrices(reserves.Reserve0, reserves.Reserve1, baseTokenDecimals, quoteTokenDecimals);
        
        return (priceToken1, priceToken2);
    }


    private async Task<int> GetDecimalsForTokenAsync(string tokenAddress)
    {
        var tokenContract = _web3Ws.Eth.GetContract(@"
[
  {
    ""constant"": true,
    ""inputs"": [],
    ""name"": ""decimals"",
    ""outputs"": [
      {
        ""name"": """",
        ""type"": ""uint256""
      }
    ],
    ""payable"": false,
    ""stateMutability"": ""view"",
    ""type"": ""function""
  }
]
", tokenAddress);
        var decimalsFunction = tokenContract.GetFunction("decimals");

        var decimals = await decimalsFunction.CallAsync<int>();
        return decimals;
    }

    private (double priceToken1, double priceToken2) CalculatePrices(int tick, int baseTokenDecimals,
        int quoteTokenDecimals)
    {
        double priceAsDouble = Math.Pow(1.0001, tick);

        BigInteger priceBigInt = new BigInteger(priceAsDouble * Math.Pow(10, 18));
        BigInteger priceToken1Wei = priceBigInt / BigInteger.Pow(10, 18);
        BigInteger priceToken2Wei = BigInteger.Divide(BigInteger.Pow(10, 36), priceBigInt);
        BigInteger scaleToken1 = BigInteger.Pow(10, baseTokenDecimals - quoteTokenDecimals);
        BigInteger scaleToken2 = BigInteger.Pow(10, quoteTokenDecimals);

        double priceToken1 = (double)priceToken1Wei / (double)scaleToken1;
        double priceToken2 = (double)priceToken2Wei / (double)scaleToken2;

        return (priceToken1, priceToken2);
    }

    private (double price, double reversePrice) CalculatePrices(BigInteger reserve0, BigInteger reserve1,
        int baseTokenDecimals, int quoteTokenDecimals)
    {
        BigInteger reserve0Wei = reserve0;
        BigInteger reserve1Wei = reserve1;

        BigInteger scaleToken1 = BigInteger.Pow(10, baseTokenDecimals - quoteTokenDecimals);
        BigInteger scaleToken2 = BigInteger.Pow(10, quoteTokenDecimals);

        BigInteger priceBigInt = (reserve1Wei * BigInteger.Pow(10, 18)) / reserve0Wei;
        BigInteger priceToken1Wei = priceBigInt / BigInteger.Pow(10, 18);
        BigInteger priceToken2Wei = BigInteger.Divide(BigInteger.Pow(10, 36), priceBigInt);

        double priceToken1 = (double)priceToken1Wei / (double)scaleToken1;
        double priceToken2 = (double)priceToken2Wei / (double)scaleToken2;

        return (priceToken1, priceToken2);
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