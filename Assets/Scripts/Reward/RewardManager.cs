using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("設定")]
    [SerializeField] private int bonusCoins = 50;

    [Header("アイテム")]
    [SerializeField] private ItemData[] itemPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public void GetBonusCoins()
    {
        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        data.AddCoins(bonusCoins);

        Debug.Log(
            "ボーナスコイン +" + bonusCoins +
            " / 所持コイン：" + data.coins
        );

        CompleteReward();
    }

    public void GetRandomItem()
    {
        if (itemPool == null || itemPool.Length == 0)
        {
            Debug.LogError(
                "RewardManagerにItemDataが設定されていません。"
            );
            return;
        }

        int index =
            Random.Range(0, itemPool.Length);

        ItemData item = itemPool[index];

        if (item == null)
        {
            Debug.LogError(
                "ItemPoolに空のItemDataがあります。"
            );
            return;
        }

        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        data.AddItem(item);

        Debug.Log(
            "アイテム獲得：" + item.itemName
        );

        CompleteReward();
    }

    private void CompleteReward()
    {
        RoguelikeManager.Instance.RewardComplete();
    }
}