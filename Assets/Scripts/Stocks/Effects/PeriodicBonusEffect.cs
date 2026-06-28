using UnityEngine;

[CreateAssetMenu(menuName = "Stocks/Effects/Periodic Bonus")]
public class PeriodicBonusEffect : StockEffect
{
    [Min(1)]
    public int roundInterval = 3;

    [Min(0)]
    public int bonusValue = 10;

    public override void Apply(StockEffectContext context)
    {
        int interval = Mathf.Max(1, roundInterval);

        if (context.RoundNumber <= 0 || context.RoundNumber % interval != 0)
            return;

        context.TotalValue += Mathf.Max(0, bonusValue);
    }
}
