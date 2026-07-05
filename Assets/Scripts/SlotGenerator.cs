using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotGenerator : MonoBehaviour
{
    private const int StockOptionCount = 3;

    public SlotCell cellPrefab;
    public GameController gameController;
    [SerializeField]
    private StockDefinition[] stockPool;
    [SerializeField]
    private StockDefinition emptyStockDefinition;
    [SerializeField]
    private StockDefinition initialPlayerStock;

    [Header("Stock Picker Configuration")]
    [SerializeField] private RaritySpawnConfiguration raritySpawnConfiguration;

    private void Awake()
    {
        stockPool = Resources.LoadAll<StockDefinition>("Stocks");

        emptyStockDefinition = Resources.LoadAll<StockDefinition>("EmptyStock").FirstOrDefault();
        initialPlayerStock = Resources.LoadAll<StockDefinition>("InitialStock").FirstOrDefault();

        if (emptyStockDefinition == null)
            Debug.LogError("No StockDefinition was found in Resources/EmptyStock.", this);

        if (initialPlayerStock == null)
            Debug.LogError("No StockDefinition was found in Resources/InitialStock.", this);

        if (raritySpawnConfiguration == null)
        {
            raritySpawnConfiguration = ScriptableObject.CreateInstance<RaritySpawnConfiguration>();
            raritySpawnConfiguration.name = "Runtime Default Rarity Spawn Configuration";
            Debug.LogWarning(
                "No Rarity Spawn Configuration was assigned; using the built-in defaults.",
                this
            );
        }

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
            gameController.playerState.AddStock(initialPlayerStock, 0);
    }

    public SlotCell GenerateSlotCell(bool isEmpty = false)
    {
        SlotCell cell = Instantiate(cellPrefab, transform);
        PortfolioStock portfolioStock = null;

        List<PortfolioStock> ownedStocks = gameController.playerState.OwnedStocks;

        float fillChance = Mathf.Lerp(0.10f, 1f, Mathf.Sqrt(ownedStocks.Count / 15f));
        bool generateFilled = !isEmpty
            && ownedStocks.Count > 0
            && Random.value < fillChance;

        if (generateFilled)
            portfolioStock = ownedStocks[Random.Range(0, ownedStocks.Count)];

        cell.Init(portfolioStock, isEmpty: !generateFilled);
        return cell;
    }

    public StockDefinition[] GetUnknownStocks()
    {
        return GetUnknownStocks(raritySpawnConfiguration);
    }

    public StockDefinition[] GetUnknownStocks(
        RaritySpawnConfiguration configuration
    )
    {
        if (configuration == null)
        {
            Debug.LogError("Cannot generate stock options without a rarity spawn configuration.", this);
            return System.Array.Empty<StockDefinition>();
        }

        int pickerDay = gameController.currentDay + 1;
        HashSet<StockDefinition> ownedStocks = new(
            gameController.playerState.OwnedStocks.Select(stock => stock.Definition)
        );

        List<StockDefinition> unownedCandidates = stockPool
            .Where(stock => !ownedStocks.Contains(stock))
            .Where(stock => IsEligibleForPicker(stock, pickerDay, configuration))
            .Distinct()
            .ToList();

        List<StockDefinition> options = DrawWeightedStocks(
            unownedCandidates,
            StockOptionCount,
            configuration
        );

        if (options.Count < StockOptionCount)
        {
            Debug.LogWarning(
                $"Only {options.Count} unique stocks are eligible for the picker on day {pickerDay}.",
                this
            );
        }

        return options.ToArray();
    }

    private List<StockDefinition> DrawWeightedStocks(
        List<StockDefinition> candidates,
        int count,
        RaritySpawnConfiguration configuration
    )
    {
        List<StockDefinition> selections = new();

        while (selections.Count < count && candidates.Count > 0)
        {
            List<Rarity> availableRarities = candidates
                .Select(stock => stock.rarity)
                .Distinct()
                .ToList();

            float totalWeight = availableRarities.Sum(
                rarity => GetRarityWeight(configuration, rarity)
            );

            if (totalWeight <= 0f)
                break;

            float roll = Random.value * totalWeight;
            Rarity selectedRarity = availableRarities[^1];

            foreach (Rarity rarity in availableRarities)
            {
                roll -= GetRarityWeight(configuration, rarity);
                if (roll <= 0f)
                {
                    selectedRarity = rarity;
                    break;
                }
            }

            List<StockDefinition> stocksOfSelectedRarity = candidates
                .Where(stock => stock.rarity == selectedRarity)
                .ToList();

            StockDefinition selectedStock = stocksOfSelectedRarity[
                Random.Range(0, stocksOfSelectedRarity.Count)
            ];

            selections.Add(selectedStock);
            candidates.Remove(selectedStock);
        }

        return selections;
    }

    private static bool IsEligibleForPicker(
        StockDefinition stock,
        int pickerDay,
        RaritySpawnConfiguration configuration
    )
    {
        if (stock == null
            || !configuration.TryGetRule(stock.rarity, out RaritySpawnRule rule))
            return false;

        return pickerDay >= Mathf.Max(1, rule.unlockDay) && rule.weight > 0f;
    }

    private static float GetRarityWeight(
        RaritySpawnConfiguration configuration,
        Rarity rarity
    )
    {
        return configuration.TryGetRule(rarity, out RaritySpawnRule rule)
            ? Mathf.Max(0f, rule.weight)
            : 0f;
    }

    public SlotCell GenerateEmptySlotCell()
    {
        SlotCell cell = Instantiate(cellPrefab, transform);
        cell.Init(null, isEmpty: true);
        return cell;
    }
}
