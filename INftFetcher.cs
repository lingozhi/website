namespace DeswapApp;

using Nethereum.Web3;
using System.Numerics;

public interface INftFetcher
{
    Task RefreshMetadata(string contractAddress, long chainId, long tokenId);

    Task<IList<UserOptionNFT>> GetUserOptionTokens(string userAddress, long chainId);

    Task<IList<UserLotteryNFT>> GetUserLotteryTokens(string userAddress, long chainId);

    Task<IList<UserRedEnvelopeNFT>> GetUserRedEnvelopeTokens(string userAddress, long chainId);

    Task<IList<UserRouletteNFT>> GetUserRouletteTokens(string userAddress, long chainId);
}

public class UserOptionNFT
{
    public long TokenId { get; set; }

    public long ChainId { get; set; }

    public required string Contract { get; set; }

    public required string ImageData { get; set; }

    public DateTimeOffset MaturityDate { get; set; }

    public OptionsKind OptionsKind { get; set; }

    public decimal BaseAssetAmount { get; set; }

    public decimal QuoteAssetAmount { get; set; }

    public decimal Price { get; set; }

    public bool Listable()
    {
        if (!Burnable() && !Exercisable())
        {
            return true;
        }
        return false;
    }

    public bool Burnable()
    {
        var now = DateTime.Now;
        if (now >= MaturityDate.AddDays(1))
        {
            return true;
        }
        return false;
    }

    public bool Exercisable()
    {
        var now = DateTime.Now;
        // TODO: 方便测试暂且注释，上线时解除
        // if (now >= MaturityDate && now < MaturityDate.AddDays(1))
        // {
        //     return true;
        // }
        if (now < MaturityDate.AddDays(1))
        {
            return true;
        }
        return false;
    }

    public (string, BigInteger) GetPayAsset()
    {
        var tokenPair = OptionsContracts.FilterByChainId(ChainId)
            .Where(item => item.NftAddress.Equals(Contract, StringComparison.CurrentCultureIgnoreCase))
            .First();
        if (OptionsKind == OptionsKind.CALL)
        {
            return (tokenPair.QuoteAssetAddress, Web3.Convert.ToWei(QuoteAssetAmount, tokenPair.QuoteAssetDecimals));
        }
        else if (OptionsKind == OptionsKind.PUT)
        {
            return (tokenPair.BaseAssetAddress, Web3.Convert.ToWei(BaseAssetAmount, tokenPair.BaseAssetDecimals));
        }
        throw new Exception("invalid token");
    }
}

public class UserLotteryNFT
{
    public long TokenId { get; set; }

    public long ChainId { get; set; }

    public required string Contract { get; set; }

    public required string Status { get; set; }

    public required string ImageData { get; set; }

    public DateTimeOffset DrawTime { get; set; }

    public decimal BaseAssetAmount { get; set; }

    public bool Listable()
    {
        if (Status == "open")
        {
            return true;
        }
        return false;
    }

    public bool Drawable()
    {
        if (DateTime.Now > DrawTime)
        {
            return true;
        }
        return false;
    }
}

public class UserRedEnvelopeNFT
{
    public long TokenId { get; set; }

    public long ChainId { get; set; }

    public required string Contract { get; set; }

    public required string Status { get; set; }

    public required string ImageData { get; set; }

    public decimal BaseAssetAmount { get; set; }
}

public class UserRouletteNFT
{
    public long TokenId { get; set; }

    public long ChainId { get; set; }

    public required string Contract { get; set; }

    public required string ImageData { get; set; }

    public decimal BaseAssetAmount { get; set; }

    public DateTimeOffset OpenTime { get; set; }

    public required string Writer { get; set; }

    public bool Openable(string userAddress)
    {
        if (DateTime.Now > OpenTime)
        {
            return true;
        }
        else if (userAddress.Equals(Writer, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }
        return false;
    }
}
