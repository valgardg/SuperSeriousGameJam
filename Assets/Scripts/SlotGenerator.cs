using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotGenerator : MonoBehaviour
{
    public SlotCell cellPrefab;
    public GameController gameController;
    [SerializeField]
    private StockDefinition[] stockPool;
    [SerializeField]
    private StockDefinition emptyStockDefinition;
    [SerializeField]
    private StockDefinition initialPlayerStock;

    private void Awake()
    {
        stockPool = Resources.LoadAll<StockDefinition>("Stocks");

        emptyStockDefinition = Resources.LoadAll<StockDefinition>("EmptyStock").FirstOrDefault();
        initialPlayerStock = Resources.LoadAll<StockDefinition>("InitialStock").FirstOrDefault();

        if (emptyStockDefinition == null)
            Debug.LogError("No StockDefinition was found in Resources/EmptyStock.", this);

        if (initialPlayerStock == null)
            Debug.LogError("No StockDefinition was found in Resources/InitialStock.", this);

        Debug.Log($"Loaded {stockPool.Length} stocks.");
    }

    private void Start()
    {
        if (gameController == null || gameController.playerState == null)
        {
            Debug.LogError("SlotGenerator requires an initialized GameController.", this);
            return;
        }

        if (initialPlayerStock != null)
            gameController.playerState.AddStock(initialPlayerStock);
    }

    public SlotCell GenerateSlotCell(bool isEmpty = false)
    {
        SlotCell cell = Instantiate(cellPrefab, transform);
        StockDefinition stockDefinition;

        List<StockDefinition> ownedStocks = gameController.playerState.OwnedStocks;

        float fillChance = Mathf.Lerp(0.10f, 1f, Mathf.Sqrt(ownedStocks.Count / 15f));
        bool generateFilled = !isEmpty
            && ownedStocks.Count > 0
            && Random.value < fillChance;

        if (generateFilled)
        {
            stockDefinition = ownedStocks[Random.Range(0, ownedStocks.Count)];
        }
        else
        {
            stockDefinition = emptyStockDefinition;
        }

        cell.Init(isEmpty: !generateFilled, stockDefinition: stockDefinition);
        return cell;
    }

    public StockDefinition[] GetUnknownStocks()
    {
        var unknownStocks = stockPool
            .Where(stock => !gameController.playerState.OwnedStocks.Contains(stock))
            .OrderBy(_ => Random.value)
            .Take(3)
            .ToList();

        if (unknownStocks.Count < 3)
        {
            var knownStocks = gameController.playerState.OwnedStocks
                .Except(unknownStocks)
                .OrderBy(_ => Random.value);

            unknownStocks.AddRange(knownStocks.Take(3 - unknownStocks.Count));
        }

        return unknownStocks.ToArray();
    }

    public SlotCell GenerateEmptySlotCell()
    {
        SlotCell cell = Instantiate(cellPrefab, transform);
        cell.Init(isEmpty: true, stockDefinition: emptyStockDefinition);
        return cell;
    }
}
