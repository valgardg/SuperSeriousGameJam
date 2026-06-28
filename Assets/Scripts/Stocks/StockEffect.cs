using UnityEngine;

public abstract class StockEffect : ScriptableObject
{
    public string effectName;
    [TextArea]
    public string description;

    public abstract void Apply(StockEffectContext context);
}