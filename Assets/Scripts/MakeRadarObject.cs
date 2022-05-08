using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// SCRIPT TO REGISTER AN THIS.GAMEOBJECT AS A RADAR OBJECT. WHICH WILL BE SHOWN ON THE RADAR...
public class MakeRadarObject : MonoBehaviour
{
    [SerializeField] Image image;


    private void Start()
    {
        Radar.RegisterRadarObject(this.gameObject, image);
    }

    private void OnDestroy()
    {
        Radar.RemoveRadarObject(this.gameObject);
    }
}
