using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour
{
    enum Is
    {
        Ragdoll, Prefab
    }
    [SerializeField] Is characterIs;
    [SerializeField] float maxSinkHeight = -2f;
    float sinkDelay = 5f;

    bool isFirstInvoke = true;


    private void Start()
    {
        sinkDelay = Random.Range(5f, 11f);

        if (characterIs == Is.Ragdoll)
            InvokeRepeating("SinkSelf", sinkDelay, 0.025f);
    }


    public void StartSink()
    {
        InvokeRepeating("SinkSelf", sinkDelay, 0.025f);
    }

    public void SinkSelf()
    {
        if (transform.position.y <= maxSinkHeight)
        {
            Destroy(gameObject);
            return;
        }

        if (isFirstInvoke)
        {
            isFirstInvoke = false;

            Collider xt = GetComponent<Collider>();
            if (xt != null)
                Destroy(xt);

            Collider[] cols = gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider x in cols)
                Destroy(x);

            Rigidbody[] rigids = gameObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody x in rigids)
            {
                x.useGravity = false;
            }
        }

        transform.Translate(new Vector3(0, -0.01f, 0));
    }
}
