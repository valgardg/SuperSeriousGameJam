using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPortfolioStock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public StockDefinition stockDefinition;
    public static event System.Action<StockDefinition> OnHover;
    public static event System.Action OnHoverExit;

    private Image stockIconImage;

    private void Awake()
    {
        stockIconImage = GetComponent<Image>();
    }

    public void Init(StockDefinition stockDefinition)
    {
        this.stockDefinition = stockDefinition;

        if (stockIconImage != null && stockDefinition != null)
            stockIconImage.sprite = stockDefinition.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover");
        OnHover?.Invoke(stockDefinition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Leave");
        OnHoverExit?.Invoke();
    }
}
