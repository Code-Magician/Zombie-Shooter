using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// SWITCHS THE SCENE AND TAKES CARE OF THE VOLUME SLIDER IN BOTH SCENES...
public class SceneSwitcher : MonoBehaviour
{
    public void GoToScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }







    // Scene Volume Change

    [SerializeField] Slider volumeSlider;
    AudioSource audioS;

    private void Awake()
    {
        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
        if (gameController == null) return;
        audioS = gameController.GetComponent<AudioSource>();
        volumeSlider.value = audioS.volume;
    }

    public void ChangeVolume()
    {
        if (audioS != null)
            audioS.volume = volumeSlider.value;
    }

}
