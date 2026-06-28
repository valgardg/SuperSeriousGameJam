using System.Collections;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    public SlotGrid slotGrid;
    public SlotGenerator slotGenerator;
    public float spinSpeed = 0.08f;
    public float finalSpinSpeed = 1.5f;
    public float timeBetweenColumnSpin = 0.3f;
    [SerializeField] private SlotCell cellPrefab;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public AudioSource startSpinSound;
    public AudioSource slotSpinningSound;
    // actions
    public static event System.Action OnSpinEnded;

    public bool spinColumnOne = false;

    public int shiftsPerSpin = 25;

    private void Start()
    {
        slotGrid.GenerateGrid();
        // StartCoroutine(ShiftColumnsCoroutine());
    }

    public void Update()
    {
        
    }

    public void CallSpinAction()
    {
        StartCoroutine(SpinAction());

        if (startSpinSound != null)
            startSpinSound.Play();
    }

    public IEnumerator SpinAction()
    {
        Coroutine lastColumnCoroutine = null;
        for (int i = 0; i < slotGrid.columns; i++)
        {
            lastColumnCoroutine = StartCoroutine(SpinColumn(i));
            yield return new WaitForSeconds(timeBetweenColumnSpin);
        }

        yield return lastColumnCoroutine; // waits for the last SpinColumn to finish

        if (slotSpinningSound != null)
            slotSpinningSound.Stop();

        OnSpinEnded?.Invoke();
    }

    public IEnumerator SpinColumn(int columnIndex)
    {
        PlayColumnSpinSound();

        for (int i = 0; i < shiftsPerSpin; i++)
        {
            float t = i / (float)(shiftsPerSpin - 1);
            float curveValue = spinCurve.Evaluate(t);
            float delay = Mathf.Lerp(spinSpeed, finalSpinSpeed, curveValue);

            // Only use the fill context for the final 4 cells that will actually land
            bool isFinalCell = i >= shiftsPerSpin - slotGrid.rows;
            ShiftColumnOnceCoroutine(columnIndex);

            yield return new WaitForSeconds(delay);
        }
    }

    private void PlayColumnSpinSound()
    {
        if (slotSpinningSound == null || slotSpinningSound.clip == null)
            return;

        slotSpinningSound.PlayOneShot(slotSpinningSound.clip);
    }

    private void OnDisable()
    {
        if (slotSpinningSound != null)
            slotSpinningSound.Stop();
    }

    private void ShiftColumnOnceCoroutine(int columnIndex)
    {
        SlotCell generatedSlotCell = slotGenerator.GenerateSlotCell();
        slotGrid.AddSlotToColumn(columnIndex, generatedSlotCell);
    }
}
