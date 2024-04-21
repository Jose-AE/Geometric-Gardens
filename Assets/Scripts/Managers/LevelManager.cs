using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{


    private static LevelManager instance;
    public static Camera mainCamera { private set; get; }


    void Awake()
    {
        //Make sure there is only a single instance of input manager class
        if (instance != null && instance != this)
        {
            Debug.LogWarning("An extra instance of LevelManager exists in '" + gameObject.name + "' gameObject and will be removed", gameObject);
            Destroy(this);
        }
        else
            instance = this;



        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }






}
