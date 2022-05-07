using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] float pitch = 1;
    [SerializeField] float minWait;
    [SerializeField] float maxWait;

    private void Start()
    {
        InvokeRepeating("PlaySound", 2f, Random.Range(minWait, maxWait));
    }

    private void PlaySound()
    {
        GameObject temp = new GameObject();
        AudioSource tAS = temp.AddComponent<AudioSource>();
        tAS.clip = audioClip;
        tAS.Play();

        Destroy(temp, audioClip.length);
    }
}
