using System;

public sealed class PortfolioStock
{
    public StockDefinition Definition { get; }
    public int AcquiredRound { get; }
    public int CurrentBaseValue { get; private set; }
    public int RoundsOwned { get; private set; }

    public PortfolioStock(StockDefinition definition, int acquiredRound)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        AcquiredRound = Math.Max(0, acquiredRound);
        CurrentBaseValue = definition.baseValue;
    }

    public void PrepareForRound(int currentRound)
    {
        RoundsOwned = Math.Max(0, currentRound - AcquiredRound);
        int growthBonus = 0;

        if (Definition.effects != null)
        {
            foreach (StockEffect effect in Definition.effects)
            {
                if (effect is IStockBaseValueModifier baseValueModifier)
                    growthBonus += baseValueModifier.GetBaseValueBonus(RoundsOwned);
            }
        }

        CurrentBaseValue = Definition.baseValue + growthBonus;
    }
}
