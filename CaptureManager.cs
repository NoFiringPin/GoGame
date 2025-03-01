using System.Collections.Generic;
using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    [SerializeField]private CubeGrid cubeGrid; // Reference to the CubeGrid
    private int[,] boardState;
    private int gridSize;

    private List<GameObject> cubeObjects;


    private readonly int[,] directions = new int[,]
    {
        { 0, 1 },  // Up
        { 0, -1 }, // Down
        { 1, 0 },  // Right
        { -1, 0 }  // Left
    };

    private void Start ()
    {
        cubeGrid = FindObjectOfType<CubeGrid>();

        if (cubeGrid != null)
        {
            boardState = cubeGrid.GetBoardState();
            gridSize = cubeGrid.gridSize;
            cubeObjects = cubeGrid.GetCubeObjects(); 
            Debug.Log("Cube grid found!");
        }
        else
        {
            Debug.LogError("CaptureManager: CubeGrid not found!");
        }
    }

    public void CheckForCaptures ()
    {
        if (cubeGrid == null)
        {
            Debug.LogError("CaptureManager: CubeGrid reference is missing.");
            return;
        }

        boardState = cubeGrid.GetBoardState();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        for (int y = 0; y < boardState.GetLength(0); y++)
        {
            for (int x = 0; x < boardState.GetLength(1); x++)
            {
                int player = boardState[y,x];
                if (player != 0 && !visited.Contains(new Vector2Int(x,y)))
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    bool hasLiberties = FloodFill(x,y,player,visited,group);

                    if (!hasLiberties)
                    {
                        Debug.Log($"Group at ({x}, {y}) is captured. Removing pieces.");
                        RemoveCapturedStones(group);
                    }
                }
            }
        }
    }


    public bool FloodFill (int startX,int startY,int player,HashSet<Vector2Int> visited,List<Vector2Int> group)
    {
        Queue<Vector2Int> toVisit = new Queue<Vector2Int>();
        toVisit.Enqueue(new Vector2Int(startX,startY));
        bool hasLiberties = false;

        while (toVisit.Count > 0)
        {
            Vector2Int current = toVisit.Dequeue();

            if (visited.Contains(current))
                continue;

            visited.Add(current);
            group.Add(current);

            for (int d = 0; d < 4; d++)
            {
                int nx = current.x + directions[d,0];
                int ny = current.y + directions[d,1];

                if (nx < 0 || ny < 0 || nx >= gridSize || ny >= gridSize)
                    continue;

                if (boardState[ny,nx] == 0) // Liberty found
                {
                    hasLiberties = true;
                    Debug.Log($"Liberty found at ({nx}, {ny}) for group starting at ({startX}, {startY}).");

                }
                else if (boardState[ny,nx] == player && !visited.Contains(new Vector2Int(nx,ny)))
                {
                    Debug.Log($"Adding stone at ({nx}, {ny}) to the group for further checking.");
                    toVisit.Enqueue(new Vector2Int(nx,ny));
                }
            }
        }
        Debug.Log($"FloodFill at ({startX}, {startY}) for player {player}");
        return hasLiberties;
    }



    private void RemoveCapturedStones (List<Vector2Int> group)
    {
        foreach (Vector2Int pos in group)
        {
            if (pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize && boardState[pos.y,pos.x] != 0)
            {
                boardState[pos.y,pos.x] = 0; // Update the board state
                string tileName = $"({pos.y + 1},{pos.x + 1})";
                GameObject gridTile = cubeGrid.CubeObjects.Find(cube => cube.name == tileName);

                if (gridTile != null)
                {
                    foreach (Transform child in gridTile.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }

                // Add the position to the illegal moves list
                string coords = $"{pos.y + 1},{pos.x + 1}"; // Convert to "row,column"
                if (!cubeGrid.illegalMoves.Contains(coords))
                {
                    cubeGrid.illegalMoves.Add(coords);
                    Debug.Log($"Added {coords} to illegal moves due to capture.");
                }
            }
        }

        // Save the updated board state to JSON
        cubeGrid.SaveBoardStateToJson(Application.persistentDataPath + "/TempBoardState.json");

        Debug.Log("Captured stones removed and board state saved.");
    }





    public void DebugBoardState ()
    {
        string boardVisual = "";
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                boardVisual += boardState[y,x] + " ";
            }
            boardVisual += "\n";
        }
        Debug.Log($"Current Board State:\n{boardVisual}");
    }

}
