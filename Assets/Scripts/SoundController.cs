using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] AudioSource fireAudio;


    public void Fire()
    {
        fireAudio.Play();
    }

}
