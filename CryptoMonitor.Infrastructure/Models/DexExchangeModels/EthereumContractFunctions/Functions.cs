﻿using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace CryptoMonitor.Infrastructure.Models.DexExchangeModels.EthereumContractFunctions;

[Function("slot0", typeof(Slot0OutputDTO))]
public class SlotFunction : FunctionMessage
{
    
}


[Function("getPool", "address")]
public class GetPairFunctionV3 : FunctionMessage
{
    [Parameter("address", "tokenA", 1)] public string TokenA { get; set; }
    [Parameter("address", "tokenB", 2)] public string TokenB { get; set; }
    [Parameter("uint24", "fee", 3)] public int FeeAmount { get; set; }
}

[Function("getPair", "address")]
public class GetPairFunction : FunctionMessage
{
    [Parameter("address", "tokenA", 1)] public virtual string TokenA { get; set; }
    [Parameter("address", "tokenB", 2)] public virtual string TokenB { get; set; }
}

[FunctionOutput]
public class Slot0OutputDTO : IFunctionOutputDTO
{
    [Parameter("uint160", "sqrtPriceX96", 1)]
    public BigInteger SqrtPriceX96 { get; set; }

    [Parameter("int24", "tick", 2)]
    public int Tick { get; set; }

    [Parameter("uint16", "observationIndex", 3)]
    public ushort ObservationIndex { get; set; }

    [Parameter("uint16", "observationCardinality", 4)]
    public ushort ObservationCardinality { get; set; }

    [Parameter("uint16", "observationCardinalityNext", 5)]
    public ushort ObservationCardinalityNext { get; set; }

    [Parameter("uint8", "feeProtocol", 6)]
    public byte FeeProtocol { get; set; }

    [Parameter("bool", "unlocked", 7)]
    public bool Unlocked { get; set; }
}