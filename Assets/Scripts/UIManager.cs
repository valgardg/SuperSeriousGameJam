using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameController gameController;
    public PlayerStatsUI playerStatsUI;

    private void Start()
    {
        playerStatsUI.Init(gameController);
    }
}
