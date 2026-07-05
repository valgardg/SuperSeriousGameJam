using UnityEngine;

[CreateAssetMenu(menuName = "Stocks/Effects/Periodic Bonus")]
public class PeriodicBonusEffect : StockEffect, IPortfolioPayoutEffect
{
    [Min(1)]
    public int roundInterval = 3;

    [Min(0)]
    public int bonusValue = 10;

    public override void Apply(StockEffectContext context)
    {
        // Periodic payouts are processed from the portfolio after grid effects.
    }

    public int GetPortfolioPayout(PortfolioStock stock)
    {
        int interval = Mathf.Max(1, roundInterval);
        if (stock == null
            || stock.RoundsOwned <= 0
            || stock.RoundsOwned % interval != 0)
            return 0;

        return Mathf.Max(0, bonusValue);
    }
}
