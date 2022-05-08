using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


// WHEN THE LIFE OF PLAYER BECOMES ZERO IT LOADS THE MAIN MENU SCENE...
public class GameOverScript : MonoBehaviour
{
    [SerializeField] float delay;

    private void Start()
    {
        if (!GameStats.reachedHome)
            GetComponent<AudioSource>().enabled = false;

        if (GameStats.allLivesFinished || GameStats.reachedHome)
            StartCoroutine(GoToMainMenu(delay));
    }

    public IEnumerator GoToMainMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(0);
    }
}
