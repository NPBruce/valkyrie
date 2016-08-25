using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// This file is used for test code that doesn't have a proper home yet
public class test : MonoBehaviour {
    // Use this for initialization
    void Start () {

        Game game = FindObjectOfType<Game>();
        EventHelper.triggerEvent("EventStart");

    }

    // Update is called once per frame
    void Update()
    {

    }
}

