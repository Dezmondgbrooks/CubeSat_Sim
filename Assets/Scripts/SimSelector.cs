using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimSelector : MonoBehaviour
{
    public GameObject SimSelectUI;
    public GameObject MenuUI;
    // Start is called before the first frame update
    void Start()
    {
        SimSelectUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onBack()
    {
        SimSelectUI.SetActive(false);
        MenuUI.SetActive(true);
    }

    public void OnMars()
    {
        // change scene
        SceneManager.LoadScene("Mars");
    }

    public void OnVenus()
    {
        //change scene
        SceneManager.LoadScene("Venus");
    }

    public void OnMercury()
    {
        //change scene
        SceneManager.LoadScene("Mercury");
    }

    public void OnStart()
    {
        MenuUI.SetActive(false);
        SimSelectUI.SetActive(true);
    }
}
