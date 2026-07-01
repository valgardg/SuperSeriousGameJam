using UnityEngine;

public class GameController : MonoBehaviour
{
    [System.NonSerialized]
    public PlayerState playerState;
    public SlotCalculator slotCalculator;
    public SlotGenerator slotGenerator;

    public static event System.Action<StockDefinition[]> DisplayStockPicker;
    public static event System.Action RentPaid;

    [SerializeField] private GameObject gameOverPanel;

    // game days information
    public int currentDay = 0;
    public LandlordController landlordController;
    private int latestRentPrice = 0;
    private SlotController activeSlotController;
    private DollarSpawner dollarSpawner;

    private void Awake()
    {
        playerState = new PlayerState();
        dollarSpawner = slotCalculator != null ? slotCalculator.dollarSpawner : null;

        if (gameOverPanel == null)
            Debug.LogError("GameController requires a Game Over Panel reference.", this);

        if (dollarSpawner == null)
            Debug.LogError("GameController requires a SlotCalculator with a DollarSpawner.", this);
    }

    private void OnEnable()
    {
        SlotController.OnSpinEnded += HandleSpinEnded;
        StockOptionUI.OnStockOptionSelected += HandleStockOptionSelected;
        LandlordController.OnLandlordEventTriggered += HandleLandlordEventTriggered;

        if (dollarSpawner != null)
            dollarSpawner.DollarReachedTarget += HandleDollarReachedTarget;
    }

    private void OnDisable()
    {
        SlotController.OnSpinEnded -= HandleSpinEnded;
        StockOptionUI.OnStockOptionSelected -= HandleStockOptionSelected;
        LandlordController.OnLandlordEventTriggered -= HandleLandlordEventTriggered;

        if (dollarSpawner != null)
            dollarSpawner.DollarReachedTarget -= HandleDollarReachedTarget;
    }

    private void HandleSpinEnded(SlotController slotController)
    {
        activeSlotController = slotController;
        int moneyBeforeSpin = playerState.money;

        slotCalculator.StartCalculation(totalValue =>
        {
            Debug.Log($"Total value calculated: {totalValue}");
            playerState.money = moneyBeforeSpin + totalValue;
            Debug.Log($"Player's new money: {playerState.money}");

            StockDefinition[] unknownStocks = slotGenerator.GetUnknownStocks();
            DampenSoundtrackVolume();
            DisplayStockPicker?.Invoke(unknownStocks);
        });
    }

    private void HandleDollarReachedTarget(int value)
    {
        playerState.money += value;
    }

    private void HandleStockOptionSelected(StockDefinition selectedStock)
    {
        playerState.AddStock(selectedStock);
        Debug.Log($"Added stock: {selectedStock.name} to player's inventory.");
        RestoreSoundtrackVolume();
        currentDay++;
        latestRentPrice = landlordController.CheckTriggerDayInfo(currentDay);

        if (latestRentPrice <= 0)
            CompleteCurrentSpinCycle();
    }

    public void AttemptToPayRent()
    {
        if (latestRentPrice <= 0)
        {
            Debug.LogWarning("Attempted to pay rent when no rent is currently due.", this);
            return;
        }

        if (playerState.money >= latestRentPrice)
        {
            Debug.Log("Player paid off rent!");
            playerState.money -= latestRentPrice;
            latestRentPrice = 0;
            RestoreSoundtrackVolume();
            RentPaid?.Invoke();
            CompleteCurrentSpinCycle();
        }
        else
        {
            Debug.Log("Player has lost!");
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (soundtrackAudioSource != null)
                soundtrackAudioSource.Stop();
        }
    }

    public AudioSource soundtrackAudioSource;

    private void HandleLandlordEventTriggered(DayInfo dayInfo)
    {
        DampenSoundtrackVolume();
    }

    private void CompleteCurrentSpinCycle()
    {
        if (activeSlotController == null)
            return;

        activeSlotController.CompleteSpinCycle();
        activeSlotController = null;
    }

    private void DampenSoundtrackVolume()
    {
        if (soundtrackAudioSource != null)
        {
            soundtrackAudioSource.volume = 0.5f;
        }
    }

    private void RestoreSoundtrackVolume()
    {
        if (soundtrackAudioSource != null)
        {
            soundtrackAudioSource.volume = 1.0f;
        }
    }

    [Header("Debug")]
    [SerializeField] private bool showDebugState = true;
    [SerializeField] private StockDefinition[] _debugOwnedStocks;

    private void Update() {
        #if UNITY_EDITOR
        if (showDebugState)
            _debugOwnedStocks = playerState.OwnedStocks.ToArray();
        #endif
    }
}
