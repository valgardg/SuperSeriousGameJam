using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameController gameController;
    public PlayerStatsUI playerStatsUI;

    [Header("Player Portfolio")]
    [SerializeField] private Transform playerPortfolioContainer;
    [SerializeField] private PlayerPortfolioStock playerPortfolioStockPrefab;

    private void Awake()
    {
        if (playerPortfolioContainer == null)
            Debug.LogError("UIManager requires a Player Portfolio Container.", this);

        if (playerPortfolioStockPrefab == null)
            Debug.LogError("UIManager requires a Player Portfolio Stock Prefab.", this);
    }

    private void OnEnable()
    {
        StockOptionUI.OnStockOptionSelected += AddStockToPortfolio;
    }

    private void OnDisable()
    {
        StockOptionUI.OnStockOptionSelected -= AddStockToPortfolio;
    }

    private void Start()
    {
        playerStatsUI.Init(gameController);
    }

    private void AddStockToPortfolio(StockDefinition stockDefinition)
    {
        if (stockDefinition == null
            || playerPortfolioContainer == null
            || playerPortfolioStockPrefab == null)
            return;

        PlayerPortfolioStock portfolioStock = Instantiate(
            playerPortfolioStockPrefab,
            playerPortfolioContainer
        );

        portfolioStock.Init(stockDefinition);
    }
}
