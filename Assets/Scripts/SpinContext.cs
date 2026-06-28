public class SpinContext
{
    public int RemainingFilled;
    public int RemainingTotal;

    public SpinContext(int filledCount, int totalCells)
    {
        RemainingFilled = filledCount;
        RemainingTotal = totalCells;
    }
}