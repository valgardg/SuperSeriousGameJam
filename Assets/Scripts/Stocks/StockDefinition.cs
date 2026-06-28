using UnityEngine;

public enum StockType 
{
    Tech,
    Consumer,
    Crypto,
    Meme,
    ETF,
};

[System.Serializable]
[CreateAssetMenu(menuName = "Stocks/Stock Definition")]
public class StockDefinition : ScriptableObject
{
    public string stockName;
    public Sprite icon;
    public StockType stockType;
    public int baseValue;
    public Rarity rarity;

    public StockEffect[] effects;
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}