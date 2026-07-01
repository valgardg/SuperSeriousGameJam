using TMPro;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    public TMP_Text currentDayText;
    public TMP_Text playerMoneyText;
    public TMP_Text nextRentDueAmountText;

    private GameController gameController;

    public void Init(GameController gameController)
    {
        this.gameController = gameController;
    }

    private void Update()
    {
        if (gameController == null) return;

        currentDayText.text = $"Day: {gameController.currentDay}";
        playerMoneyText.text = $"{gameController.playerState.money}";
        nextRentDueAmountText.text = $"Upcoming rent: ${LandlordController.Instance.NextRentDueAmount}";

    }
}
