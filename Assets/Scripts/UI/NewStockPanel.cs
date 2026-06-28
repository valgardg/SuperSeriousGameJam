using UnityEngine;

public class NewStockPanel : MonoBehaviour
{
    public Transform optionContainer;
    public GameObject stockOptionPrefab;

    private void Start()
    {
        gameObject.SetActive(false); // Hide the panel initially
        StockOptionUI.OnStockOptionSelected += HandleStockOptionSelected;
        GameController.DisplayStockPicker += DisplayStockPicker;
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

        gameObject.SetActive(true); 
    }

    private void HandleStockOptionSelected(StockDefinition selectedStock)
    {
        // Handle the selected stock option here
        Debug.Log($"Selected stock: {selectedStock.name}");

        // Hide the panel after selection
        gameObject.SetActive(false);
    }
}
