using UnityEngine;
using UnityEngine.UI;

public class NewStockPanel : MonoBehaviour
{
    public Transform optionContainer;
    public GameObject stockOptionPrefab;

    [Header("Panel Controls")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button chooseButton;
    [SerializeField] private Button spinButton;

    private bool isAwaitingSelection;

    private void Awake()
    {
        StockOptionUI.OnStockOptionSelected += HandleStockOptionSelected;
        GameController.DisplayStockPicker += DisplayStockPicker;

        if (closeButton != null)
            closeButton.onClick.AddListener(TogglePanel);

        if (chooseButton != null)
            chooseButton.onClick.AddListener(TogglePanel);

        SetChooseButtonVisible(false);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        StockOptionUI.OnStockOptionSelected -= HandleStockOptionSelected;
        GameController.DisplayStockPicker -= DisplayStockPicker;

        if (closeButton != null)
            closeButton.onClick.RemoveListener(TogglePanel);

        if (chooseButton != null)
            chooseButton.onClick.RemoveListener(TogglePanel);
    }

    public void DisplayStockPicker(StockDefinition[] stockDefinitions)
    {
        // Clear existing options
        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new options
        foreach (var stockDefinition in stockDefinitions)
        {
            GameObject optionGO = Instantiate(stockOptionPrefab, optionContainer);
            StockOptionUI optionUI = optionGO.GetComponent<StockOptionUI>();
            optionUI.Init(stockDefinition);
        }  

        isAwaitingSelection = true;

        if (spinButton != null)
            spinButton.interactable = false;

        SetPanelVisible(true);
    }

    public void TogglePanel()
    {
        if (!isAwaitingSelection)
            return;

        SetPanelVisible(!gameObject.activeSelf);
    }

    private void HandleStockOptionSelected(StockDefinition selectedStock)
    {
        // Handle the selected stock option here
        Debug.Log($"Selected stock: {selectedStock.name}");

        isAwaitingSelection = false;
        SetPanelVisible(false);

        if (spinButton != null)
            spinButton.interactable = true;
    }

    private void SetPanelVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
        SetChooseButtonVisible(isAwaitingSelection && !isVisible);
    }

    private void SetChooseButtonVisible(bool isVisible)
    {
        if (chooseButton != null)
            chooseButton.gameObject.SetActive(isVisible);
    }
}
