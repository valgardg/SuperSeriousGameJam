using UnityEngine;

public class LandlordController : MonoBehaviour
{
    public DayInfo[] dayInfos;

    public static LandlordController Instance { get; private set; }

    public int NextRentDueAmount { get; private set; }

    public static event System.Action<DayInfo> OnLandlordEventTriggered;

    private int nextRentIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        dayInfos = Resources.LoadAll<DayInfo>("Day Info");
        System.Array.Sort(dayInfos, (a, b) => a.triggerDay.CompareTo(b.triggerDay));

        nextRentIndex = 0;
        UpdateNextRentDueAmount();
    }

    public int CheckTriggerDayInfo(int day)
    {
        while (nextRentIndex < dayInfos.Length
            && dayInfos[nextRentIndex].triggerDay < day)
            nextRentIndex++;

        if (nextRentIndex >= dayInfos.Length
            || dayInfos[nextRentIndex].triggerDay != day)
        {
            UpdateNextRentDueAmount();
            return 0;
        }

        DayInfo currentRent = dayInfos[nextRentIndex];
        nextRentIndex++;
        UpdateNextRentDueAmount();
        OnLandlordEventTriggered?.Invoke(currentRent);
        return currentRent.rentPrice;
    }

    private void UpdateNextRentDueAmount()
    {
        NextRentDueAmount = nextRentIndex < dayInfos.Length
            ? dayInfos[nextRentIndex].rentPrice
            : 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
