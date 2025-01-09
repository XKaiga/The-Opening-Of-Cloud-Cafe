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
        GameManager.startDayNum = 2;
        this.gameObject.SetActive(false);
        SceneManager.LoadScene("Dialogue");
    }
}
