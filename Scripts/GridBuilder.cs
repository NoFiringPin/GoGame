using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    public GameObject cubePrefab; // Assign your cube prefab in the inspector
    public int rows = 5; // Number of rows for the grid
    public int columns = 5; // Number of columns for the grid
    public float spacing = 1.5f; // Spacing between grid elements

    public CubeGrid cubeGrid; // Reference to CubeGrid in the scene

    void Start ()
    {
        BuildGrid();
    }

    void BuildGrid ()
    {
        for (int i = 1; i <= rows; i++)
        {
            for (int j = 1; j <= columns; j++)
            {
                // Corrected position: j is used for X axis (columns), i is used for Z axis (rows)
                Vector3 position = new Vector3((j - 1) * spacing,0,(i - 1) * spacing);

                // Instantiate the cube
                GameObject newCube = Instantiate(cubePrefab,position,Quaternion.identity);

                // Name the cube based on its grid position
                newCube.name = "(" + i + "," + j + ")";

                // Add the cube to CubeGrid's list of CubeObjects
                cubeGrid.CubeObjects.Add(newCube);

                // Parent the cube under the grid manager for a clean hierarchy
                newCube.transform.parent = this.transform;
            }
        }
    }

}
