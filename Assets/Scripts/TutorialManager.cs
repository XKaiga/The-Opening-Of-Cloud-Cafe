using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public void OnClickYesOption() {
        GameManager.startDayNum = 0;
        this.gameObject.SetActive(false);
        SceneManager.LoadScene("Dialogue");
    }
    public void OnClickNoOption()
    {
        GameManager.startDayNum = 3;
        this.gameObject.SetActive(false);
        SceneManager.LoadScene("Dialogue");
    }
}
