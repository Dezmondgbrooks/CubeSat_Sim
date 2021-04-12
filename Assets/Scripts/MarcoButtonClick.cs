using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarcoButtonClick : MonoBehaviour
{ 
    public GameObject MarcoUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //pop back up menu
        }
    }

    public void OnHide()
    {
        Debug.Log("On Hide");
    }

    public void OnStart()
    {
        MarcoUI.SetActive(false);
    }

    public void OnBack()
    {
        SceneManager.LoadScene("Nasa Home Screen");
    }

    public void OnRestart()
    {
        MarcoUI.SetActive(true);
    }


}
