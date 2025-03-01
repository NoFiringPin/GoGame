using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region SINGLETON

    public static GameManager Instance;
    private void Awake () => Instance = this;

    #endregion

    [SerializeField] private bool ComputerOpponent = true;
    [SerializeField] private float timeToComplete;
    [SerializeField] private CubeGrid cubeGrid;
    private float remainingTime;

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI turnCounter;
    public TextMeshProUGUI turnIndicator;
    public Button passButton;
    public GameObject gameOverCanvas;
    public int turnCount = 0;


    public bool PlayerTurn = true; // Indicates whether it's the player's turn
    public int CurrentColour = 1;  // Start with Black by default (1 for Black, 2 for White)

    private int consecutivePasses = 0;

    public bool gameIsOver = false; // Flag to track if the game has ended

    private void Start ()
    {
        remainingTime = timeToComplete;
        UpdateTimerText();
        UpdateTurnCounter();
        UpdateTurnIndicator();
        passButton.interactable = true; // Ensure pass button is interactable at start

        if (cubeGrid != null)
        {
            cubeGrid.OnGameOver += HandleGameOver;
        }
    }

    private void Update ()
    {
        if (!gameIsOver) // Only update the timer if the game isn't over
        {
            GameTimer();
        }
    }

    public void PassTurn ()
    {
        consecutivePasses++;
        if (consecutivePasses >= 2)
        {
            Debug.Log("Both players passed. Ending game.");
            EndGame();
        }
        else
        {
            ChangeTurn();
        }
    }

    private void GameTimer ()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerText();

            if (remainingTime <= 0)
            {
                EndGame();
            }
        }
    }

    private void HandleGameOver ()
    {
        Debug.Log("GameManager detected game over.");
        EndGame();
    }

    private void OnDestroy ()
    {
        if (cubeGrid != null)
        {
            cubeGrid.OnGameOver -= HandleGameOver;
        }
    }

    /// <summary>
    /// Changes the game turn between colors and handles AI if needed.
    /// </summary>
    public void ChangeTurn ()
    {
        // Increment turn count at the start of every new turn
        turnCount++;
        UpdateTurnCounter();

        // Toggle the current color
        CurrentColour = CurrentColour == 1 ? 2 : 1;

        // Determine if it's the player's turn or the AI's turn
        if (ComputerOpponent && CurrentColour == 2) // It's AI's turn
        {
            PlayerTurn = false;
            passButton.interactable = false; // Disable the pass button during AI's turn
            Invoke("AIMove",UnityEngine.Random.Range(0.5f,1.5f)); // Delay the AI's move slightly
        }
        else // It's the player's turn
        {
            PlayerTurn = true;
            passButton.interactable = true; // Enable the pass button for the player's turn
            consecutivePasses = 0; // Reset consecutive passes for the player
        }

        UpdateTurnIndicator();
    }

    public void EndGame ()
    {
        Debug.Log("Game Over!");
        gameIsOver = true; // Set the game over flag to true
        gameOverCanvas.SetActive(true); // Display game-over screen
        passButton.interactable = false;
    }

    private void CheckForEndGame ()
    {
        if (!cubeGrid.HasPlayerValidMoves() && !cubeGrid.HasAIValidMoves())
        {
            EndGame();
            return;
        }
        else if (!cubeGrid.HasPlayerValidMoves())
        {
            Debug.Log("Game Over: AI wins as Player has no valid moves left.");
            EndGame();
            return;
        }
        else if (!cubeGrid.HasAIValidMoves())
        {
            Debug.Log("Game Over: Player wins as AI has no valid moves left.");
            EndGame();
            return;
        }
    }

    private void AIMove ()
    {
        if (!PlayerTurn) // Ensure that it's still the AI's turn
        {
            GoBoard.Instance.AIMove();
            ChangeTurn(); // Switch back to the player's turn after the AI move
        }
    }

    private void UpdateTimerText ()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60F);
        int seconds = Mathf.FloorToInt(remainingTime % 60F);
        timeText.text = string.Format("{0:00}:{1:00}",minutes,seconds);
    }

    public void UpdateTurnCounter ()
    {
        turnCounter.text = "Turn: " + turnCount;
    }

    private void UpdateTurnIndicator ()
    {
        string playerColor = CurrentColour == 1 ? "Black" : "White";
        if (ComputerOpponent && CurrentColour == 2)
        {
            turnIndicator.text = "AI (" + playerColor + ")'s Turn";
        }
        else
        {
            turnIndicator.text = playerColor + "'s Turn";
        }
    }
}
