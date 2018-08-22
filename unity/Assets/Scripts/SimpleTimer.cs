using UnityEngine;
using System.Collections;
using System;

public class SimpleTimer : MonoBehaviour
{
    public float targetTime = 0.0f;
    private Action triggerEnd;

    public void Init(float time, Action f)
    {
        targetTime = time;
        triggerEnd = f;
    }

    void Update()
    {
        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f)
        {
            if (triggerEnd != null)
                triggerEnd();
        }
    }
}

