using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadThisSceneName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadThisSceneNumber(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }

    public void RestartThisScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
