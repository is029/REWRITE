using UnityEngine;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance { get; private set; }

    [Header("行動数")]
    [SerializeField] private int baseActionsPerTurn = 3;

    [SerializeField] private int currentActionsPerTurn = 3;

    public int CurrentActionsPerTurn
    {
        get { return currentActionsPerTurn; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentActionsPerTurn =
            baseActionsPerTurn;
    }

    /// <summary>
    /// 行動数を変更する
    /// </summary>
    public void SetActionsPerTurn(int amount)
    {
        currentActionsPerTurn =
            Mathf.Max(1, amount);

        Debug.Log(
            "===== RULE REWRITE =====\n" +
            "1ターンの行動数：" +
            currentActionsPerTurn
        );
    }

    /// <summary>
    /// 行動数を+1する
    /// </summary>
    public void AddActionPerTurn(int amount)
    {
        currentActionsPerTurn += amount;

        currentActionsPerTurn =
            Mathf.Max(1, currentActionsPerTurn);

        Debug.Log(
            "===== RULE REWRITE =====\n" +
            "1ターンの行動数：" +
            currentActionsPerTurn
        );
    }

    /// <summary>
    /// 初期状態に戻す
    /// </summary>
    public void ResetRules()
    {
        currentActionsPerTurn =
            baseActionsPerTurn;

        Debug.Log(
            "ルールを初期状態に戻しました。"
        );
    }
}