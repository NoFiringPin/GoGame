using UnityEngine;
using TMPro;

public class ConditionManager : MonoBehaviour
{
    [SerializeField] private CubeGrid cubeGrid;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI turnLimitText;
    [SerializeField] private bool showFinalScore = true;
    [SerializeField] private bool enableTurnLimit = false;
    [SerializeField] private int maxTurns = 0; // 0 = unlimited turns

    [Header("Score Win Conditions")]
    [SerializeField] private bool enableScoreLimit = false; // Enable/Disable score-based win condition
    [SerializeField] private int playerWinScore = 0; // 0 = disabled
    [SerializeField] private int aiWinScore = 0; // 0 = disabled

    private bool hasDisplayedScore = false;

    private void Start ()
    {
        if (cubeGrid == null)
        {
            cubeGrid = FindObjectOfType<CubeGrid>();
        }
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false); // Hide score UI initially
        }
        if (turnLimitText != null)
        {
            turnLimitText.gameObject.SetActive(enableTurnLimit); // Only show if enabled
        }
    }

    private void Update ()
    {
        if (gameManager == null)
            return;

        // Check Turn Limit Condition
        if (enableTurnLimit && maxTurns > 0 && gameManager.turnCount >= maxTurns)
        {
            Debug.Log("Turn limit reached. Game Over.");
            gameManager.EndGame();
        }

        // Check Score Win Condition
        if (enableScoreLimit)
        {
            int playerScore = cubeGrid.CountPlayerPieces();
            int aiScore = cubeGrid.CountAIPieces();

            if (playerWinScore > 0 && playerScore >= playerWinScore)
            {
                Debug.Log($"Player reached {playerWinScore} points. Player Wins!");
                CalculateFinalScores();
                gameManager.EndGame();
                return;
            }
            else if (aiWinScore > 0 && aiScore >= aiWinScore)
            {
                Debug.Log($"AI reached {aiWinScore} points. AI Wins!");
                CalculateFinalScores();
                gameManager.EndGame();
                return;
            }
        }

        // Detect Game Over and show final score
        if (gameManager.gameIsOver && !hasDisplayedScore)
        {
            CalculateFinalScores();
            hasDisplayedScore = true;
        }

        // Update Turn Limit Display
        if (turnLimitText != null && enableTurnLimit)
        {
            turnLimitText.text = $"Turns Left: {Mathf.Max(0,maxTurns - gameManager.turnCount)}";
        }
    }

    /// <summary>
    /// This function calculates and displays final scores when the game ends.
    /// </summary>
    private void CalculateFinalScores ()
    {
        if (cubeGrid == null)
        {
            Debug.LogError("ConditionManager: CubeGrid reference is missing!");
            return;
        }

        int playerScore = cubeGrid.CountPlayerPieces();
        int aiScore = cubeGrid.CountAIPieces();

        Debug.Log($"Final Score - Player: {playerScore} | AI: {aiScore}");

        if (scoreText != null && showFinalScore)
        {
            scoreText.text = $"Final Score:\nPlayer: {playerScore}\nAI: {aiScore}";
            scoreText.gameObject.SetActive(true);
        }
    }
}
