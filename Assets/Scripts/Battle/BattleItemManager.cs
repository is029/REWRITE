using UnityEngine;

public class BattleItemManager : MonoBehaviour
{
    public static BattleItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // アイテムを使用
    public bool UseItem(int index)
    {
        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        // クールダウン中
        if (!data.CanUseItem())
        {
            Debug.Log(
                "アイテムはあと " +
                data.itemCooldown +
                " ターン使用できません。"
            );

            return false;
        }

        if (index < 0 ||
            index >= data.items.Count)
        {
            Debug.LogWarning(
                "存在しないアイテムです。"
            );

            return false;
        }

        ItemData item = data.items[index];

        if (item == null)
        {
            Debug.LogWarning(
                "アイテムがありません。"
            );

            return false;
        }

        switch (item.itemType)
        {
            case ItemType.Heal:

                BattleManager.Instance.Player.Heal(
                    item.value
                );

                Debug.Log(
                    "アイテム使用：" +
                    item.itemName +
                    " / HP +" +
                    item.value
                );

                break;

            case ItemType.Damage:

                BattleManager.Instance.Enemy.TakeDamage(
                    item.value
                );

                Debug.Log(
                    "アイテム使用：" +
                    item.itemName +
                    " / Enemyに" +
                    item.value +
                    "ダメージ"
                );

                break;
        }

        // アイテムを1個消費
        data.items.RemoveAt(index);

        // 2ターンのクールダウン
        data.UseItemCooldown();

        return true;
    }
}