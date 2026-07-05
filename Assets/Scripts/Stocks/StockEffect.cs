using UnityEngine;

public interface IStockBaseValueModifier
{
    int GetBaseValueBonus(int roundsOwned);
}

public interface IPortfolioPayoutEffect
{
    int GetPortfolioPayout(PortfolioStock stock);
}

public abstract class StockEffect : ScriptableObject
{
    public string effectName;
    [TextArea]
    public string description;

    public abstract void Apply(StockEffectContext context);
}
