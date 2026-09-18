using UnityEngine;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance { get; private set; }

    [Header("行動数")]
    [SerializeField] private int baseActionsPerTurn = 3;

    [SerializeField] private int currentPlayerActionsPerTurn = 3;
    [SerializeField] private int currentEnemyActionsPerTurn = 3;

    public int CurrentPlayerActionsPerTurn
    {
        get { return currentPlayerActionsPerTurn; }
    }

    public int CurrentEnemyActionsPerTurn
    {
        get { return currentEnemyActionsPerTurn; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentPlayerActionsPerTurn =
            baseActionsPerTurn;

        currentEnemyActionsPerTurn =
            baseActionsPerTurn;
    }

    /// <summary>
    /// プレイヤーの行動数を変更する
    /// </summary>
    public void SetPlayerActionsPerTurn(int amount)
    {
        currentPlayerActionsPerTurn =
            Mathf.Max(1, amount);

        Debug.Log(
            "===== PLAYER RULE REWRITE =====\n" +
            "プレイヤーの1ターン行動数：" +
            currentPlayerActionsPerTurn
        );
    }

    /// <summary>
    /// プレイヤーの行動数を+する
    /// </summary>
    public void AddPlayerActionPerTurn(int amount)
    {
        currentPlayerActionsPerTurn += amount;

        currentPlayerActionsPerTurn =
            Mathf.Max(1, currentPlayerActionsPerTurn);

        Debug.Log(
            "===== PLAYER RULE REWRITE =====\n" +
            "プレイヤーの1ターン行動数：" +
            currentPlayerActionsPerTurn
        );
    }

    /// <summary>
    /// 敵の行動数を変更する
    /// </summary>
    public void SetEnemyActionsPerTurn(int amount)
    {
        currentEnemyActionsPerTurn =
            Mathf.Max(1, amount);

        Debug.Log(
            "===== ENEMY RULE REWRITE =====\n" +
            "敵の1ターン行動数：" +
            currentEnemyActionsPerTurn
        );
    }

    /// <summary>
    /// ルールを初期状態に戻す
    /// </summary>
    public void ResetRules()
    {
        currentPlayerActionsPerTurn =
            baseActionsPerTurn;

        currentEnemyActionsPerTurn =
            baseActionsPerTurn;

        Debug.Log(
            "ルールを初期状態に戻しました。"
        );
    }
}