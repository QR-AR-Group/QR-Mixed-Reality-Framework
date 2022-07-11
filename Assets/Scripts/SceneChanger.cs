using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance;
    private static List<string> sceneList = new List<string>();
    private static int currentSceneIndex = 0;

    public void Init()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sceneList.Add(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void LoadScene(string sceneName)
    {
        sceneList.Add(sceneName);
        ++currentSceneIndex;
        SceneManager.LoadScene(sceneName);
    }

    public static void GoToPreviousScene()
    {
        if (currentSceneIndex > 0)
        {
            sceneList.RemoveAt(currentSceneIndex);
            --currentSceneIndex;
            SceneManager.LoadScene(sceneList[currentSceneIndex]);
        }
    }
}