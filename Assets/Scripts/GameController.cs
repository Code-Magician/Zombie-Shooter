using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// A SINGLETON SCRIPT WHICH PESISTS BETWEEN THE SCENES..
public class GameController : MonoBehaviour
{
    public static GameController Instance;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        GameStats.gameOver = false;
    }

}
