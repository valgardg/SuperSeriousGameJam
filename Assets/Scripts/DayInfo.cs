using UnityEngine;

[CreateAssetMenu(menuName = "Game/Day Info")]
public class DayInfo : ScriptableObject
{
    public int triggerDay;
    public int rentPrice;
    public string landlordText;
}