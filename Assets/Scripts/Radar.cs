using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// RADAR OBJECT WITH OWNER AS THAT GAMEOBJECT AND ICON AS AN IMAGE THAT WILL BE VISIBLE ON THE RADAR...
public class RadarObject
{
    public GameObject owner { get; set; }
    public Image icon { get; set; }

}

public class Radar : MonoBehaviour
{
    [SerializeField] Transform playerPos;
    public float mapScale = 2.0f;

    // RADAR OBJECTS THAT WILL BE VISIBLE ON THE RADAR...
    public static List<RadarObject> radObjects = new List<RadarObject>();

    public static void RegisterRadarObject(GameObject o, Image i)
    {
        Image image = Instantiate(i);
        radObjects.Add(new RadarObject() { owner = o, icon = image });
    }

    public static void RemoveRadarObject(GameObject o)
    {
        List<RadarObject> newList = new List<RadarObject>();

        for (int i = 0; i < radObjects.Count; i++)
        {
            if (radObjects[i].owner == o)
            {
                Destroy(radObjects[i].icon);
            }
            else
                newList.Add(radObjects[i]);
        }

        radObjects.RemoveRange(0, radObjects.Count);
        radObjects.AddRange(newList);
    }



    private void Update()
    {
        if (playerPos == null)
            return;

        foreach (RadarObject r in radObjects)
        {
            // DIRECTION VECTOR TO THE ZOMBIE FROM PLAYER...
            Vector3 radarPos = r.owner.transform.position - playerPos.position;
            // MAPSCALE TIMES THE DIST BETWEEN ZOMBIE AND PLAYER 
            float distToOjbect = Vector3.Distance(r.owner.transform.position, playerPos.position) * mapScale;

            // WHEN WE ROTATE FPS THEN THIS CODE ROATES THE RADAR AND MAKES VISIBLE THE THINGS THAT ARE INFRONT OF US FPS...
            float deltaY = Mathf.Atan2(radarPos.x, radarPos.z) * Mathf.Rad2Deg - 270 - playerPos.eulerAngles.y;
            radarPos.x = distToOjbect * Mathf.Cos(deltaY * Mathf.Deg2Rad) * -1;
            radarPos.z = distToOjbect * Mathf.Sin(deltaY * Mathf.Deg2Rad);

            // RADAR SCRIPT IS ATTACHED TO THE CANVAS SO MAKING THE OBJECT THE CHILD OF THE CANVAS...
            r.icon.transform.SetParent(this.transform);
            RectTransform rt = this.GetComponent<RectTransform>();
            // SETTING THE POSITION OF THE RADAR OBJECT WITH RESPECT TO THE CANVAS AND RADAR...
            r.icon.transform.position = new Vector3(radarPos.x + rt.pivot.x, radarPos.z + rt.pivot.y, 0) + this.transform.position;
        }
    }
}
