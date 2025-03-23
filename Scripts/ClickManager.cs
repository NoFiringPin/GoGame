using System.Collections;
using TMPro;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [SerializeField] private CubeGrid cubeGrid;
    [SerializeField] private TextMeshProUGUI errorMessage;

    void Start ()
    {
        GameObject cubeManager = GameObject.FindWithTag("CubeManager");

        if (cubeManager != null)
        {
            cubeGrid = cubeManager.GetComponent<CubeGrid>();

            if (cubeGrid == null)
            {
                Debug.LogError("Error: CubeManager found, but CubeGrid component is missing.");
            }
        }
        else
        {
            Debug.LogError("Error: No GameObject with tag 'CubeManager' found in the scene.");
        }

        if (errorMessage == null)
        {
            GameObject canvas = GameObject.Find("MainCanvas");

            if (canvas != null)
            {
                errorMessage = canvas.transform.Find("ErrorMessage")?.GetComponent<TextMeshProUGUI>();
                if (errorMessage != null)
                {
                    errorMessage.text = "";
                }
            }
            else
            {
                Debug.LogError("Error: No Canvas found with name 'MainCanvas'.");
            }
        }
    }

    void Update ()
    {
        if (!cubeGrid.IsPlayerTurn())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray,out RaycastHit hit))
            {
                GameObject clickedCube = hit.collider.gameObject;

                if (cubeGrid != null && cubeGrid.CubeObjects.Contains(clickedCube))
                {
                    if (cubeGrid.AllowedCubes.Contains(clickedCube.name))
                    {
                        bool success = cubeGrid.PlacePlayerTile(clickedCube);

                        if (!success)
                        {
                            ShowErrorMessage("Failed to place tile. Try again.");
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Move invalid, try again");
                    }
                }
            }
        }
    }

    private void ShowErrorMessage (string message)
    {
        if (errorMessage != null)
        {
            errorMessage.text = message;
            CancelInvoke("HideErrorMessage");
            Invoke("HideErrorMessage",1.75f);
        }
    }

    private void HideErrorMessage ()
    {
        if (errorMessage != null)
        {
            errorMessage.text = "";
        }
    }
}
