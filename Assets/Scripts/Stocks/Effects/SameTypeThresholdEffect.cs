using UnityEngine;

public enum ThresholdStockMatchMode
{
    SameStockType,
    SameStock,
    SpecificStockType
}

[CreateAssetMenu(menuName = "Stocks/Effects/Same Type Threshold")]
public class SameTypeThresholdEffect : StockEffect
{
    public ThresholdStockMatchMode matchMode = ThresholdStockMatchMode.SameStockType;
    public StockType requiredStockType = StockType.Tech;

    [Min(1)]
    public int requiredMatchingStocks = 2;

    [Min(0)]
    public int bonusPerMatchingStock = 1;

    public override void Apply(StockEffectContext context)
    {
        int matchingStockCount = CountOtherStocksOfSameType(context);
        int threshold = Mathf.Max(1, requiredMatchingStocks);

        if (matchingStockCount < threshold)
            return;

        context.TotalValue += matchingStockCount * Mathf.Max(0, bonusPerMatchingStock);
    }

    private int CountOtherStocksOfSameType(StockEffectContext context)
    {
        int count = 0;

        for (int x = 0; x < context.Grid.columns; x++)
        {
            for (int y = 0; y < context.Grid.rows; y++)
            {
                if (x == context.X && y == context.Y)
                    continue;

                SlotCell cell = context.Grid.GetCell(x, y);
                if (cell == null || cell.IsEmpty || cell.StockDefinition == null)
                    continue;

                if (IsMatchingStock(context, cell.StockDefinition))
                    count++;
            }
        }

        return count;
    }

    private bool IsMatchingStock(
        StockEffectContext context,
        StockDefinition candidate
    )
    {
        switch (matchMode)
        {
            case ThresholdStockMatchMode.SameStockType:
                return candidate.stockType
                    == context.SourceCell.StockDefinition.stockType;

            case ThresholdStockMatchMode.SameStock:
                return candidate == context.SourceCell.StockDefinition;

            case ThresholdStockMatchMode.SpecificStockType:
                return candidate.stockType == requiredStockType;

            default:
                return false;
        }
    }
}
