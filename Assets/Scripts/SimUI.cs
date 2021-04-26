using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SimUI : MonoBehaviour
{
    public bool isPaused = false;
    public GameObject BeginUI;
    public GameObject InputUI;
    public GameObject Simulation;
    public GameObject Deployer;
    
   

    //textbox stuff
    public string gainString;
    public string sensString;
    public string solarString;

    public GameObject gainInputField;
    public GameObject sensInputField;

    public Text AvgLM;
    public Text LowLM;
    public Text AvgPower;
    public Text LowPower;
    public Text Cost;
    public Text Num;


    // Start is called before the first frame update
    void Start()
    {
        Pause();
        InputUI.SetActive(false);
        Simulation.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Resume()
    {
        Time.timeScale = 1.0f;
        isPaused = false;
    }

    void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }
    // move to input ui
    public void OnBegin()
    {
        InputUI.SetActive(true);
        BeginUI.SetActive(false);
    }
    // send back to home screen
    public void OnBack()
    {
        Resume();
        SceneManager.LoadScene("Nasa Home Screen");
    }

    public void OnStart()
    {
        StoreInputs();
        Deployer.GetComponent<Deploy>().Populate();
        Resume();
        UnityEngine.Debug.Log("Gain: " + gainString);
        UnityEngine.Debug.Log("Sens: " + sensString);
        UnityEngine.Debug.Log("Solar: " + solarString);
        InputUI.SetActive(false);
        Simulation.SetActive(true);
        LinkMargin();
        Power();
        TotalCost();
    }

    public void OnUIBack()
    {
        InputUI.SetActive(false);
        BeginUI.SetActive(true);
    }

    public void StoreInputs()
    {
        gainString = gainInputField.GetComponent<Text>().text;
        sensString = sensInputField.GetComponent<Text>().text;
    }

    public void OnSimBack()
    {
        SceneManager.LoadScene("Mars");
    }

    public void PauseSim()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
            LinkMargin();
            Power();
            TotalCost();
        }
    }

    public void LinkMargin()
    {
        double Avg = Deployer.GetComponent<Deploy>().avgLinkMargin();
        double low = Deployer.GetComponent<Deploy>().LowestLink;

        AvgLM.text = Avg.ToString();
        LowLM.text = low.ToString();
    }

    public void Power()
    {
        double Avg = Deployer.GetComponent<Deploy>().avgPower();
        double low = Deployer.GetComponent<Deploy>().LowPower;

        AvgPower.text = Avg.ToString();
        LowPower.text = low.ToString();
    }

    public void TotalCost()
    {
        int numCube = Deployer.GetComponent<Deploy>().CubeList.Count;
        double cost = numCube * 0.252 + 10;
        Cost.text = cost.ToString();
        Num.text = numCube.ToString();
    }
}
