using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Move
{
    public string playerMove;  // Player move as string
    public string aiMove;      // AI move as string
}

[System.Serializable]
public class Turn
{
    public List<Move> moves;   // List of moves for each turn
}

public class PresetTurnManager : MonoBehaviour
{
    public string puzzleName = "newPuzzle";  // Name of the puzzle
    public List<Turn> presetTurns = new List<Turn>();  // List of turns

    private string filePath;

    void Start ()
    {
        filePath = $"{Application.dataPath}/BoardPuzzles/{puzzleName}.json";
    }

    // Converts the presetTurns into JSON format and saves it to a file
    public void SaveToJson ()
    {
        PresetTurnsData data = new PresetTurnsData();
        data.presetTurns = presetTurns;

        string json = JsonUtility.ToJson(data,true);  // Converts to JSON and formats it
        File.WriteAllText(filePath,json);

        Debug.Log($"Preset turns saved to {filePath}");
    }

    // Loads the JSON file back into the presetTurns structure
    public void LoadFromJson ()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PresetTurnsData data = JsonUtility.FromJson<PresetTurnsData>(json);
            presetTurns = data.presetTurns;

            Debug.Log("Preset turns loaded successfully");
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }

    // Nested class to hold the preset turns data for JSON serialization
    [System.Serializable]
    public class PresetTurnsData
    {
        public List<Turn> presetTurns;
    }
}
