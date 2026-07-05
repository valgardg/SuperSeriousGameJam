using System.Collections.Generic;

public class PlayerState
{
    public event System.Action<PortfolioStock> StockAdded;

    public int money = 0;
    public List<PortfolioStock> OwnedStocks = new();

    public PortfolioStock AddStock(StockDefinition stock, int acquiredRound)
    {
        if (stock == null || GetStock(stock) != null)
            return null;

        PortfolioStock portfolioStock = new PortfolioStock(stock, acquiredRound);
        OwnedStocks.Add(portfolioStock);
        StockAdded?.Invoke(portfolioStock);
        return portfolioStock;
    }

    public PortfolioStock GetStock(StockDefinition definition)
    {
        return OwnedStocks.Find(stock => stock.Definition == definition);
    }

    public void PreparePortfolioForRound(int currentRound)
    {
        foreach (PortfolioStock stock in OwnedStocks)
            stock.PrepareForRound(currentRound);
    }
}
