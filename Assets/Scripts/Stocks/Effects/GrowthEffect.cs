using UnityEngine;

[CreateAssetMenu(menuName = "Stocks/Effects/Growth")]
public class GrowthEffect : StockEffect, IStockBaseValueModifier
{
    [Min(1)]
    public int roundsPerGrowth = 3;

    [Min(0)]
    public int growthAmount = 1;

    public override void Apply(StockEffectContext context)
    {
        // Growth is applied to the portfolio entry before base-value scoring.
    }

    public int GetBaseValueBonus(int roundsOwned)
    {
        int interval = Mathf.Max(1, roundsPerGrowth);
        int completedGrowthPeriods = Mathf.Max(0, roundsOwned) / interval;

        return completedGrowthPeriods * Mathf.Max(0, growthAmount);
    }
}
