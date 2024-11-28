using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnClickStartOption()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void OnClickContinueOption()
    {
        SaveSystem.LoadLatestSaveSlot();
    }

    public void OnClickOptionsOption()
    {
        SceneManager.LoadScene("Options");
    }

    public void OnClickExitOption()
    {
        Application.Quit();

    }
}
