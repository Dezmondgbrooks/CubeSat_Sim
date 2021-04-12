using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Simulator : MonoBehaviour
{
    public GameObject SimRanUI;
    public Text Antenna;
    public Text Link;

    void Start() { SimRanUI.SetActive(false); }

    // Update is called once per frame
    void Update()
    {
        
    }

    public double AntennaGain()
    {
        return 0;
    }

    public double LinkMarginCalc()
    {
        // Rs sensitiviy can be estimated using websites
        double Rs = 0;
        double Gr = 0;
        double Gt = 25.5;
        double Tp = 0;
        double d = 157000000000000;
        double f = 0;
        double FSL = 20 * Math.Log(d) + 20 * Math.Log(f) + 92.45;
        return Gr + Gt + Tp - Rs - FSL;
    } 

    public void onBack()
    {
        SimRanUI.SetActive(false);
    }

    public void OnStart()
    {
        SimRanUI.SetActive(true);
        double LM = LinkMarginCalc();
        double Ant = AntennaGain();
        string specifier = "G";

        string linkMargin = LM.ToString();
        string antennaGain = Ant.ToString();

        Antenna.text = antennaGain;
        Link.text = linkMargin;
    }
}
