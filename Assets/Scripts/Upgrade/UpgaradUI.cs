using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("コイン表示")]
    [SerializeField] private TMP_Text coinText;

    [Header("商品")]
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TMP_Text[] upgradeNameTexts;
    [SerializeField] private TMP_Text[] upgradeDescriptionTexts;
    [SerializeField] private TMP_Text[] upgradePriceTexts;

    [Header("購入完了")]
    [SerializeField] private Button completeButton;

    private void Start()
    {
        Show();

        if (completeButton != null)
        {
            completeButton.onClick.RemoveAllListeners();

            completeButton.onClick.AddListener(
                CompleteUpgrade
            );
        }
    }

    public void Show()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogError(
                "UpgradeManagerが見つかりません。"
            );

            return;
        }

        UpgradeData[] upgrades =
            UpgradeManager.Instance.Upgrades;

        UpdateCoinText();

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < upgrades.Length &&
                upgrades[i] != null)
            {
                int index = i;

                upgradeButtons[i]
                    .gameObject
                    .SetActive(true);

                upgradeNameTexts[i].text =
                    upgrades[i].upgradeName;

                upgradeDescriptionTexts[i].text =
                    upgrades[i].description;

                upgradePriceTexts[i].text =
                    upgrades[i].price +
                    " コイン";

                upgradeButtons[i]
                    .onClick
                    .RemoveAllListeners();

                upgradeButtons[i]
                    .onClick
                    .AddListener(
                        () => BuyUpgrade(index)
                    );
            }
            else
            {
                upgradeButtons[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }

    private void BuyUpgrade(int index)
    {
        UpgradeData[] upgrades =
            UpgradeManager.Instance.Upgrades;

        if (index < 0 ||
            index >= upgrades.Length)
        {
            return;
        }

        bool success =
            UpgradeManager.Instance
                .BuyUpgrade(upgrades[index]);

        if (success)
        {
            Debug.Log(
                "強化を購入：" +
                upgrades[index].upgradeName
            );

            UpdateCoinText();
        }
    }

    private void UpdateCoinText()
    {
        if (coinText == null)
        {
            return;
        }

        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        coinText.text =
            "所持コイン：" +
            data.coins;
    }

    private void CompleteUpgrade()
    {
        UpgradeManager.Instance
            .CompleteUpgrade();
    }
}