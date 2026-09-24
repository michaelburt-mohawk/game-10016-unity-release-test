using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;

    public Vector2Int position;

    public bool freeze = false;

    int activeCellCount = -1;

    public void Initialize(Board board, Tetronimo tetronimo)
    {
        // set a reference to the board object
        this.board = board;

        // search for the tetromino data and assign
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        // create a copy of the tetronimo cell locations
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0; i < data.cells.Length; i++) cells[i] = data.cells[i];

        // set the start position of the piece
        position = board.startPosition;

        activeCellCount = cells.Length;
    }

    private void Update()
    {
        if (board.tetrisManager.gameOver) return;
        if (freeze) return;

        board.Clear(this);

        // hard drop takes priority
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else
        {
            // if hard drop didn't happen, do other transformations
            if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);

            if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);

            if (Input.GetKeyDown(KeyCode.LeftArrow)) Rotate(1);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) Rotate(-1);
        }

        board.Set(this);

        // TBD debug only - debug key is P
        if (Input.GetKeyDown(KeyCode.P))
        {
            board.CheckBoard();
        }
        
        // checking the board must come AFTER board.set
        if (freeze)
        {
            board.CheckBoard();
            board.SpawnPiece();
        }

    }

    void Rotate(int direction)
    {
        // copy cells in case we need to revert
        Vector2Int[] originalCells = new Vector2Int[cells.Length];

        for (int i = 0; i < cells.Length; i++) originalCells[i] = cells[i];

        ApplyRotation(direction);

        if (!board.IsPositionValid(this, position))
        {
            // if position not valid, try wall kicks
            if (!TryWallKicks())
            {
                // all wall kicks have failed at this point, so revert the rotation
                RevertRotation(originalCells);
            }
            else Debug.Log("Wall kick succeeded");
        }
        else Debug.Log("Rotation successful");
    }

    void RevertRotation(Vector2Int[] originalCells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = originalCells[i];
        }
    }

    bool TryWallKicks()
    {
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1), // diagonal down-left
            new Vector2Int(1, -1), // diagonal down-right
        };

        if (data.tetronimo == Tetronimo.I)
        {
            wallKickOffsets.Add(2 * Vector2Int.left);
            wallKickOffsets.Add(2 * Vector2Int.right);
        }

        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
        }

        return false;
    }

    void ApplyRotation(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 90 * direction);

        bool isSpecial = data.tetronimo == Tetronimo.I || data.tetronimo == Tetronimo.O;
        for (int i = 0; i < cells.Length; i++)
        {
            // convert cell location a Vector3 to work with quaternions
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            // fix the origin
            if (isSpecial)
            {
                cellPosition.x -= 0.5f;
                cellPosition.y -= 0.5f;
            }

            // get the result
            Vector3 result = rotation * cellPosition;

            // put it back in the cells data
            if (isSpecial)
            {
                cells[i].x = Mathf.CeilToInt(result.x);
                cells[i].y = Mathf.CeilToInt(result.y);
            }
            else
            {
                cells[i].x = Mathf.RoundToInt(result.x);
                cells[i].y = Mathf.RoundToInt(result.y);
            }
        }
    }

    void HardDrop()
    {
        // algorithm: repeatedly move down until we can't anymore

        while (Move(Vector2Int.down))
        {
            // do nothing
        }

        freeze = true;
    }

    // move ONLY moves the tetronimo if the position is valid
    // it also RETURNS whether or not the movement was valid
    public bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;
        newPosition += translation;

        bool positionValid = board.IsPositionValid(this, newPosition);
        if (positionValid) position = newPosition;

        return positionValid;
    }

    public void ReduceActiveCount()
    {
        activeCellCount -= 1;
        Debug.Log($"Tetrinomo {data.tetronimo} active cell count = {activeCellCount}");
        if (activeCellCount <= 0)
        {
            Debug.Log($"Tetronimo {data.tetronimo} destroyed");
            Destroy(gameObject);
        }
    }

}
