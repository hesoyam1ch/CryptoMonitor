using System.Numerics;
using Nethereum.Web3;

namespace CryptoMonitor.Infrastructure.ExchangeRealisation.Code;

public static class Utils
{
    public static async Task<int> GetDecimalsForTokenAsync(string tokenAddress, Web3 web3)
    {
        var tokenContract = web3.Eth.GetContract(@"
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
    
    public static (double priceToken1, double priceToken2) CalculatePrices(int tick, int baseTokenDecimals,
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

    public static (double price, double reversePrice) CalculatePrices(BigInteger reserve0, BigInteger reserve1,
      int baseTokenDecimals, int quoteTokenDecimals)
    {
        
      BigInteger scaleToken1 = BigInteger.Pow(10, baseTokenDecimals - quoteTokenDecimals);
      BigInteger scaleToken2 = BigInteger.Pow(10, quoteTokenDecimals);

      BigInteger priceBigInt = (reserve1 * BigInteger.Pow(10, 18)) / reserve0;
      BigInteger priceToken1Wei = priceBigInt / BigInteger.Pow(10, 18);
      BigInteger priceToken2Wei = BigInteger.Divide(BigInteger.Pow(10, 36), priceBigInt);

      double priceToken1 = (double)priceToken1Wei / (double)scaleToken1;
      double priceToken2 = (double)priceToken2Wei / (double)scaleToken2;

      return (priceToken1, priceToken2);
    }
    
    public static string GetTokenContractAddress(Dictionary<string,string> TokensSmartContract,string tokenSymbol)
    {
      tokenSymbol = tokenSymbol.ToUpper();
      if (TokensSmartContract.TryGetValue(tokenSymbol, out string address))
      {
        return address;
      }
    
      throw new Exception($"Token {tokenSymbol} not found in the smart contracts dictionary.");
    }

}