using UnityEngine;

public class Dollar : MonoBehaviour
{
    [Header("Movement")]
    public Transform target;
    public float travelDuration = 0.8f;

    [Header("Arc")]
    [Tooltip("How high the arc peaks. Negative curves toward the screen left, positive toward right.")]
    public float arcHeight = 1.5f;
    public float arcSideOffset = 0.8f;

    private Vector3 _startPos;
    private Vector3 _controlPoint;
    private float _elapsed;
    private bool _moving;
    private int _value;
    private System.Action<int> _onArrived;
    private System.Action _onDespawned;
    private bool _despawnNotificationSent;

    public bool launchDollar = false;

    public Sprite redDollarSprite;
    
    /// <summary>
    /// Call this after instantiating to assign the target and kick off the flight.
    /// </summary>
    public void Launch(
        Transform destination,
        bool isNegative = false,
        System.Action<int> onArrived = null,
        System.Action onDespawned = null
    )
    {
        target = destination;
        _startPos = transform.position;
        _value = isNegative ? -1 : 1;
        _onArrived = onArrived;
        _onDespawned = onDespawned;
        _despawnNotificationSent = false;

        if (isNegative && redDollarSprite != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = redDollarSprite;
        }

        // Build a quadratic Bézier control point:
        // midpoint between start and target, lifted upward and offset slightly to the side.
        Vector3 mid = (_startPos + target.position) * 0.5f;
        _controlPoint = mid + new Vector3(arcSideOffset, arcHeight, 0f);

        _elapsed = 0f;
        _moving = true;
    }

    void Update()
    {
        if (launchDollar)
        {
            Launch(target);
            launchDollar = false;
        }

        if (!_moving || target == null) return;

        _elapsed += Time.deltaTime;

        // Normalised time [0..1]
        float t = Mathf.Clamp01(_elapsed / travelDuration);

        // Ease-in: t² makes it start slow and accelerate
        float easedT = t * t;

        // Quadratic Bézier:  B(t) = (1-t)²·P0  +  2(1-t)t·P1  +  t²·P2
        float oneMinusT = 1f - easedT;
        transform.position =
            oneMinusT * oneMinusT * _startPos +
            2f * oneMinusT * easedT * _controlPoint +
            easedT * easedT * target.position;

        if (t >= 1f)
        {
            _moving = false;
            OnArrived();
        }
    }

    private void OnArrived()
    {
        System.Action<int> callback = _onArrived;
        _onArrived = null;
        callback?.Invoke(_value);

        NotifyDespawned();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        NotifyDespawned();
    }

    private void NotifyDespawned()
    {
        if (_despawnNotificationSent)
            return;

        _despawnNotificationSent = true;
        System.Action callback = _onDespawned;
        _onDespawned = null;
        callback?.Invoke();
    }
}
