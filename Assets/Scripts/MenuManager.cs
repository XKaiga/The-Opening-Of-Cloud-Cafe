using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnClickNewGameBtn()
    {
        SaveSystem.LoadNewGameData();
        SceneManager.LoadScene("Tutorial");
    }

    public void OnClickLoadBtn()
    {
        SceneManager.LoadScene("SaveLoadData");
    }


    public void OnClickContinueOption()
    {
        bool loaded = SaveSystem.LoadLatestSaveSlot();
        if (loaded)
            SceneManager.LoadScene("Dialogue");
        else
            Debug.LogWarning("Warning: Couldn't find Latest Save!");
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
