using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stocks/Effects/Unique Stock Type Bonus")]
public class UniqueStockTypeBonusEffect : StockEffect
{
    [Min(0)]
    public int bonusPerUniqueStockType = 1;

    public override void Apply(StockEffectContext context)
    {
        HashSet<StockType> uniqueStockTypes = new HashSet<StockType>();

        for (int x = 0; x < context.Grid.columns; x++)
        {
            for (int y = 0; y < context.Grid.rows; y++)
            {
                SlotCell cell = context.Grid.GetCell(x, y);
                if (cell == null || cell.IsEmpty || cell.StockDefinition == null)
                    continue;

                uniqueStockTypes.Add(cell.StockDefinition.stockType);
            }
        }

        context.TotalValue += uniqueStockTypes.Count
            * Mathf.Max(0, bonusPerUniqueStockType);
    }
}
