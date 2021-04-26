using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceGraphicsToolkit;
using System;

public class Deploy2 : MonoBehaviour
{
    public GameObject CubeSat;
    public GameObject Earth;
    public GameObject Planet;
    public GameObject Sun;
    public GameObject UI;

    static int Uunit = 8276;

    public double G;
    public double Rs;

    public List<GameObject> CubeList;
    int count = 0;
    public double Range = 0.0;
    public int CubeCount = 0;
    bool made = false;

    private float nextActionTime = 0.0f;
    public float period = 0.1f;

    public double LowestLink;
    public double LowPower;

    // Start is called before the first frame update
    void Start()
    {
        //double dist = Vector3.Distance(Earth.position, Planet.position);
        //UnityEngine.Debug.Log("Distance E M: " + dist);
    }

    // Update is called once per frame
    void Update()
    {

        if (made && !UI.GetComponent<SimUI>().isPaused && Time.time > nextActionTime)
        {
            nextActionTime += period;

            double dist = DistanceBetween(CubeList[count], CubeList[count + 1]) * Uunit;
            //UnityEngine.Debug.Log("dist: " + dist);
            //UnityEngine.Debug.Log("range: " + Range);
            //UnityEngine.Debug.Log("count: " + count);
            // new cubesat
            if (dist > Range)
            {
                NewSat(count, count + 1);
            }
            //}

            //}
            count += 1;
            if (count > CubeList.Count - 2) { count = 0; }
        }

    }

    private double DistanceBetween(GameObject item1, GameObject item2)
    {
        return Vector3.Distance(item1.GetComponent<Transform>().position, item2.GetComponent<Transform>().position);
    }

    private void NewSat(int cube1, int cube2)
    {
        //UnityEngine.Debug.Log("cube1: " + cube1);
        //UnityEngine.Debug.Log("cube2: " + cube2);
        GameObject temp = Instantiate(CubeSat, Sun.transform) as GameObject;
        // get radius between the 2
        temp.GetComponent<SgtSimpleOrbit>().Radius = (CubeList[cube1].GetComponent<SgtSimpleOrbit>().Radius + CubeList[cube2].GetComponent<SgtSimpleOrbit>().Radius) / 2.0f;
        // along with angle
        temp.GetComponent<SgtSimpleOrbit>().Angle = (CubeList[cube1].GetComponent<SgtSimpleOrbit>().Angle + CubeList[cube2].GetComponent<SgtSimpleOrbit>().Angle) / 2.0f;
        // set degrees per second
        temp.GetComponent<SgtSimpleOrbit>().DegreesPerSecond = (CubeList[cube1].GetComponent<SgtSimpleOrbit>().DegreesPerSecond + CubeList[cube2].GetComponent<SgtSimpleOrbit>().DegreesPerSecond) / 2.0f;
        // insert into list
        CubeList.Insert(cube2, temp);
    }

    public double LinkMarginCalc(int cube1, int cube2)
    {
        //rs sensitiviy can be estimated using websites
        double rs = Rs; //reciever sens
        double gr = G; //ant gain
        double gt = G; //ant gain
        double tp = 3.0; //locked at 3
        double d = DistanceBetween(CubeList[cube1], CubeList[cube2]) * Uunit; //distance between cube sats
        double f = 8.4;
        double fsl = 20 * Math.Log(d) + 20 * Math.Log(f) + 92.45 - 50.51;
        //UnityEngine.Debug.Log("Distance between CubeSats: " + DistanceBetween(CubeList[cube1], CubeList[cube2]));
        UnityEngine.Debug.Log("Distance: " + d);
        UnityEngine.Debug.Log("Rs: " + rs);
        UnityEngine.Debug.Log("G: " + gr);

        return Rs + 2 * G - 20 * Math.Log(d, 10.0) - 57.51;   //gr + gt + tp + rs - fsl;
    }
    // given the inputs find distance between cubsates
    public double CubeRange()
    {
        string gain = UI.GetComponent<SimUI>().gainString;
        string sens = UI.GetComponent<SimUI>().sensString;
        G = double.Parse(gain);
        Rs = double.Parse(sens);
        double pow = 2 * G - 3.5 + Rs + 3 - 20 * Math.Log(8.4, 10.0) - 92.45 + 50.51; //+ 50.51;
        //UnityEngine.Debug.Log("Gain: " + G);
        //UnityEngine.Debug.Log("Sens: " + Rs);
        //UnityEngine.Debug.Log("pow: " + pow);
        return (Math.Pow(10, pow / 20));
    }

    public long NumNeeded(double distance)
    {
        // in km
        double total = DistanceBetween(Earth, Planet) * Uunit;//Vector3.Distance(Earth.position, Planet.position) * Uunit;
        //UnityEngine.Debug.Log("Distance: " + distance);
        //UnityEngine.Debug.Log("Distance total: " + total);
        return Convert.ToInt64(total / distance) * 2;
    }

    public void Populate()
    {
        //unit squares
        Range = CubeRange();
        long numCube = NumNeeded(Range);
        // distance in radius from sun
        double spacing = (double)((Earth.GetComponent<SgtSimpleOrbit>().Radius - Planet.GetComponent<SgtSimpleOrbit>().Radius) / numCube);//5.0 / numCube;

        //float shit = (float)spacing;
        float prev = 10.0f;
        //UnityEngine.Debug.Log("NumCube: " + numCube);
        //UnityEngine.Debug.Log("Spacing: " + shit);
        GameObject temp;

        // degrees per second
        double speed = (double)((Earth.GetComponent<SgtSimpleOrbit>().Radius - Planet.GetComponent<SgtSimpleOrbit>().Radius) / numCube);// numCube;

        for (int i = 1; i <= numCube; i++)
        {
            // create cubesat and make parent sun
            temp = Instantiate(CubeSat, Sun.transform) as GameObject;

            //temp.transform.parent = Sun.transform;
            CubeList.Add(temp);

            CubeList[i - 1].GetComponent<SgtSimpleOrbit>().Radius = 10.0f + (float)spacing * i;
            CubeList[i - 1].GetComponent<SgtSimpleOrbit>().Angle = 0;
            prev = prev - (float)speed;
            CubeList[i - 1].GetComponent<SgtSimpleOrbit>().DegreesPerSecond = prev;
        }
        made = true;
    }

    public double avgLinkMargin()
    {
        List<double> Links = new List<double>();
        double preDivide = 0.0;
        double temp = 0.0;
        for (int i = 0; i < CubeList.Count - 1; i++)
        {
            temp = LinkMarginCalc(i, i + 1);
            Links.Add(temp);
            preDivide += temp;
        }

        Links.Sort();
        UnityEngine.Debug.Log("Size: " + Links.Count);
        LowestLink = Links[1];

        return preDivide / CubeList.Count;
    }

    public double avgPower()
    {
        List<double> powerList = new List<double>();
        double preDivide = 0.0;
        double temp = 0.0;
        for (int i = 0; i < CubeList.Count - 1; i++)
        {
            temp = calcPower(i);
            powerList.Add(temp);
            preDivide += temp;
        }
        powerList.Sort();
        LowPower = powerList[0];
        return preDivide / CubeList.Count;
    }

    public double calcPower(int cube)
    {
        double dist = DistanceBetween(Sun, CubeList[cube]);
        return (Math.Pow(695000000.0, 2) / Math.Pow(dist, 2)) * 64000000.0;
    }
}
