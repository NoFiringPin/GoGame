using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class CubeGrid : MonoBehaviour
{
    public GameObject playerTile;
    public GameObject computerTile;
    public GameObject gridTilePrefab;
    public string puzzleName;
    public string presetName;
    public int gridSize = 5;

    private List<GameObject> cubeObjects = new List<GameObject>();
    private List<string> allowedCubes = new List<string>();
    private List<string> previousIllegalMoves = new List<string>();
    private Dictionary<string,string> presetMoves = new Dictionary<string,string>();
    public event Action OnGameOver;


    private bool playerTurn = true;
    private bool gameEnded = false;
    private bool aiProcessing = false;
    private int[,] boardState;

    [SerializeField]private CaptureManager captureManager; // Reference to CaptureManager
    [SerializeField] private GameManager gameManager;


    public List<string> illegalMoves = new List<string>(); // Public list of illegal moves as strings

    public List<GameObject> CubeObjects => cubeObjects;
    public List<string> AllowedCubes => allowedCubes;

    public List<GameObject> GetCubeObjects ()
    {
        return cubeObjects;
    }

    void Start ()
    {
        boardState = new int[gridSize,gridSize];
        InitializeGrid();
        LoadAllowedMoves($"{Application.dataPath}/TurnPresets/{presetName}.json");
        LoadBoardConfiguration($"{Application.dataPath}/BoardPuzzles/{puzzleName}.json");

        gameManager = GameObject.FindGameObjectWithTag("GameManager")?.GetComponent<GameManager>();
        captureManager = FindObjectOfType<CaptureManager>();
        Debug.Log("Illegal Moves List: " + string.Join(", ",illegalMoves));

        if (captureManager == null)
        {
            Debug.LogError("CubeGrid: CaptureManager not found!");
        }

        StartCoroutine(MonitorIllegalMoves());

    }

    public void InitializeGrid ()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                Vector3 position = new Vector3((j - 1),0,(i - 1));
                GameObject gridTile = Instantiate(gridTilePrefab,position,Quaternion.identity);
                gridTile.name = $"({i},{j})";
                gridTile.transform.parent = this.transform;

                // Check if the tile is an illegal move
                string coords = $"{i},{j}"; // Format the tile coordinates as "row,column"
                if (illegalMoves.Contains(coords))
                {
                    gridTile.GetComponent<Collider>().enabled = false; // Disable interaction
                    gridTile.GetComponent<Renderer>().material.color = Color.red; // Optional: visually mark as disabled
                    Debug.Log($"Tile ({i}, {j}) disabled as it's an illegal move.");
                }

                cubeObjects.Add(gridTile);
            }
        }

        Debug.Log($"Grid initialized with size: {gridSize}x{gridSize}");
    }


    private void LoadAllowedMoves (string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PresetMovesData data = JsonUtility.FromJson<PresetMovesData>(json);

            foreach (var move in data.moves)
            {
                (int px, int py) = ParseTileName(move.playerMove);
                (int ax, int ay) = ParseTileName(move.aiMove);

                if (IsMoveLegal(px,py) && IsMoveLegal(ax,ay))
                {
                    presetMoves.Add(move.playerMove,move.aiMove);
                    allowedCubes.Add(move.playerMove);
                    Debug.Log($"Loaded Move: Player Move - {move.playerMove} -> AI Move - {move.aiMove}");
                }
                else
                {
                    Debug.LogWarning($"Illegal move in presets: Player Move - {move.playerMove}, AI Move - {move.aiMove}");
                }
            }
        }
        else
        {
            Debug.LogError("Preset moves file not found at path: " + path);
        }
    }


    private void LoadBoardConfiguration (string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            BoardData data = JsonConvert.DeserializeObject<BoardData>(json);
            gridSize = data.boardSize;

            boardState = new int[gridSize,gridSize];
            int index = 0;

            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    boardState[y,x] = data.boardFlat[index++];

                    string tileName = $"({y + 1},{x + 1})";
                    GameObject gridTile = cubeObjects.Find(cube => cube.name == tileName);

                    if (gridTile != null)
                    {
                        if (boardState[y,x] == 1)
                        {
                            Vector3 tilePosition = gridTile.transform.position + Vector3.up * 0.5f;
                            GameObject stone = Instantiate(playerTile,tilePosition,Quaternion.identity);
                            stone.transform.parent = gridTile.transform;
                        }
                        else if (boardState[y,x] == 2)
                        {
                            Vector3 tilePosition = gridTile.transform.position + Vector3.up * 0.5f;
                            GameObject stone = Instantiate(computerTile,tilePosition,Quaternion.identity);
                            stone.transform.parent = gridTile.transform;
                        }
                    }
                }
            }

            Debug.Log("Board configuration loaded and stones initialized.");
        }
        else
        {
            Debug.LogError("Board configuration file not found!");
        }
    }

    public void SaveBoardStateToJson (string filePath)
    {
        BoardData boardData = new BoardData
        {
            boardFlat = new int[gridSize * gridSize],
            boardSize = gridSize
        };

        int index = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                boardData.boardFlat[index++] = boardState[y,x];
            }
        }

        string json = JsonUtility.ToJson(boardData,true);
        File.WriteAllText(filePath,json);

        Debug.Log($"Board state saved to {filePath}");
    }

    public void LoadBoardStateFromJson (string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        BoardData boardData = JsonUtility.FromJson<BoardData>(json);

        if (boardData.boardSize != gridSize)
        {
            Debug.LogError("Mismatch between board sizes. JSON file might be corrupted.");
            return;
        }

        int index = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                boardState[y,x] = boardData.boardFlat[index++];
            }
        }

        Debug.Log($"Board state loaded from {filePath}");
        RefreshBoardVisuals(); // Update the board visuals based on the loaded state
    }


    public bool IsPlayerTurn ()
    {
        return playerTurn && !gameEnded && !aiProcessing;
    }

    public int CountAIPieces ()
    {
        int count = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (boardState[y,x] == 2) // 2 represents the AI's pieces
                {
                    count++;
                }
            }
        }
        return count;
    }

    public int CountPlayerPieces ()
    {
        int count = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (boardState[y,x] == 1) // 1 represents the player's pieces
                {
                    count++;
                }
            }
        }
        return count;
    }

    public bool PlacePlayerTile (GameObject gridTile)
    {
        if (!playerTurn || aiProcessing || gameEnded)
        {
            Debug.LogError("Invalid move: Not the player's turn or the game has ended.");
            return false;
        }

        string tileName = gridTile.name;
        (int x, int y) = ParseTileName(tileName);

        Debug.Log($"Attempting to place player tile at ({x + 1}, {y + 1})");

        // Check if the move is legal
        if (!IsMoveLegal(x,y))
        {
            Debug.LogWarning($"Illegal move attempt at ({x + 1}, {y + 1})");
            return false; // Abort the tile placement if the move is illegal
        }

        // If the move is legal, place the player's stone
        Vector3 tilePosition = gridTile.transform.position + Vector3.up * 0.5f;
        GameObject stone = Instantiate(playerTile,tilePosition,Quaternion.identity);
        stone.transform.parent = gridTile.transform;

        boardState[y,x] = 1;

        if (captureManager != null)
        {
            captureManager.CheckForCaptures();
        }

        // Increment the turn count in GameManager
        if (gameManager != null)
        {
            gameManager.turnCount++;
            gameManager.UpdateTurnCounter();
        }

        // Switch to AI's turn
        playerTurn = false;
        aiProcessing = true;

        // Check for preset AI move
        if (presetMoves.TryGetValue(tileName,out string aiMove))
        {
            StartCoroutine(AIDelayedMove(aiMove));
        }
        else
        {
            Debug.LogError("AI move not found.");
            playerTurn = true;
            aiProcessing = false;
        }

        return true;
    }




    public void PlaceStoneAt (int x,int y,int player)
    {
        // Ensure coordinates are within bounds
        if (x < 0 || y < 0 || x >= gridSize || y >= gridSize)
        {
            Debug.LogError($"Invalid coordinates: ({x}, {y})");
            return;
        }

        string tileName = $"({y + 1},{x + 1})"; // Convert to 1-based naming
        GameObject gridTile = cubeObjects.Find(tile => tile.name == tileName);

        if (gridTile != null)
        {
            // Remove existing stones
            foreach (Transform child in gridTile.transform)
            {
                Destroy(child.gameObject);
            }

            // Place a new stone if player > 0
            if (player == 1) // Black
            {
                Vector3 tilePosition = gridTile.transform.position + Vector3.up * 0.5f;
                GameObject stone = Instantiate(playerTile,tilePosition,Quaternion.identity);
                stone.transform.parent = gridTile.transform;
            }
            else if (player == 2) // White
            {
                Vector3 tilePosition = gridTile.transform.position + Vector3.up * 0.5f;
                GameObject stone = Instantiate(computerTile,tilePosition,Quaternion.identity);
                stone.transform.parent = gridTile.transform;
            }

            // Update the internal board state
            boardState[y,x] = player;

            Debug.Log($"Placed stone at ({x}, {y}) for player: {player}");
        }

        if (captureManager != null)
        {
            captureManager.CheckForCaptures();
        }

        else
        {
            Debug.LogError($"Grid tile not found for coordinates: ({x}, {y})");
        }
    }

    private void ProcessAITurn (string aiMove)
    {
        StartCoroutine(AIDelayedMove(aiMove));
    }

    private IEnumerator AIDelayedMove (string intendedMove)
    {
        yield return new WaitForSeconds(1.0f);

        if (!HasValidMoves(2))
        {
            Debug.Log("No more valid moves available for the AI.");
            EndGame();
            yield break;
        }

        string aiMove = FindValidAIMove(intendedMove);

        if (aiMove == null)
        {
            Debug.Log("AI has no valid moves left.");
            EndGame();
            yield break;
        }

        (int x, int y) = ParseTileName(aiMove);

        // Validate the move
        if (!IsMoveLegal(x,y))
        {
            Debug.LogError($"AI attempted an illegal move at ({x + 1}, {y + 1})");
            playerTurn = true;
            aiProcessing = false;
            yield break;
        }

        // Temporarily place the stone to validate it
        boardState[y,x] = 2;

        if (!HasLiberties(x,y,2))
        {
            Debug.LogWarning($"AI attempted an illegal move at ({x + 1}, {y + 1}). Reverting.");
            boardState[y,x] = 0; // Revert the move
            playerTurn = true;
            aiProcessing = false;
            yield break;
        }

        // If the move is valid, finalize it
        GameObject aiTile = cubeObjects.Find(cube => cube.name == aiMove);
        if (aiTile != null)
        {
            Vector3 tilePosition = aiTile.transform.position + Vector3.up * 0.5f;
            GameObject stone = Instantiate(computerTile,tilePosition,Quaternion.identity);
            stone.transform.parent = aiTile.transform;

            Debug.Log($"AI placed a stone at ({x + 1}, {y + 1}).");

            // Check for captures
            if (captureManager != null)
            {
                captureManager.CheckForCaptures();
            }
        }

        // Switch turn back to the player
        playerTurn = true;
        aiProcessing = false;
    }




    private void RemoveStoneAt (int x,int y)
    {
        string tileName = $"({y + 1},{x + 1})"; // Convert to 1-based naming
        GameObject gridTile = cubeObjects.Find(cube => cube.name == tileName);

        if (gridTile != null)
        {
            foreach (Transform child in gridTile.transform)
            {
                Destroy(child.gameObject);
            }

            boardState[y,x] = 0; // Update the board state
            Debug.Log($"Stone removed at ({x}, {y}).");
        }
    }

    private bool HasLiberties (int x,int y,int player)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> group = new List<Vector2Int>();

        return captureManager.FloodFill(x,y,player,visited,group);
    }


    private bool HasValidMoves (int player)
    {
        if (player == 1) // Player
        {
            foreach (var move in presetMoves.Keys)
            {
                (int x, int y) = ParseTileName(move);
                if (IsMoveLegal(x,y) && boardState[y,x] == 0)
                {
                    return true;
                }
            }
        }
        else if (player == 2) // AI
        {
            foreach (var move in presetMoves.Values)
            {
                (int x, int y) = ParseTileName(move);
                if (IsMoveLegal(x,y) && boardState[y,x] == 0)
                {
                    return true;
                }
            }
        }

        return false; // No valid moves available
    }

    public bool HasPlayerValidMoves ()
    {
        foreach (var move in presetMoves.Keys)
        {
            (int x, int y) = ParseTileName(move);
            if (IsMoveLegal(x,y) && boardState[y,x] == 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasAIValidMoves ()
    {
        foreach (var move in presetMoves.Values)
        {
            (int x, int y) = ParseTileName(move);
            if (IsMoveLegal(x,y) && boardState[y,x] == 0)
            {
                return true;
            }
        }
        return false;
    }



    private bool IsMoveLegal (int x,int y)
    {
        Debug.Log($"Checking legality of move at ({x + 1}, {y + 1})");

        if (x < 0 || y < 0 || x >= gridSize || y >= gridSize)
        {
            Debug.LogWarning($"Move out of bounds at ({x}, {y})");
            return false;
        }

        if (boardState[y,x] != 0)
        {
            Debug.LogWarning($"Move attempted on occupied tile at ({x}, {y})");
            return false;
        }

        // Check if the move is in the illegal moves list
        string coords = $"{y + 1},{x + 1}"; // Format as "row,column" (1-based indexing)
        if (illegalMoves.Contains(coords))
        {
            Debug.LogWarning($"Move attempted on illegal tile at ({x + 1}, {y + 1})");
            return false;
        }

        return true;
    }

    private string FindValidAIMove (string intendedMove)
    {
        // Parse the intended move
        (int x, int y) = ParseTileName(intendedMove);

        // Format the coordinates as "row,column"
        string intendedCoords = $"{y + 1},{x + 1}";

        // Check if the intended move is legal and not in the illegalMoves list
        if (IsMoveLegal(x,y) && !illegalMoves.Contains(intendedCoords))
        {
            return intendedMove;
        }

        // Iterate through all possible preset AI moves
        foreach (var move in presetMoves.Values)
        {
            (x, y) = ParseTileName(move);

            // Format the coordinates as "row,column"
            string coords = $"{y + 1},{x + 1}";

            // Skip moves that are illegal or in the illegalMoves list
            if (IsMoveLegal(x,y) && !illegalMoves.Contains(coords))
            {
                return move;
            }
        }

        // No valid moves found
        return null;
    }



    private bool IsTileValidForAI (string tileName)
    {
        (int x, int y) = ParseTileName(tileName);

        // Check if the tile is within bounds, unoccupied, and part of the preset moves
        return x >= 0 && y >= 0 && x < gridSize && y < gridSize && boardState[y,x] == 0;
    }

    private (int, int) ParseTileName (string name)
    {
        string[] parts = name.Trim('(',')').Split(',');
        return (int.Parse(parts[1]) - 1, int.Parse(parts[0]) - 1); // Adjusted order
    }

    private void EndGame ()
    {
        gameEnded = true;
        OnGameOver?.Invoke(); // Trigger the event
        Debug.Log("Game has ended. No more valid moves for either player or AI.");
    }

    public int[,] GetBoardState ()
    {
        return boardState;
    }

    private string GetBoardStateString ()
    {
        string board = "";
        for (int y = 0; y < boardState.GetLength(0); y++)
        {
            for (int x = 0; x < boardState.GetLength(1); x++)
            {
                board += boardState[y,x] + " ";
            }
            board += "\n";
        }
        return board;
    }


    public void UpdateBoardState (int[,] newBoardState)
    {
        if (newBoardState == null || newBoardState.GetLength(0) != gridSize || newBoardState.GetLength(1) != gridSize)
        {
            Debug.LogError("UpdateBoardState: Invalid board state provided.");
            return;
        }

        boardState = newBoardState;

        Debug.Log("Updated board state. Verifying contents:");
        for (int y = 0; y < gridSize; y++)
        {
            string row = "";
            for (int x = 0; x < gridSize; x++)
            {
                row += boardState[y,x] + " ";
            }
            Debug.Log(row);
        }

        RefreshBoardVisuals();
    }




    private void RefreshBoardVisuals ()
    {
        Debug.Log("Refreshing board visuals...");
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < boardState.GetLength(0); y++)
        {
            for (int x = 0; x < boardState.GetLength(1); x++)
            {
                string tileName = $"({y + 1},{x + 1})";
                GameObject gridTile = cubeObjects.Find(cube => cube.name == tileName);

                if (gridTile != null)
                {
                    if (boardState[y,x] == 1)
                    {
                        Vector3 tilePosition = gridTile.transform.position + Vector3.up * 0.5f;
                        GameObject stone = Instantiate(playerTile,tilePosition,Quaternion.identity);
                        stone.transform.parent = gridTile.transform;
                    }
                    else if (boardState[y,x] == 2)
                    {
                        Vector3 tilePosition = gridTile.transform.position + Vector3.up * 0.5f;
                        GameObject stone = Instantiate(computerTile,tilePosition,Quaternion.identity);
                        stone.transform.parent = gridTile.transform;
                    }
                }
            }
        }
        Debug.Log("Board visuals refreshed.");
    }

    private void UpdateIllegalTiles ()
    {
        foreach (GameObject tile in cubeObjects)
        {
            // Parse the tile name to get (row, column)
            string[] parts = tile.name.Trim('(',')').Split(',');
            int row = int.Parse(parts[0]);
            int column = int.Parse(parts[1]);

            string coords = $"{row},{column}"; // Format the coordinates as "row,column"
            if (illegalMoves.Contains(coords))
            {
                tile.SetActive(false); // Deactivate the tile
                Debug.Log($"Tile at ({row}, {column}) disabled as an illegal move.");
            }
            else
            {
                tile.SetActive(true); // Reactivate the tile
                Collider collider = tile.GetComponent<Collider>();
                if (collider != null)
                    collider.enabled = true;

                Debug.Log($"Tile at ({row}, {column}) reactivated and available for play.");
            }
        }
    }




    public void RemoveIllegalMove (string coordinate)
    {
        if (illegalMoves.Contains(coordinate))
        {
            illegalMoves.Remove(coordinate);
            Debug.Log($"Removed illegal move at {coordinate}.");
            UpdateIllegalTiles(); // Refresh tile states
        }
        else
        {
            Debug.LogWarning($"Coordinate {coordinate} is not in the illegalMoves list.");
        }
    }

    private IEnumerator MonitorIllegalMoves ()
    {
        List<string> previousIllegalMoves = new List<string>(illegalMoves);

        while (true)
        {
            if (!AreListsEqual(illegalMoves,previousIllegalMoves))
            {
                Debug.Log("Illegal moves list has changed. Updating tiles...");
                UpdateIllegalTiles();
                previousIllegalMoves = new List<string>(illegalMoves);
            }

            yield return new WaitForSeconds(0.5f); // Periodically check for changes
        }
    }

    private bool AreListsEqual (List<string> list1,List<string> list2)
    {
        if (list1.Count != list2.Count)
            return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i])
                return false;
        }

        return true;
    }

    [System.Serializable]
    public class BoardData
    {
        public int[] boardFlat;
        public int boardSize;
    }

    [System.Serializable]
    public class PresetMovesData
    {
        public List<Move> moves;
    }
}
