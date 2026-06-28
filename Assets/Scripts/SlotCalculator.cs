using System.Collections;
using UnityEngine;

public class SlotCalculator : MonoBehaviour
{
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
                    gameController.currentDay + 1,
                    totalValue
                );

                foreach (StockEffect effect in slot.StockDefinition.effects)
                {
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

        Debug.Log($"Total grid value after effects: {totalValue}");
        yield return StartCoroutine(dollarSpawner.WaitUntilComplete());
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
