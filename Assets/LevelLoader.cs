using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transistionTime = 1f;

    // Update is called once per frame
    public void LoadNextLevel(string scene_name)
    {
        //StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
      
        transition.SetTrigger("Start");

        //Wait
        //yield return new WaitForSeconds(transistionTime);
        //Load Screen
        SceneManager.LoadScene(scene_name);
    }

    public void OnMarco()
    {
        transition.SetTrigger("Start");
        SceneManager.LoadScene("Marco");
    }

   /* IEnumerator LoadLevel(string name)
    {
        transition.SetTrigger("Start");

        //Wait
        yield return new WaitForSeconds(transistionTime);
        //Load Screen
        SceneManager.LoadScene(levelIndex);
    }
   */ 
}
