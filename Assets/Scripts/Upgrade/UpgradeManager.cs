using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("ショップ商品")]
    [SerializeField] private UpgradeData[] upgrades;

    public UpgradeData[] Upgrades => upgrades;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 商品を購入
    public bool BuyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogError("UpgradeDataがありません。");
            return false;
        }

        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        // コインが足りない
        if (data.coins < upgrade.price)
        {
            Debug.Log(
                "コインが足りません。" +
                " 必要：" + upgrade.price +
                " 所持：" + data.coins
            );

            return false;
        }

        // コインを支払う
        bool success =
            data.SpendCoins(upgrade.price);

        if (!success)
        {
            return false;
        }

        // 強化を適用
        ApplyUpgrade(upgrade, data);

        Debug.Log(
            "購入：" + upgrade.upgradeName +
            " / 残りコイン：" + data.coins
        );

        return true;
    }

    // 強化効果を適用
    private void ApplyUpgrade(
        UpgradeData upgrade,
        PlayerRunData data)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.AttackUp:

                data.AddAttack(upgrade.value);

                Debug.Log(
                    "攻撃力 +" +
                    upgrade.value
                );

                break;

            case UpgradeType.SpeedUp:

                data.AddSpeed(upgrade.value);

                Debug.Log(
                    "速度 +" +
                    upgrade.value
                );

                break;

            case UpgradeType.MaxHPUp:

                data.AddMaxHP(upgrade.value);

                Debug.Log(
                    "最大HP +" +
                    upgrade.value
                );

                break;

            case UpgradeType.Heal:

                data.Heal(upgrade.value);

                Debug.Log(
                    "HP +" +
                    upgrade.value
                );

                break;
        }
    }

    // ショップ終了
    public void CompleteUpgrade()
    {
        Debug.Log(
            "===== UPGRADE COMPLETE ====="
        );

        RoguelikeManager.Instance
            .UpgradeComplete();
    }
}