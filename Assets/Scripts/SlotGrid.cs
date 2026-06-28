using UnityEngine;

public class SlotGrid : MonoBehaviour
{
    [SerializeField] private SlotCell cellPrefab;
    public int columns = 5;
    public int rows = 4;
    [SerializeField] private Vector2 cellSpacing = new Vector2(1.25f, 1.25f);

    public SlotGenerator slotGenerator;

    public SlotCell[,] cells;

    // private void Start()
    // {
    //     GenerateGrid();
    // }

    public void GenerateGrid()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        cells = new SlotCell[columns, rows];

        Vector2 offset = new Vector2(
            (columns - 1) * cellSpacing.x / 2f,
            (rows - 1) * cellSpacing.y / 2f
        );

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                SlotCell cell = slotGenerator.GenerateEmptySlotCell();

                Vector2 pos = new Vector2(
                    x * cellSpacing.x - offset.x,
                    offset.y - y * cellSpacing.y
                );

                cell.transform.localPosition = pos;
                cells[x, y] = cell;
            }
        }
    }

    private Vector2 GetCellPosition(int x, int y)
    {
        Vector2 offset = new Vector2(
            (columns - 1) * cellSpacing.x / 2f,
            (rows - 1) * cellSpacing.y / 2f
        );

        Vector2 pos = new Vector2(
            x * cellSpacing.x - offset.x,
            offset.y - y * cellSpacing.y
        );

        return pos;
    }

    public SlotCell GetCell(int column, int row)
    {
        return cells[column, row];
    }

    public void ShiftColumn(int columnIndex)
    {
        SlotCell lastCell = cells[columnIndex, rows - 1];

        for (int row = rows - 1; row > 0; row--)
        {
            cells[columnIndex, row] = cells[columnIndex, row - 1];
            cells[columnIndex, row].transform.localPosition = GetCellPosition(columnIndex, row);
        }

        cells[columnIndex, 0] = lastCell;
        cells[columnIndex, 0].transform.localPosition = GetCellPosition(columnIndex, 0);
    }

    public void AddSlotToColumn(int columnIndex, SlotCell newCell)
    {
        SlotCell removedCell = cells[columnIndex, rows - 1];

        // Shift references down
        for (int row = rows - 1; row > 0; row--)
        {
            cells[columnIndex, row] = cells[columnIndex, row - 1];
        }

        // Insert new cell at top
        cells[columnIndex, 0] = newCell;

        // Reposition every cell after the array is correct
        for (int row = 0; row < rows; row++)
        {
            cells[columnIndex, row].transform.localPosition =
                GetCellPosition(columnIndex, row);
        }

        Destroy(removedCell.gameObject);
    }
}