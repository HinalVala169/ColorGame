using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwichMode : MonoBehaviour
{
    public GameObject[] scrolls;


    public void Start()
    {
       switchSelectedMode(0);
    }


    public void switchSelectedMode(int scrollNo)
    {
        foreach(GameObject go in scrolls)
        {
            go.SetActive(false);
        }
        scrolls[scrollNo].SetActive(true);
    }
}
