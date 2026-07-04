using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaritySpawnRule
{
    public Rarity rarity;

    [Min(1)]
    public int unlockDay = 1;

    [Min(0f)]
    public float weight = 1f;

    public RaritySpawnRule()
    {
    }

    public RaritySpawnRule(Rarity rarity, int unlockDay, float weight)
    {
        this.rarity = rarity;
        this.unlockDay = unlockDay;
        this.weight = weight;
    }
}

[CreateAssetMenu(menuName = "Stocks/Rarity Spawn Configuration")]
public class RaritySpawnConfiguration : ScriptableObject
{
    [SerializeField]
    private RaritySpawnRule[] rules = CreateDefaultRules();

    public bool TryGetRule(Rarity rarity, out RaritySpawnRule matchingRule)
    {
        if (rules != null)
        {
            foreach (RaritySpawnRule rule in rules)
            {
                if (rule != null && rule.rarity == rarity)
                {
                    matchingRule = rule;
                    return true;
                }
            }
        }

        matchingRule = null;
        return false;
    }

    [ContextMenu("Reset to Default Rules")]
    private void ResetToDefaultRules()
    {
        rules = CreateDefaultRules();
    }

    private void OnValidate()
    {
        if (rules == null || rules.Length == 0)
        {
            rules = CreateDefaultRules();
            return;
        }

        HashSet<Rarity> configuredRarities = new();

        foreach (RaritySpawnRule rule in rules)
        {
            if (rule == null)
                continue;

            rule.unlockDay = Mathf.Max(1, rule.unlockDay);
            rule.weight = Mathf.Max(0f, rule.weight);

            if (!configuredRarities.Add(rule.rarity))
            {
                Debug.LogWarning(
                    $"Duplicate rarity spawn rule for {rule.rarity}; the first rule will be used.",
                    this
                );
            }
        }

        foreach (Rarity rarity in System.Enum.GetValues(typeof(Rarity)))
        {
            if (!configuredRarities.Contains(rarity))
            {
                Debug.LogWarning(
                    $"No rarity spawn rule is configured for {rarity}.",
                    this
                );
            }
        }
    }

    private static RaritySpawnRule[] CreateDefaultRules()
    {
        return new[]
        {
            new RaritySpawnRule(Rarity.Common, 1, 60f),
            new RaritySpawnRule(Rarity.Uncommon, 3, 30f),
            new RaritySpawnRule(Rarity.Rare, 7, 9f),
            new RaritySpawnRule(Rarity.Legendary, 12, 1f)
        };
    }
}
