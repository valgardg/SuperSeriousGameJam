using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPortfolioStock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PortfolioStock PortfolioStock { get; private set; }
    public static event System.Action<PortfolioStock> OnHover;
    public static event System.Action OnHoverExit;

    private Image stockIconImage;

    private void Awake()
    {
        stockIconImage = GetComponent<Image>();
    }

    public void Init(PortfolioStock portfolioStock)
    {
        PortfolioStock = portfolioStock;

        if (stockIconImage != null && portfolioStock != null)
            stockIconImage.sprite = portfolioStock.Definition.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover");
        OnHover?.Invoke(PortfolioStock);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Leave");
        OnHoverExit?.Invoke();
    }
}
