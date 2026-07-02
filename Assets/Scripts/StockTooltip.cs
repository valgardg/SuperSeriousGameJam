using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockTooltip : MonoBehaviour
{
    public Image stockIcon;
    public TMP_Text stockNameText;
    public TMP_Text stockStatsText;
    public TMP_Text stockSideEffectText;

    private void Awake()
    {
        PlayerPortfolioStock.OnHover += DisplayTooltip;
        PlayerPortfolioStock.OnHoverExit += HideTooltip;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        PlayerPortfolioStock.OnHover -= DisplayTooltip;
        PlayerPortfolioStock.OnHoverExit -= HideTooltip;
    }

    private void DisplayTooltip(StockDefinition stockDefinition)
    {
        if (stockDefinition == null)
        {
            return;
        }

        gameObject.SetActive(true);
        stockIcon.sprite = stockDefinition.icon;
        stockNameText.text = stockDefinition.stockName;
        stockStatsText.text = $"{stockDefinition.stockType} - ${stockDefinition.baseValue}";
        stockSideEffectText.text = stockDefinition.effects != null
            && stockDefinition.effects.Length > 0
            && stockDefinition.effects[0] != null
                ? $"{stockDefinition.effects[0].effectName}: {stockDefinition.effects[0].description}"
                : string.Empty;
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
