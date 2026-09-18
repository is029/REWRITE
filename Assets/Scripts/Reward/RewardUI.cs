using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [Header("Reward Image")]
    [SerializeField] private Image rewardImage;

    [Header("Reward Buttons")]
    [SerializeField] private Button coinButton;
    [SerializeField] private Button itemButton;

    [Header("現在の所持コイン")]
    [SerializeField] private TMP_Text currentCoinText;

    [Header("現在の所持アイテム")]
    [SerializeField] private TMP_Text currentItemText;

    private void Start()
    {
        Show();
    }

    public void Show()
    {
        if (RewardManager.Instance == null)
        {
            Debug.LogError(
                "RewardManagerが見つかりません。"
            );

            return;
        }

        if (rewardImage != null)
        {
            rewardImage.gameObject.SetActive(true);
        }

        // コイン報酬
        coinButton.gameObject.SetActive(true);
                
        // アイテム報酬
        itemButton.gameObject.SetActive(true);

        // ボタンイベント
        coinButton.onClick.RemoveAllListeners();

        coinButton.onClick.AddListener(
            SelectCoinReward
        );

        itemButton.onClick.RemoveAllListeners();

        itemButton.onClick.AddListener(
            SelectItemReward
        );

        // 現在の所持状況を表示
        UpdatePlayerStatus();
    }

    // 現在のコイン・アイテムを表示
    private void UpdatePlayerStatus()
    {
        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        // コイン
        if (currentCoinText != null)
        {
            currentCoinText.text =
                "所持コイン：" +
                data.coins;
        }

        // アイテム
        if (currentItemText != null)
        {
            if (data.items.Count == 0)
            {
                currentItemText.text =
                    "所持アイテム：なし";
            }
            else
            {
                string itemList =
                    "所持アイテム：\n";

                for (int i = 0; i < data.items.Count; i++)
                {
                    if (data.items[i] == null)
                    {
                        continue;
                    }

                    itemList +=
                        "・" +
                        data.items[i].itemName +
                        "\n";
                }

                currentItemText.text =
                    itemList;
            }
        }
    }

    // コイン報酬
    private void SelectCoinReward()
    {
        Debug.Log(
            "コイン報酬を選択"
        );

        RewardManager.Instance
            .GetBonusCoins();
    }

    // アイテム報酬
    private void SelectItemReward()
    {
        Debug.Log(
            "アイテム報酬を選択"
        );

        RewardManager.Instance
            .GetRandomItem();
    }
}