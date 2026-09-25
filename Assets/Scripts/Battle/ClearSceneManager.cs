using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearSceneManager : MonoBehaviour
{
    public void GoTitle()
    {
        RoguelikeManager.Instance.ResetRun();
        SceneManager.LoadScene("TitleScene");
    }
}
