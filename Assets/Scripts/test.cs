using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class test : MonoBehaviour {
    // Use this for initialization
    void Start () {

        Game game = FindObjectOfType<Game>();
        game.triggerEvent("EventStart");

    }

    // Update is called once per frame
    void Update()
    {

    }
}

