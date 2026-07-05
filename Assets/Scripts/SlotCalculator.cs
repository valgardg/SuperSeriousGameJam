using System.Collections;
using UnityEngine;

public class SlotCalculator : MonoBehaviour
{
    public event System.Action<PortfolioStock, int> PortfolioPayoutCalculated;

    public SlotGrid slotGrid;
    public GameController gameController;
    public DollarSpawner dollarSpawner;

    [Header("Timing")]
    public float cellDelay = 0.3f;

    public void StartCalculation(System.Action<int> onComplete)
    {
        StartCoroutine(CalculateGridValueCoroutine(onComplete));
    }

    private IEnumerator CalculateGridValueCoroutine(System.Action<int> onComplete)
    {
        int currentRound = gameController.currentDay + 1;
        gameController.playerState.PreparePortfolioForRound(currentRound);

        int totalValue = 0;
        yield return StartCoroutine(CalculateBaseValueCoroutine(result => totalValue = result));

        for (int x = 0; x < slotGrid.columns; x++)
        {
            for (int y = 0; y < slotGrid.rows; y++)
            {
                SlotCell slot = slotGrid.GetCell(x, y);

                if (slot == null || slot.IsEmpty || slot.StockDefinition == null)
                    continue;

                if (slot.StockDefinition.effects == null || slot.StockDefinition.effects.Length == 0)
                    continue;

                yield return new WaitForSeconds(cellDelay);

                int valueBeforeEffects = totalValue;

                StockEffectContext context = new StockEffectContext(
                    gameController.playerState,
                    slotGrid,
                    slot,
                    x,
                    y,
                    currentRound,
                    totalValue
                );

                foreach (StockEffect effect in slot.StockDefinition.effects)
                {
                    if (effect == null
                        || effect is IStockBaseValueModifier
                        || effect is IPortfolioPayoutEffect)
                        continue;

                    effect.Apply(context);
                }

                totalValue = context.TotalValue;

                int effectDelta = totalValue - valueBeforeEffects;
                if (effectDelta != 0)
                {
                    dollarSpawner.SpawnDollars(effectDelta, slot.transform);
                }
            }
        }

        yield return StartCoroutine(CalculatePortfolioPayoutCoroutine(
            totalValue,
            result => totalValue = result
        ));

        Debug.Log($"Total value after base values, effects, and portfolio payouts: {totalValue}");
        yield return StartCoroutine(dollarSpawner.WaitUntilComplete());
        onComplete?.Invoke(totalValue);
    }

    private IEnumerator CalculatePortfolioPayoutCoroutine(
        int startingValue,
        System.Action<int> onComplete
    )
    {
        int totalValue = startingValue;

        foreach (PortfolioStock portfolioStock in gameController.playerState.OwnedStocks)
        {
            StockEffect[] effects = portfolioStock.Definition.effects;
            if (effects == null)
                continue;

            foreach (StockEffect effect in effects)
            {
                if (!(effect is IPortfolioPayoutEffect portfolioPayoutEffect))
                    continue;

                int payout = portfolioPayoutEffect.GetPortfolioPayout(portfolioStock);
                if (payout == 0)
                    continue;

                yield return new WaitForSeconds(cellDelay);
                totalValue += payout;
                PortfolioPayoutCalculated?.Invoke(portfolioStock, payout);
            }
        }

        onComplete?.Invoke(totalValue);
    }

    private IEnumerator CalculateBaseValueCoroutine(System.Action<int> onComplete)
    {
        int totalValue = 0;

        for (int x = 0; x < slotGrid.columns; x++)
        {
            for (int y = 0; y < slotGrid.rows; y++)
            {
                SlotCell slot = slotGrid.GetCell(x, y);

                if (slot == null || slot.IsEmpty)
                    continue;

                yield return new WaitForSeconds(cellDelay);

                totalValue += slot.BaseValue;

                if (slot.BaseValue != 0)
                {
                    dollarSpawner.SpawnDollars(slot.BaseValue, slot.transform);
                }
            }
        }

        onComplete?.Invoke(totalValue);
    }
}
