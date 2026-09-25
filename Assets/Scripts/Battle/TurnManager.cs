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

    [Tooltip("攻撃・スキル・回復アニメーション開始から効果処理まで")]
    [SerializeField] private float damageDelay = 1.0f;

    [Tooltip("効果処理・ヒットアニメーションから次の行動まで")]
    [SerializeField] private float hitDelay = 2.0f;

    [Tooltip("アニメーションを使わない行動の基本間隔")]
    [SerializeField] private float actionInterval = 0.8f;

    private int playerSpeedModifier = 0;
    private int enemySpeedModifier = 0;

    public event Action OnActionsReset;

    public int CurrentTurn { get; private set; } = 1;

    public int CurrentActionIndex
    {
        get
        {
            return playerActions.Count;
        }
    }

    public bool IsExecuting { get; private set; }


    // ========================================
    // 初期化
    // ========================================

    private void Awake()
    {
        Instance = this;
    }


    // ========================================
    // 行動数
    // ========================================

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
    // プレイヤー通常行動予約
    // ========================================

    public bool AddPlayerAction(ActionType actionType)
    {
        if (IsExecuting)
        {
            return false;
        }

        int actionPerTurn =
            GetActionsPerTurn();

        if (playerActions.Count >= actionPerTurn)
        {
            Debug.Log(
                "このターンの行動は" +
                actionPerTurn +
                "つまでです。"
            );

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
            "/" +
            actionPerTurn +
            ")"
        );

        RefreshEnemyFutureUI();

        return true;
    }


    // ========================================
    // プレイヤースキル予約
    // ========================================

    public bool AddPlayerSkill(SkillData skill)
    {
        if (IsExecuting)
        {
            return false;
        }

        int actionPerTurn =
            GetActionsPerTurn();

        if (playerActions.Count >= actionPerTurn)
        {
            Debug.Log(
                "このターンの行動は" +
                actionPerTurn +
                "つまでです。"
            );

            return false;
        }

        if (skill == null)
        {
            Debug.LogError(
                "SkillDataが設定されていません。"
            );

            return false;
        }

        // 同じスキルを予約しているか
        foreach (BattleAction paction in playerActions)
        {
            if (paction.actionType == ActionType.Skill &&
                paction.skillData == skill)
            {
                Debug.Log(
                    "スキル「" +
                    skill.skillName +
                    "」はこのターンにすでに使用予約されています。"
                );

                return false;
            }
        }

        // 1ターンにスキルは1回まで
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

        // クールタイム
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
            "/" +
            actionPerTurn +
            ")"
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


    // ========================================
    // プレイヤー行動クリア
    // ========================================

    public void ClearPlayerActions()
    {
        if (IsExecuting)
        {
            return;
        }

        playerActions.Clear();

        Debug.Log(
            "プレイヤーの予約行動をすべてクリアしました。"
        );

        OnActionsReset?.Invoke();
    }


    // ========================================
    // ターン開始
    // ========================================

    public void StartTurn()
    {
        playerActions.Clear();

        IsExecuting = false;

        Debug.Log(
            "===== TURN " +
            CurrentTurn +
            " ====="
        );

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

        int actionPerTurn =
            GetActionsPerTurn();

        if (playerActions.Count != actionPerTurn)
        {
            Debug.Log(
                actionPerTurn +
                "つの行動を選択してください。"
            );

            return;
        }

        StartCoroutine(
            ExecuteActions()
        );
    }


    // ========================================
    // 行動実行
    // ========================================

    private IEnumerator ExecuteActions()
    {
        IsExecuting = true;

        List<BattleAction> enemyActions =
            BattleManager.Instance
                .EnemyAI
                .GetEnemyActions();

        int playerActionPerTurn =
            GetActionsPerTurn();

        if (enemyActions == null)
        {
            Debug.LogError(
                "敵の行動が作成されていません。"
            );

            IsExecuting = false;

            yield break;
        }


        // ========================================
        // ① → ② → ③
        // ========================================

        for (int i = 0; i < playerActionPerTurn; i++)
        {
            // 戦闘終了チェック
            if (BattleManager.Instance.Player.IsDead ||
                BattleManager.Instance.Enemy.IsDead)
            {
                IsExecuting = false;
                yield break;
            }


            // プレイヤー行動数チェック
            if (i >= playerActions.Count)
            {
                Debug.LogError(
                    "プレイヤーの行動数が不足しています。" +
                    " 必要：" +
                    playerActionPerTurn +
                    " / 実際：" +
                    playerActions.Count
                );

                IsExecuting = false;

                yield break;
            }


            BattleAction playerAction =
                playerActions[i];


            // ====================================
            // 敵行動取得
            // ====================================

            BattleAction enemyAction = null;

            if (i < enemyActions.Count)
            {
                enemyAction =
                    enemyActions[i];
            }


            Debug.Log(
                "========== ACTION " +
                (i + 1) +
                " =========="
            );


            // ====================================
            // 敵行動がない
            // ====================================

            if (enemyAction == null)
            {
                Debug.Log(
                    "Player : " +
                    GetActionDisplayName(playerAction) +
                    " / SPEED " +
                    playerAction.speed
                );

                Debug.Log(
                    "Enemy : ---"
                );

                yield return ExecutePlayerAction(
                    playerAction,
                    false
                );

                if (BattleManager.Instance.Player.IsDead ||
                    BattleManager.Instance.Enemy.IsDead)
                {
                    IsExecuting = false;
                    yield break;
                }

                continue;
            }


            // ====================================
            // 通常の1対1
            // ====================================

            Debug.Log(
                "Player : " +
                GetActionDisplayName(playerAction) +
                " / SPEED " +
                playerAction.speed
            );

            Debug.Log(
                "Enemy : " +
                GetActionDisplayName(enemyAction) +
                " / SPEED " +
                enemyAction.speed
            );


            bool enemyDefending =
                enemyAction.actionType ==
                ActionType.Defend;

            bool playerDefending =
                playerAction.actionType ==
                ActionType.Defend;


            // ====================================
            // プレイヤー先攻
            // ====================================

            if (playerAction.speed >= enemyAction.speed)
            {
                yield return ExecutePlayerAction(
                    playerAction,
                    enemyDefending
                );


                if (BattleManager.Instance.Player.IsDead ||
                    BattleManager.Instance.Enemy.IsDead)
                {
                    IsExecuting = false;
                    yield break;
                }


                yield return ExecuteEnemyActionWithDelay(
                    enemyAction,
                    playerDefending
                );
            }


            // ====================================
            // 敵先攻
            // ====================================

            else
            {
                yield return ExecuteEnemyActionWithDelay(
                    enemyAction,
                    playerDefending
                );


                if (BattleManager.Instance.Player.IsDead ||
                    BattleManager.Instance.Enemy.IsDead)
                {
                    IsExecuting = false;
                    yield break;
                }


                yield return ExecutePlayerAction(
                    playerAction,
                    enemyDefending
                );
            }


            // ====================================
            // 次のACTIONへ
            //
            // 各行動自身が2秒待っているため、
            // ここでは追加待機しない
            // ====================================
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
        if (action == null)
        {
            yield break;
        }

        BattleUnit player =
            BattleManager.Instance.Player;


        switch (action.actionType)
        {
            // ====================================
            // 攻撃
            // ====================================

            case ActionType.Attack:

                Debug.Log(
                    "Player：攻撃！ SPEED " +
                    action.speed
                );

                // アニメーション開始
                player.PlayAttackAnimation();

                // 約1秒
                yield return new WaitForSeconds(
                    damageDelay
                );

                // ダメージ処理
                BattleManager.Instance.PlayerAttack(
                    enemyDefending
                );

                // BattleUnit.TakeDamage()側で
                // Hitアニメーションが再生される

                // ヒット後2秒
                yield return new WaitForSeconds(
                    hitDelay
                );

                break;


            // ====================================
            // 防御
            // ====================================

            case ActionType.Defend:

                Debug.Log(
                    "Player：防御！ SPEED " +
                    action.speed
                );

                // 防御アニメーション
                player.PlayDefendAnimation();

                // 防御効果を少し見せてから適用
                yield return new WaitForSeconds(
                    damageDelay
                );

                player.Defend();

                // 次の行動まで待つ
                yield return new WaitForSeconds(
                    hitDelay
                );

                break;


            // ====================================
            // スキル
            // ====================================

            case ActionType.Skill:

                if (action.skillData == null)
                {
                    Debug.LogWarning(
                        "スキルデータがありません。"
                    );

                    yield break;
                }

                Debug.Log(
                    "Player：" +
                    action.skillData.skillName +
                    "！ SPEED " +
                    action.speed
                );


                // スキルアニメーション
                player.PlaySkillAnimation(
                    action.skillData
                );


                // 約1秒
                yield return new WaitForSeconds(
                    damageDelay
                );


                // スキル効果
                BattleManager.Instance.ExecutePlayerSkill(
                    action.skillData,
                    enemyDefending
                );


                // クールタイム開始
                player.StartSkillCooldown(
                    action.skillData
                );


                // Hitアニメーションは
                // TakeDamage()側で必要な場合のみ再生


                // ヒット後2秒
                yield return new WaitForSeconds(
                    hitDelay
                );

                break;
        }
    }


    // ========================================
    // Enemyの行動
    // ========================================

    private IEnumerator ExecuteEnemyActionWithDelay(
        BattleAction action,
        bool playerDefending)
    {
        if (action == null)
        {
            yield break;
        }

        BattleUnit enemy =
            BattleManager.Instance.Enemy;


        switch (action.actionType)
        {
            // ====================================
            // 攻撃
            // ====================================

            case ActionType.Attack:

                Debug.Log(
                    "Enemy：攻撃！ SPEED " +
                    action.speed
                );

                // 攻撃アニメーション
                enemy.PlayAttackAnimation();

                // 約1秒
                yield return new WaitForSeconds(
                    damageDelay
                );

                // ダメージ
                BattleManager.Instance.ExecuteEnemyAction(
                    action,
                    playerDefending
                );

                // HitはTakeDamage()側


                // ヒット後2秒
                yield return new WaitForSeconds(
                    hitDelay
                );

                break;


            // ====================================
            // 防御
            // ====================================

            case ActionType.Defend:

                Debug.Log(
                    "Enemy：防御！ SPEED " +
                    action.speed
                );

                // 防御アニメーション
                enemy.PlayDefendAnimation();

                // 約1秒
                yield return new WaitForSeconds(
                    damageDelay
                );

                // 防御効果
                BattleManager.Instance.ExecuteEnemyAction(
                    action,
                    playerDefending
                );

                // 次の行動まで2秒
                yield return new WaitForSeconds(
                    hitDelay
                );

                break;


            // ====================================
            // 回復
            // ====================================

            case ActionType.Heal:

                Debug.Log(
                    "Enemy：回復！ SPEED " +
                    action.speed
                );

                // 回復アニメーション
                enemy.PlayHealAnimation();

                // 約1秒
                yield return new WaitForSeconds(
                    damageDelay
                );

                // 回復処理
                BattleManager.Instance.ExecuteEnemyAction(
                    action,
                    playerDefending
                );

                // 次の行動まで2秒
                yield return new WaitForSeconds(
                    hitDelay
                );

                break;


            // ====================================
            // スキル
            // ====================================

            case ActionType.Skill:

                if (action.skillData == null)
                {
                    Debug.LogWarning(
                        "Enemy SkillDataがありません。"
                    );

                    yield break;
                }

                Debug.Log(
                    "Enemy：" +
                    action.skillData.skillName +
                    "！ SPEED " +
                    action.speed
                );


                // スキルアニメーション
                enemy.PlaySkillAnimation(
                    action.skillData
                );


                // 約1秒
                yield return new WaitForSeconds(
                    damageDelay
                );


                // スキル効果
                BattleManager.Instance.ExecuteEnemyAction(
                    action,
                    playerDefending
                );


                // HitはTakeDamage()側


                // ヒット後2秒
                yield return new WaitForSeconds(
                    hitDelay
                );

                break;


            // ====================================
            // その他
            // ====================================

            default:

                BattleManager.Instance.ExecuteEnemyAction(
                    action,
                    playerDefending
                );

                yield return new WaitForSeconds(
                    actionInterval
                );

                break;
        }
    }


    // ========================================
    // 行動表示名
    // ========================================

    private string GetActionDisplayName(
        BattleAction action)
    {
        if (action == null)
        {
            return "---";
        }

        if (action.actionType ==
            ActionType.Skill)
        {
            if (action.skillData != null)
            {
                return action.skillData.skillName;
            }

            return "スキル";
        }

        switch (action.actionType)
        {
            case ActionType.Attack:
                return "攻撃";

            case ActionType.Defend:
                return "防御";

            case ActionType.Heal:
                return "回復";

            default:
                return "---";
        }
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
        if (RoguelikeManager.Instance != null &&
            RoguelikeManager.Instance.PlayerData != null)
        {
            RoguelikeManager.Instance.PlayerData
                .ReduceItemCooldown();
        }


        // 次のターン
        CurrentTurn++;

        IsExecuting = false;


        Debug.Log(
            "===== NEXT TURN : " +
            CurrentTurn +
            " ====="
        );


        BattleManager.Instance.StartTurn();
    }


    // ========================================
    // 行動置き換え
    // ========================================

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


    // ========================================
    // スキル置き換え
    // ========================================

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


    // ========================================
    // バトル停止
    // ========================================

    public void StopBattle()
    {
        StopAllCoroutines();

        IsExecuting = false;

        Debug.Log(
            "TurnManager：バトル停止"
        );
    }
}