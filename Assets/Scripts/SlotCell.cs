using UnityEngine;

public class SlotCell : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Sprite emptySprite;
    private SpriteRenderer spriteRenderer;

    public PortfolioStock PortfolioStock { get; private set; }
    public StockDefinition StockDefinition => PortfolioStock?.Definition;
    public int BaseValue => PortfolioStock?.CurrentBaseValue ?? 0;
    public bool IsEmpty { get; private set; }
    
    public void Init(PortfolioStock portfolioStock, bool isEmpty = false)
    {
        IsEmpty = isEmpty || portfolioStock == null;
        PortfolioStock = portfolioStock;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (IsEmpty)
        {
            spriteRenderer.sprite = emptySprite;
            return;
        }

        spriteRenderer.sprite = portfolioStock.Definition.icon;
    }
}
