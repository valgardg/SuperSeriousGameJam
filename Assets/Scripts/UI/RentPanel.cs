using TMPro;
using UnityEngine;

public class RentPanel : MonoBehaviour
{
    public TMP_Text landlordText;
    public TMP_Text rentPriceTex;

    private void Awake()
    {
        LandlordController.OnLandlordEventTriggered += ShowRentPanel;
        GameController.RentPaid += HideRentPanel;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        LandlordController.OnLandlordEventTriggered -= ShowRentPanel;
        GameController.RentPaid -= HideRentPanel;
    }

    public void ShowRentPanel(DayInfo dayInfo)
    {
        gameObject.SetActive(true);
        landlordText.text = dayInfo.landlordText;
        rentPriceTex.text = "" + dayInfo.rentPrice;
        Debug.Log($"Rent panel shown for day {dayInfo.triggerDay} with rent price {dayInfo.rentPrice}.");
    }

    public void HideRentPanel()
    {
        gameObject.SetActive(false);
    }
}
