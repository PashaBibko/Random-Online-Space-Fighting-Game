using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneController
{
    // Forces it to always open on the main menu //
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] private static void Init()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            //SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }

    // Wrapper around the Network Manager to load scenes //
    public static void Load(string name) => NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
}
