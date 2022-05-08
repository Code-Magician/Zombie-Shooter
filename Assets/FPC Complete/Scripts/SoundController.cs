using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SCRIPT ATTACHED TO THE FPS MODEL WHICH TAKES CARE OF THE FIRE EVENT ON THE FPS ANIMATION...
public class SoundController : MonoBehaviour
{
    [SerializeField] AudioSource fireAudio;


    public void Fire()
    {
        fireAudio.Play();
    }

    public void FireAnimationFinished()
    {
        GameStats.canShoot = true;
    }
}
