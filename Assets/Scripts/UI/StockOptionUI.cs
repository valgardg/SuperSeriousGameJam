using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockOptionUI : MonoBehaviour
{
    public StockDefinition stockDefinition;

    // ui elements
    public Image stockIconImage;
    public TMP_Text stockName;
    public TMP_Text stockRarity;
    public TMP_Text stockStats;
    public TMP_Text stockSideEEffect;

    public static event System.Action<StockDefinition> OnStockOptionSelected;

    public void Init(StockDefinition stockDefinition)
    {
        this.stockDefinition = stockDefinition;

        // set the icon
        stockIconImage.sprite = stockDefinition.icon;

        // set the description
        stockName.text = stockDefinition.stockName;
        stockRarity.text = $"{stockDefinition.rarity}";
        stockStats.text = $"{stockDefinition.stockType} - Gives ${stockDefinition.baseValue}";
        stockSideEEffect.text = stockDefinition.effects != null && stockDefinition.effects.Length > 0
            ? $"{stockDefinition.effects[0].effectName}: {stockDefinition.effects[0].description}"
            : string.Empty;
    }

    private string GetStockOptionDescription(StockDefinition stockDefinition)
    {
        // Replace this with your actual logic to generate the description
        string description = $"Ticker: {stockDefinition.stockName}\n" +
                $"Base value: {stockDefinition.baseValue}\n" +
               $"Rarity: {stockDefinition.rarity}\n" + 
               $"Type: {stockDefinition.stockType}\n";

               if (stockDefinition.effects != null && stockDefinition.effects.Length > 0)
                   description += $"{stockDefinition.effects[0].effectName}: {stockDefinition.effects[0].description}\n";
        return description;
    }

    public void OnOptionSelected()
    {
        OnStockOptionSelected?.Invoke(stockDefinition);
    }
}
