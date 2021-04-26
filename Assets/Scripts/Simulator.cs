using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Simulator : MonoBehaviour
{
    public GameObject SimRanUI;
    public Text Link;

    void Start() { SimRanUI.SetActive(false); }

    // Update is called once per frame
    void Update()
    {
        
    }

    public double LinkMarginCalc()
    {
        // Rs sensitiviy can be estimated using websites
        double Rs = 156;
        double Gr = 22.5;
        double Gt = 25.5;
        double Tp = 10.0;
        double d = 157.0 * Math.Pow(10,6);
        double f = 8.46;
        double FSL = 20 * Math.Log(d,10.0) + 20 * Math.Log(f, 10.0) + 92.45 -64.33;
        return Gr + Gt + Tp + Rs - FSL;
    } 

    public void onBack()
    {
        SimRanUI.SetActive(false);
    }

    public void OnStart()
    {
        SimRanUI.SetActive(true);
        double LM = LinkMarginCalc();
        string specifier = "G";
        string linkMargin = LM.ToString();
       

        Link.text = linkMargin;
    }
}
