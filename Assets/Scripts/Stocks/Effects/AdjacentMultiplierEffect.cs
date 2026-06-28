using UnityEngine;

public enum AdjacentMultiplierMatchMode
{
    AllStocks,
    SameStock,
    SameStockType,
    SpecificStockType
}

[CreateAssetMenu(menuName = "Stocks/Effects/Adjacent Multiplier")]
public class AdjacentMultiplierEffect : StockEffect
{
    public AdjacentMultiplierMatchMode matchMode = AdjacentMultiplierMatchMode.AllStocks;
    public StockType requiredStockType = StockType.Tech;

    [Min(1)]
    public int multiplier = 2;

    public override void Apply(StockEffectContext context)
    {
        int extraMultiplier = Mathf.Max(1, multiplier) - 1;

        AddAdjacentValue(context, context.X + 1, context.Y, extraMultiplier);
        AddAdjacentValue(context, context.X - 1, context.Y, extraMultiplier);
        AddAdjacentValue(context, context.X, context.Y + 1, extraMultiplier);
        AddAdjacentValue(context, context.X, context.Y - 1, extraMultiplier);
    }

    private void AddAdjacentValue(
        StockEffectContext context,
        int x,
        int y,
        int extraMultiplier
    )
    {
        if (x < 0 || x >= context.Grid.columns) return;
        if (y < 0 || y >= context.Grid.rows) return;

        SlotCell cell = context.Grid.GetCell(x, y);
        if (cell == null || cell.IsEmpty || cell.StockDefinition == null)
            return;

        if (!IsMatchingStock(context, cell.StockDefinition))
            return;

        context.TotalValue += cell.BaseValue * extraMultiplier;
    }

    private bool IsMatchingStock(
        StockEffectContext context,
        StockDefinition candidate
    )
    {
        switch (matchMode)
        {
            case AdjacentMultiplierMatchMode.AllStocks:
                return true;

            case AdjacentMultiplierMatchMode.SameStock:
                return candidate == context.SourceCell.StockDefinition;

            case AdjacentMultiplierMatchMode.SameStockType:
                return candidate.stockType
                    == context.SourceCell.StockDefinition.stockType;

            case AdjacentMultiplierMatchMode.SpecificStockType:
                return candidate.stockType == requiredStockType;

            default:
                return false;
        }
    }
}
