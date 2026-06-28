using UnityEngine;

public enum AdjacentStockMatchMode
{
    SameStock,
    SameStockType,
    SpecificStockType
}

[CreateAssetMenu(menuName = "Stocks/Effects/Adjacent Bonus")]
public class AdjacentBonusEffect : StockEffect
{
    public AdjacentStockMatchMode matchMode = AdjacentStockMatchMode.SameStock;
    public StockType requiredStockType = StockType.Tech;

    public int bonusPerAdjacentStock = 5;

    public override void Apply(StockEffectContext context)
    {
        int adjacentCount = 0;

        adjacentCount += HasMatchingStock(context, context.X + 1, context.Y) ? 1 : 0;
        adjacentCount += HasMatchingStock(context, context.X - 1, context.Y) ? 1 : 0;
        adjacentCount += HasMatchingStock(context, context.X, context.Y + 1) ? 1 : 0;
        adjacentCount += HasMatchingStock(context, context.X, context.Y - 1) ? 1 : 0;

        context.TotalValue += adjacentCount * bonusPerAdjacentStock;
    }

    private bool HasMatchingStock(StockEffectContext context, int x, int y)
    {
        if (x < 0 || x >= context.Grid.columns) return false;
        if (y < 0 || y >= context.Grid.rows) return false;

        SlotCell cell = context.Grid.GetCell(x, y);
        if (cell == null || cell.IsEmpty || cell.StockDefinition == null)
            return false;

        switch (matchMode)
        {
            case AdjacentStockMatchMode.SameStock:
                return cell.StockDefinition == context.SourceCell.StockDefinition;

            case AdjacentStockMatchMode.SameStockType:
                return cell.StockDefinition.stockType
                    == context.SourceCell.StockDefinition.stockType;

            case AdjacentStockMatchMode.SpecificStockType:
                return cell.StockDefinition.stockType == requiredStockType;

            default:
                return false;
        }
    }
}
