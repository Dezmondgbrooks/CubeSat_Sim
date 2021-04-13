using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deploy : MonoBehaviour
{
    public GameObject CubeSat;
    public Transform Earth;
    public Transform Planet;
    // Start is called before the first frame update
    void Start()
    {
        GameObject a = Instantiate(CubeSat) as GameObject;
        
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(Earth.position, Planet.position);
        UnityEngine.Debug.Log("Distance Between: " + dist);
    }

    private float DistanceBetween(Transform item1, Transform item2)
    {
        //return Vector3.Distance(item1.position, item2.posistion);
        return 0;
    }
}
