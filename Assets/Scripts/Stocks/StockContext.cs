public class StockEffectContext
{
    public PlayerState Player;
    public SlotGrid Grid;
    public SlotCell SourceCell;

    public int X;
    public int Y;
    public int RoundNumber;

    public int TotalValue;

    public StockEffectContext(
        PlayerState player,
        SlotGrid grid,
        SlotCell sourceCell,
        int x,
        int y,
        int roundNumber,
        int totalValue
    )
    {
        Player = player;
        Grid = grid;
        SourceCell = sourceCell;
        X = x;
        Y = y;
        RoundNumber = roundNumber;
        TotalValue = totalValue;
    }
}
