using System.Collections.Generic;

public class PlayerState
{
    public int money = 0;
    public List<StockDefinition> OwnedStocks = new();

    public void AddStock(StockDefinition stock)
    {
        OwnedStocks.Add(stock);
    }
}