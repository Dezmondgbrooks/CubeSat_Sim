using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSat : MonoBehaviour
{
    public GameObject Cube;

    public double distance;
    public double LM;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LinkMarginCalc()
    {

        // Rs sensitiviy can be estimated using websites
        //double Rs = 0;  Reciever sens
        //double Gr = 0;  ant Gain
        //double Gt = 25.5;  ant gain
        //double Tp = 3; locked at 3
        //double d = 157000000000000; distance between cube sats
        //double f = 8.4;
        //double FSL = 20 * Math.Log(d) + 20 * Math.Log(f) + 92.45;
        //return Gr + Gt + Tp - Rs - FSL;
    }

    
}
