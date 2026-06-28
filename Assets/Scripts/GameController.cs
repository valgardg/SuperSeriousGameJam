using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    public PlayerState playerState;
    public SlotCalculator slotCalculator;
    public SlotGenerator slotGenerator;

    public static event System.Action<StockDefinition[]> DisplayStockPicker;
    public static event System.Action RentPaid;

    public GameObject gameOverPanel;

    // game days information
    public int currentDay = 0;
    public LandlordController landlordController;
    private int latestRentPrice = 0;

    private void Start()
    {
        playerState = new PlayerState();

        SlotController.OnSpinEnded += HandleSpinEnded;
        StockOptionUI.OnStockOptionSelected += HandleStockOptionSelected;
        Debug.Log("Subscribed to OnSpinEnded event.");

        // hook up soundtrack adjustments
        HookSoundAdjustments();
    }

    private void HandleSpinEnded()
    {
        slotCalculator.StartCalculation(totalValue =>
        {
            Debug.Log($"Total value calculated: {totalValue}");
            playerState.money += totalValue;
            Debug.Log($"Player's new money: {playerState.money}");

            StockDefinition[] unknownStocks = slotGenerator.GetUnknownStocks();
            DisplayStockPicker?.Invoke(unknownStocks);
        });
    }

    private void HandleStockOptionSelected(StockDefinition selectedStock)
    {
        playerState.AddStock(selectedStock);
        Debug.Log($"Added stock: {selectedStock.name} to player's inventory.");
        currentDay++;
        latestRentPrice = landlordController.CheckTriggerDayInfo(currentDay);
    }

    public void AttemptToPayRent()
    {
        if (playerState.money >= latestRentPrice)
        {
            Debug.Log("Player paid off rent!");
            playerState.money -= latestRentPrice;
            RentPaid.Invoke();
        }
        else
        {
            Debug.Log("Player has lost!");
            gameOverPanel.SetActive(true);
            soundtrackAudioSource.Stop();
        }
    }

    public AudioSource soundtrackAudioSource;

    private void HookSoundAdjustments()
    {
        StockOptionUI.OnStockOptionSelected += (StockDefinition stockDefinition) => RestoreSoundtrackVolume();
        DisplayStockPicker += (StockDefinition[] stockDefinitions) => DampenSoundtrackVolume();
        LandlordController.OnLandlordEventTriggered += (DayInfo dayInfo) => DampenSoundtrackVolume();
        RentPaid += () => RestoreSoundtrackVolume();
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
