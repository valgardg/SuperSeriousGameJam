using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameController gameController;
    public PlayerStatsUI playerStatsUI;

    [Header("Player Portfolio")]
    [SerializeField] private Transform playerPortfolioContainer;
    [SerializeField] private PlayerPortfolioStock playerPortfolioStockPrefab;

    private readonly Dictionary<PortfolioStock, PlayerPortfolioStock> portfolioViews = new();
    private PlayerState subscribedPlayerState;
    private SlotCalculator subscribedSlotCalculator;
    private DollarSpawner dollarSpawner;

    private void Awake()
    {
        if (playerPortfolioContainer == null)
            Debug.LogError("UIManager requires a Player Portfolio Container.", this);

        if (playerPortfolioStockPrefab == null)
            Debug.LogError("UIManager requires a Player Portfolio Stock Prefab.", this);
    }

    private void Start()
    {
        playerStatsUI.Init(gameController);

        subscribedPlayerState = gameController.playerState;
        subscribedSlotCalculator = gameController.slotCalculator;
        dollarSpawner = subscribedSlotCalculator.dollarSpawner;

        subscribedPlayerState.StockAdded += AddStockToPortfolio;
        subscribedSlotCalculator.PortfolioPayoutCalculated += HandlePortfolioPayout;

        foreach (PortfolioStock stock in subscribedPlayerState.OwnedStocks)
            AddStockToPortfolio(stock);
    }

    private void OnDestroy()
    {
        if (subscribedPlayerState != null)
            subscribedPlayerState.StockAdded -= AddStockToPortfolio;

        if (subscribedSlotCalculator != null)
            subscribedSlotCalculator.PortfolioPayoutCalculated -= HandlePortfolioPayout;
    }

    private void AddStockToPortfolio(PortfolioStock stockEntry)
    {
        if (stockEntry == null
            || playerPortfolioContainer == null
            || playerPortfolioStockPrefab == null
            || portfolioViews.ContainsKey(stockEntry))
            return;

        PlayerPortfolioStock portfolioStockView = Instantiate(
            playerPortfolioStockPrefab,
            playerPortfolioContainer
        );

        portfolioStockView.Init(stockEntry);
        portfolioViews.Add(stockEntry, portfolioStockView);
    }

    private void HandlePortfolioPayout(PortfolioStock stock, int payout)
    {
        if (stock == null || payout == 0 || dollarSpawner == null)
            return;

        if (!portfolioViews.TryGetValue(stock, out PlayerPortfolioStock stockView))
        {
            AddStockToPortfolio(stock);
            portfolioViews.TryGetValue(stock, out stockView);
        }

        if (stockView == null)
            return;

        dollarSpawner.SpawnDollars(
            payout,
            ConvertPortfolioPositionToWorld(stockView.transform.position)
        );
    }

    private Vector3 ConvertPortfolioPositionToWorld(Vector3 portfolioPosition)
    {
        Canvas canvas = playerPortfolioContainer.GetComponentInParent<Canvas>();
        Camera canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            canvasCamera,
            portfolioPosition
        );

        Camera worldCamera = Camera.main;
        if (worldCamera == null)
            return portfolioPosition;

        float payoutPlaneZ = dollarSpawner.dollarTarget != null
            ? dollarSpawner.dollarTarget.position.z
            : 0f;

        Plane payoutPlane = new Plane(
            Vector3.forward,
            new Vector3(0f, 0f, payoutPlaneZ)
        );
        Ray ray = worldCamera.ScreenPointToRay(screenPosition);

        return payoutPlane.Raycast(ray, out float distance)
            ? ray.GetPoint(distance)
            : portfolioPosition;
    }
}
