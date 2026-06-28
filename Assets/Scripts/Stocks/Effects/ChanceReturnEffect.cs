using UnityEngine;

[CreateAssetMenu(menuName = "Stocks/Effects/Chance Return")]
public class ChanceReturnEffect : StockEffect
{
    [Range(0f, 1f)]
    public float successChance = 0.5f;

    [Min(0)]
    public int returnValue = 10;

    [Min(0)]
    public int failurePenalty = 0;

    public override void Apply(StockEffectContext context)
    {
        float clampedChance = Mathf.Clamp01(successChance);
        bool succeeded = clampedChance >= 1f
            || (clampedChance > 0f && Random.value < clampedChance);

        if (succeeded)
        {
            context.TotalValue += Mathf.Max(0, returnValue);
        }
        else
        {
            context.TotalValue -= Mathf.Max(0, failurePenalty);
        }
    }
}
