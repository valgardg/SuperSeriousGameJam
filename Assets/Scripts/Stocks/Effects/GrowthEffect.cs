using UnityEngine;

[CreateAssetMenu(menuName = "Stocks/Effects/Growth")]
public class GrowthEffect : StockEffect
{
    [Min(1)]
    public int roundsPerGrowth = 3;

    [Min(0)]
    public int growthAmount = 1;

    public override void Apply(StockEffectContext context)
    {
        int interval = Mathf.Max(1, roundsPerGrowth);
        int completedGrowthPeriods = Mathf.Max(0, context.RoundNumber) / interval;

        context.TotalValue += completedGrowthPeriods * Mathf.Max(0, growthAmount);
    }
}
