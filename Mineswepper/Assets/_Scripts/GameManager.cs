using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int Width = 16;
    public int Height = 16;
    public int MineCounts = 32;

    private Board _board;
    private Cell[,] _state;
    private bool _gameOver;

    private void OnValidate() {
        MineCounts = Mathf.Clamp(MineCounts, 0, Width * Height);
    }

    private void Awake()
    {
        _board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        _state = new Cell[Width, Height];
        _gameOver = false;
        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(Width / 2f, Height / 2f, -10f);
        _board.Draw(_state);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cell cell = new Cell();
                cell.Position = new Vector3Int(x, y, 0);
                cell.Type = Cell.TypeCell.Empty;
                _state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < MineCounts; i++)
        {
            int rX = Random.Range(0, Width);
            int rY = Random.Range(0, Height);

            while (_state[rX, rY].Type == Cell.TypeCell.Mine)
            {
                rX++;
                if (rX >= Width)
                {
                    rX = 0;
                    rY++;

                    if (rY >= Height)
                    {
                        rY = 0;
                    }
                }
            }

            _state[rX, rY].Type = Cell.TypeCell.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cell cell = _state[x, y];

                if (cell.Type == Cell.TypeCell.Mine)
                {
                    continue;
                }

                cell.Number = CountMines(x, y);

                if (cell.Number > 0)
                {
                    cell.Type = Cell.TypeCell.Number;
                }

                _state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (GetCell(x, y).Type == Cell.TypeCell.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }

        if (!_gameOver)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
        }

    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.Type == Cell.TypeCell.Invalid || cell.Revealed)
        {
            return;
        }

        cell.Flagged = !cell.Flagged;
        _state[cellPosition.x, cellPosition.y] = cell;
        _board.Draw(_state);
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.Type == Cell.TypeCell.Invalid || cell.Revealed || cell.Flagged)
        {
            return;
        }

        switch (cell.Type)
        {
            case Cell.TypeCell.Mine:
                Explode(cell);
                break;
            case Cell.TypeCell.Empty:
                Flood(cell);
                CheckWInCondition();
                break;
            default:
                cell.Revealed = true;
                _state[cellPosition.x, cellPosition.y] = cell;
                CheckWInCondition();
                break;
        }

        _board.Draw(_state);
    }

    private void Flood(Cell cell)
    {
        if (cell.Revealed) return;
        if (cell.Type == Cell.TypeCell.Mine || cell.Type == Cell.TypeCell.Invalid) return;

        cell.Revealed = true;
        _state[cell.Position.x, cell.Position.y] = cell;

        if (cell.Type == Cell.TypeCell.Empty)
        {
            Flood(GetCell(cell.Position.x - 1, cell.Position.y));
            Flood(GetCell(cell.Position.x + 1, cell.Position.y));
            Flood(GetCell(cell.Position.x, cell.Position.y - 1));
            Flood(GetCell(cell.Position.x, cell.Position.y + 1));
        }
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game Over!");
        _gameOver = true;

        cell.Revealed = true;
        cell.Exploded = true;
        _state[cell.Position.x, cell.Position.y] = cell;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cell = _state[x, y];

                if (cell.Type == Cell.TypeCell.Mine)
                {
                    cell.Revealed = true;
                    _state[x, y] = cell;
                }
            }
        }
    }

    private void CheckWInCondition()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cell cell = _state[x, y];

                if (cell.Type != Cell.TypeCell.Mine && !cell.Revealed)
                {
                    return;
                }
            }
        }

        Debug.Log("Winner!");
        _gameOver = true;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cell cell = _state[x, y];

                if (cell.Type == Cell.TypeCell.Mine)
                {
                    cell.Flagged = true;
                    _state[x, y] = cell;
                }
            }
        }
    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return _state[x, y];
        }
        else
        {
            return new Cell();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
