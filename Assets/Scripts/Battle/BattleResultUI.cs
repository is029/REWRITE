using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private TMP_Text defeatText;

    [Header("Scene")]
    [SerializeField] private string battleSceneName;
    [SerializeField] private string titleSceneName;


    private void Start()
    {
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
    }

    // 勝った時のリザルト表示
    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
        defeatPanel.SetActive(false);

        if (victoryText != null)
        {
            victoryText.text = "VICTORY";
        }
    }

    // 負けた時のリザルト表示
    public void ShowDefeat()
    {
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(true);

        if (defeatText != null)
        {
            defeatText.text = "DEFEAT";
        }
    }


    public void Retry()
    {
        if (!string.IsNullOrEmpty(battleSceneName))
        {
            SceneManager.LoadScene(battleSceneName);
        }
    }


    public void GoToTitle()
    {
        if (!string.IsNullOrEmpty(titleSceneName))
        {
            RoguelikeManager.Instance.ResetRun();
            SceneManager.LoadScene(titleSceneName);
        }
    }

    public void NextStage()
    {
        if (RoguelikeManager.Instance == null)
            return;

        switch (RoguelikeManager.Instance.CurrentStage)
        {
            case RoguelikeManager.StageType.Reward:
                Debug.Log("報酬画面へ");
                SceneManager.LoadScene("RewardScene");
                break;

            case RoguelikeManager.StageType.Upgrade:
                MapManager.Instance.ReturnToMap();
                break;

            case RoguelikeManager.StageType.Boss:
                MapManager.Instance.ReturnToMap();
                break;

            case RoguelikeManager.StageType.Clear:
                Debug.Log("クリア画面へ");
                SceneManager.LoadScene("ClearScene");
                break;
        }
    }
}