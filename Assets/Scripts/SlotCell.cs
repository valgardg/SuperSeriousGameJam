using UnityEngine;

public class SlotCell : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Sprite emptySprite;
    private SpriteRenderer spriteRenderer;
    public int baseValue;

    public StockDefinition StockDefinition { get; private set; }
    public int BaseValue { get; private set; }
    public bool IsEmpty { get; private set; }
    
    public void Init(StockDefinition stockDefinition, bool isEmpty = false)
    {
        IsEmpty = isEmpty;
        StockDefinition = stockDefinition;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (IsEmpty || stockDefinition == null)
        {
            BaseValue = 0;
            spriteRenderer.sprite = emptySprite;
            return;
        }

        BaseValue = stockDefinition.baseValue;
        spriteRenderer.sprite = stockDefinition.icon;
    }
}
