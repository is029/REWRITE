using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    private List<BattleAction> playerActions =
        new List<BattleAction>();

    [Header("アニメーション・行動間隔")]
    [SerializeField] private float actionInterval = 0.8f;

    private int playerSpeedModifier = 0;
    private int enemySpeedModifier = 0;

    public event Action OnActionsReset;

    public int CurrentTurn { get; private set; } = 1;

    public int CurrentActionIndex
    {
        get { return playerActions.Count; }
    }

    public bool IsExecuting { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public int GetActionsPerTurn()
    {
        if (RuleManager.Instance == null)
        {
            return 3;
        }

        return RuleManager.Instance.CurrentPlayerActionsPerTurn;
    }

    public int GetEnemyActionsPerTurn()
    {
        if (RuleManager.Instance == null)
        {
            return 3;
        }

        return RuleManager.Instance.CurrentEnemyActionsPerTurn;
    }

    // ========================================
    // プレイヤーの通常行動を予約
    // ========================================

    public bool AddPlayerAction(ActionType actionType)
    {
        if (IsExecuting)
        {
            return false;
        }

        int actionPerTurn = GetActionsPerTurn();
        if (playerActions.Count >= actionPerTurn)
        {
            Debug.Log("このターンの行動は3つまでです。");
            return false;
        }

        BattleAction action =
            new BattleAction(
                actionType,
                true,
                BattleManager.Instance.Player
            );

        playerActions.Add(action);

        Debug.Log(
            "プレイヤー行動予約：" +
            actionType +
            " / SPEED " +
            action.speed +
            " (" +
            playerActions.Count +
            "/3)"
        );

        RefreshEnemyFutureUI();

        return true;
    }


    // ========================================
    // プレイヤーのスキルを予約
    // ========================================

    public bool AddPlayerSkill(SkillData skill)
    {
        if (IsExecuting)
        {
            return false;
        }

        int actionPerTurn = GetActionsPerTurn();

        if (playerActions.Count >= actionPerTurn)
        {
            Debug.Log("このターンの行動は3つまでです。");
            return false;
        }

        if (skill == null)
        {
            Debug.LogError("SkillDataが設定されていません。");
            return false;
        }

        // このターンですでに同じスキルを予約しているか確認
        foreach (BattleAction paction in playerActions)
        {
            if (paction.actionType == ActionType.Skill &&
                paction.skillData == skill)
            {
                Debug.Log(
                    "スキル「" + skill.skillName +
                    "」はこのターンにすでに使用予約されています。"
                );

                return false;
            }
        }

        // ========================================
        // 1ターンにスキルは1回まで
        // ========================================
        foreach (BattleAction paction in playerActions)
        {
            if (paction.actionType == ActionType.Skill)
            {
                Debug.Log(
                    "このターンはすでにスキルを使用予約しています。"
                );

                return false;
            }
        }

        // クールタイム中なら使用不可
        if (!BattleManager.Instance.Player.IsSkillAvailable(skill))
        {
            int cooldown =
                BattleManager.Instance.Player.GetSkillCooldown(skill);

            Debug.Log(
                "スキル「" +
                skill.skillName +
                "」はクールタイム中です。残り：" +
                cooldown +
                "ターン"
            );

            return false;
        }

        BattleAction action =
            new BattleAction(
                ActionType.Skill,
                true,
                BattleManager.Instance.Player,
                skill
            );

        playerActions.Add(action);

        Debug.Log(
            "プレイヤースキル予約：" +
            skill.skillName +
            " / SPEED " +
            action.speed +
            " (" +
            playerActions.Count +
            "/3)"
        );

        RefreshEnemyFutureUI();

        return true;
    }


    // ========================================
    // 敵未来UI更新
    // ========================================
    private void RefreshEnemyFutureUI()
    {
        if (BattleManager.Instance == null)
        {
            return;
        }

        if (BattleManager.Instance.EnemyFutureUI == null)
        {
            return;
        }

        BattleManager.Instance.EnemyFutureUI
            .ShowEnemyFuture(
                playerActions
            );
    }

    // ========================================
    // プレイヤー行動取得
    // ========================================

    public List<BattleAction> GetPlayerActions()
    {
        return playerActions;
    }

    public void ClearPlayerActions()
    {
        if (IsExecuting)
        {
            return;
        }

        playerActions.Clear();

        Debug.Log("プレイヤーの予約行動をすべてクリアしました。");

        OnActionsReset?.Invoke();
    }

    // ========================================
    // ターン開始
    // ========================================
    public void StartTurn()
    {
        // プレイヤーの予約行動をリセット
        playerActions.Clear();

        IsExecuting = false;

        Debug.Log(
            "===== TURN " +
            CurrentTurn +
            " ====="
        );

        // UIにも「リセットした」と通知
        OnActionsReset?.Invoke();
    }

    // ========================================
    // ターン実行開始
    // ========================================

    public void ExecuteTurn()
    {
        if (IsExecuting)
        {
            return;
        }

        int actionPerTurn = GetActionsPerTurn();

        if (playerActions.Count != actionPerTurn)
        {
            Debug.Log(
                "3つの行動を選択してください。"
            );

            return;
        }

        StartCoroutine(ExecuteActions());
    }


    // ========================================
    // ① → ② → ③を実行
    // ========================================
    private IEnumerator ExecuteActions()
    {
        IsExecuting = true;

        List<BattleAction> enemyActions =
            BattleManager.Instance.EnemyAI.GetEnemyActions();

        int playerActionPerTurn = GetActionsPerTurn();

        if (enemyActions == null)
        {
            Debug.LogError("敵の行動が作成されていません。");
            IsExecuting = false;
            yield break;
        }

        for (int i = 0; i < playerActionPerTurn; i++)
        {
            // 戦闘終了チェック
            if (BattleManager.Instance.Player.IsDead ||
                BattleManager.Instance.Enemy.IsDead)
            {
                yield break;
            }

            // プレイヤーの行動数チェック
            if (i >= playerActions.Count)
            {
                Debug.LogError(
                    "プレイヤーの行動数が不足しています。" +
                    " 必要：" + playerActionPerTurn +
                    " / 実際：" + playerActions.Count
                );

                IsExecuting = false;
                yield break;
            }

            BattleAction playerAction = playerActions[i];

            // 敵の行動
            BattleAction enemyAction = null;

            if (i < enemyActions.Count)
            {
                enemyAction = enemyActions[i];
            }

            Debug.Log(
                "========== ACTION " +
                (i + 1) +
                " =========="
            );

            // ========================================
            // 敵の行動がない場合
            // ========================================

            if (enemyAction == null)
            {
                Debug.Log(
                    "Player : " +
                    playerAction.actionType +
                    " / SPEED " +
                    playerAction.speed
                );

                Debug.Log("Enemy : ---");

                // プレイヤー行動
                yield return ExecutePlayerAction(
                    playerAction,
                    false
                );

                // 行動間隔
                yield return new WaitForSeconds(actionInterval);

                if (BattleManager.Instance.Player.IsDead ||
                    BattleManager.Instance.Enemy.IsDead)
                {
                    yield break;
                }

                continue;
            }

            // ========================================
            // 通常の1対1行動
            // ========================================

            Debug.Log(
                "Player : " +
                playerAction.actionType +
                " / SPEED " +
                playerAction.speed
            );

            Debug.Log(
                "Enemy : " +
                enemyAction.actionType +
                " / SPEED " +
                enemyAction.speed
            );

            bool enemyDefending =
                enemyAction.actionType == ActionType.Defend;

            bool playerDefending =
                playerAction.actionType == ActionType.Defend;

            // ========================================
            // プレイヤーの方が速い
            // ========================================

            if (playerAction.speed > enemyAction.speed)
            {
                yield return ExecutePlayerAction(
                    playerAction,
                    enemyDefending
                );

                yield return new WaitForSeconds(actionInterval);

                if (BattleManager.Instance.Player.IsDead ||
                    BattleManager.Instance.Enemy.IsDead)
                {
                    yield break;
                }

                // 敵行動
                yield return ExecuteEnemyActionWithDelay(
                    enemyAction,
                    playerDefending
                );
            }

            // ========================================
            // 同速ならプレイヤー先攻
            // ========================================

            else if (playerAction.speed == enemyAction.speed)
            {
                yield return ExecutePlayerAction(
                    playerAction,
                    enemyDefending
                );

                yield return new WaitForSeconds(actionInterval);

                if (BattleManager.Instance.Player.IsDead ||
                    BattleManager.Instance.Enemy.IsDead)
                {
                    yield break;
                }

                // 敵行動
                yield return ExecuteEnemyActionWithDelay(
                    enemyAction,
                    playerDefending
                );
            }

            // ========================================
            // 敵の方が速い
            // ========================================

            else
            {
                yield return ExecuteEnemyActionWithDelay(
                    enemyAction,
                    playerDefending
                );

                yield return new WaitForSeconds(actionInterval);

                if (BattleManager.Instance.Player.IsDead ||
                    BattleManager.Instance.Enemy.IsDead)
                {
                    yield break;
                }

                // プレイヤー行動
                yield return ExecutePlayerAction(
                    playerAction,
                    enemyDefending
                );
            }

            // 次の行動まで待つ
            yield return new WaitForSeconds(actionInterval);
        }

        EndTurn();
    }

    // ========================================
    // Playerの行動
    // ========================================
    private IEnumerator ExecutePlayerAction(
        BattleAction action,
        bool enemyDefending)
    {
        switch (action.actionType)
        {
            case ActionType.Attack:

                Debug.Log(
                    "Player：攻撃！ SPEED " +
                    action.speed
                );

                BattleManager.Instance.PlayerAttack(
                    enemyDefending
                );

                break;


            case ActionType.Defend:

                Debug.Log(
                    "Player：防御！ SPEED " +
                    action.speed
                );

                BattleManager.Instance.Player.PlayDefendAnimation();

                BattleManager.Instance.Player.Defend();

                break;


            case ActionType.Skill:

                if (action.skillData == null)
                {
                    Debug.LogWarning(
                        "スキルデータがありません。"
                    );

                    break;
                }

                Debug.Log(
                    "Player：" +
                    action.skillData.skillName +
                    "！ SPEED " +
                    action.speed
                );

                // スキル専用アニメーション
                BattleManager.Instance.Player.PlaySkillAnimation(
                    action.skillData
                );

                BattleManager.Instance.ExecutePlayerSkill(
                    action.skillData,
                    enemyDefending
                );

                // スキル使用後にクールタイム開始
                BattleManager.Instance.Player.StartSkillCooldown(
                    action.skillData
                );

                break;
        }

        yield return null;
    }

    private IEnumerator ExecuteEnemyActionWithDelay(
    BattleAction action,
    bool playerDefending)
    {
        BattleManager.Instance.ExecuteEnemyAction(
            action,
            playerDefending
        );

        yield return null;
    }

    // ========================================
    // Enemy / Player Speed変更
    // ========================================

    public void ChangeEnemySpeed(int amount)
    {
        enemySpeedModifier += amount;

        Debug.Log(
            "Enemy Speed補正：" +
            enemySpeedModifier
        );
    }


    public void ChangePlayerSpeed(int amount)
    {
        playerSpeedModifier += amount;

        Debug.Log(
            "Player Speed補正：" +
            playerSpeedModifier
        );
    }


    // ========================================
    // ターン終了
    // ========================================
    private void EndTurn()
    {
        Debug.Log(
            "===== TURN " +
            CurrentTurn +
            " END ====="
        );

        // 状態異常・Buff/Debuff処理
        BattleManager.Instance.Player.EndTurnEffects();
        BattleManager.Instance.Enemy.EndTurnEffects();

        // アイテムクールタイム減少
        RoguelikeManager.Instance.PlayerData.ReduceItemCooldown();

        // 次のターンへ
        CurrentTurn++;

        IsExecuting = false;

        Debug.Log(
            "===== NEXT TURN : " +
            CurrentTurn +
            " ====="
        );

        // 自動的に次ターン開始
        BattleManager.Instance.StartTurn();
    }

    public bool ReplacePlayerAction(
    int index,
    ActionType actionType)
    {
        if (IsExecuting)
        {
            return false;
        }

        if (index < 0 ||
            index >= playerActions.Count)
        {
            return false;
        }

        BattleAction action =
            new BattleAction(
                actionType,
                true,
                BattleManager.Instance.Player
            );

        playerActions[index] = action;

        Debug.Log(
            "行動" +
            (index + 1) +
            "を " +
            actionType +
            " に変更しました。"
        );

        RefreshEnemyFutureUI();

        return true;
    }

    public bool ReplacePlayerSkill(
    int index,
    SkillData skill)
    {
        if (IsExecuting)
        {
            return false;
        }

        if (index < 0 ||
            index >= playerActions.Count)
        {
            return false;
        }

        if (skill == null)
        {
            return false;
        }

        BattleAction action =
            new BattleAction(
                ActionType.Skill,
                true,
                BattleManager.Instance.Player,
                skill
            );

        playerActions[index] = action;

        Debug.Log(
            "行動" +
            (index + 1) +
            "をスキル「" +
            skill.skillName +
            "」に変更しました。"
        );

        RefreshEnemyFutureUI();

        return true;
    }

    public void StopBattle()
    {
        StopAllCoroutines();

        IsExecuting = false;

        Debug.Log(
            "TurnManager：バトル停止"
        );
    }
}