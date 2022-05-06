using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarObject
{
    public GameObject owner { get; set; }
    public Image icon { get; set; }

}

public class Radar : MonoBehaviour
{
    [SerializeField] Transform playerPos;
    public float mapScale = 2.0f;

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
            Vector3 radarPos = r.owner.transform.position - playerPos.position;
            float distToOjbect = Vector3.Distance(r.owner.transform.position, playerPos.position) * mapScale;

            float deltaY = Mathf.Atan2(radarPos.x, radarPos.z) * Mathf.Rad2Deg - 270 - playerPos.eulerAngles.y;
            radarPos.x = distToOjbect * Mathf.Cos(deltaY * Mathf.Deg2Rad) * -1;
            radarPos.z = distToOjbect * Mathf.Sin(deltaY * Mathf.Deg2Rad);


            r.icon.transform.SetParent(this.transform);
            RectTransform rt = this.GetComponent<RectTransform>();
            r.icon.transform.position = new Vector3(radarPos.x + rt.pivot.x, radarPos.z + rt.pivot.y, 0) + this.transform.position;
        }
    }
}
