using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Buttons : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        GameController.ballCount = 0;
    }

    public void Speedx1()
    {
        Time.timeScale = 1f;
    }
    public void Speedx2()
    {
        Time.timeScale = 2f;
    }

    public void Speedx4()
    {
        Time.timeScale = 4f;
    }


}
