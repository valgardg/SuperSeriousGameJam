using System.Linq;
using UnityEngine;

public class LandlordController : MonoBehaviour
{
    public DayInfo[] dayInfos;

    public static LandlordController Instance { get; private set; }

    public int NextRentDueAmount { get; private set; }

    public static event System.Action<DayInfo> OnLandlordEventTriggered;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dayInfos = Resources.LoadAll<DayInfo>("Day Info");
        NextRentDueAmount = dayInfos.Length > 0 ? dayInfos.OrderBy(d => d.triggerDay).FirstOrDefault()?.rentPrice ?? 0 : 0;

    }

    public int CheckTriggerDayInfo(int day)
    {
        for (int i = 0; i < dayInfos.Length; i++)
        {
            DayInfo dayInfo = dayInfos[i];
            if (dayInfo.triggerDay == day)
            {
                OnLandlordEventTriggered?.Invoke(dayInfo);
                if (i + 1 < dayInfos.Length)
                {
                    Debug.Log($"Setting price to {dayInfos[i].rentPrice} for day info of day {i}");
                    NextRentDueAmount = dayInfos[i].rentPrice; // set price of next rent due
                }
                return dayInfo.rentPrice;
            }
        }
        return 0;
    }
}