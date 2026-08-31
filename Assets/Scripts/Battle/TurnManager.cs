using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public const int ActionsPerTurn = 3;

    private List<BattleAction> playerActions =
        new List<BattleAction>();

    private int playerSpeedModifier = 0;
    private int enemySpeedModifier = 0;

    public int CurrentTurn { get; private set; } = 1;

    public int CurrentActionIndex
    {
        get { return playerActions.Count; }
    }

    public bool IsExecuting { get; private set; }


    // ========================================
    // プレイヤーの通常行動を予約
    // ========================================

    public bool AddPlayerAction(ActionType actionType)
    {
        if (IsExecuting)
        {
            return false;
        }

        if (playerActions.Count >= ActionsPerTurn)
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

        if (playerActions.Count >= ActionsPerTurn)
        {
            Debug.Log("このターンの行動は3つまでです。");
            return false;
        }

        if (skill == null)
        {
            Debug.LogError("SkillDataが設定されていません。");
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


    // ========================================
    // ターン開始
    // ========================================

    public void StartTurn()
    {
        playerActions.Clear();

        playerSpeedModifier = 0;
        enemySpeedModifier = 0;

        IsExecuting = false;

        Debug.Log(
            "===== TURN " +
            CurrentTurn +
            " ====="
        );

        RefreshEnemyFutureUI();
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

        if (playerActions.Count != ActionsPerTurn)
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
            BattleManager.Instance.EnemyAI
            .GetEnemyActions();

        if (enemyActions == null ||
            enemyActions.Count < ActionsPerTurn)
        {
            Debug.LogError(
                "敵の行動が3つ作成されていません。"
            );

            IsExecuting = false;

            yield break;
        }

        playerSpeedModifier = 0;
        enemySpeedModifier = 0;


        // ====================================
        // ① → ② → ③
        // ====================================

        for (int i = 0; i < ActionsPerTurn; i++)
        {
            BattleAction playerAction =
                playerActions[i];

            BattleAction enemyAction =
                enemyActions[i];


            int playerSpeed =
                playerAction.speed +
                playerSpeedModifier;

            int enemySpeed =
                enemyAction.speed +
                enemySpeedModifier;


            Debug.Log(
                "========== ACTION " +
                (i + 1) +
                " =========="
            );

            Debug.Log(
                "Player : " +
                playerAction.actionType +
                " / SPEED " +
                playerSpeed
            );

            Debug.Log(
                "Enemy : " +
                enemyAction.actionType +
                " / SPEED " +
                enemySpeed
            );


            // =================================
            // この番号だけの防御状態
            // =================================

            bool playerDefending =
                playerAction.actionType ==
                ActionType.Defend;

            bool enemyDefending =
                enemyAction.actionType ==
                ActionType.Defend;


            // =================================
            // Speed比較
            // =================================

            if (playerSpeed > enemySpeed)
            {
                // Playerが先
                ExecutePlayerAction(
                    playerAction,
                    enemyDefending
                );

                yield return new WaitForSeconds(0.5f);


                // Enemy
                BattleManager.Instance.ExecuteEnemyAction(
                    enemyAction,
                    playerDefending
                );
            }
            else
            {
                // Enemyが先
                BattleManager.Instance.ExecuteEnemyAction(
                    enemyAction,
                    playerDefending
                );

                yield return new WaitForSeconds(0.5f);


                // Player
                ExecutePlayerAction(
                    playerAction,
                    enemyDefending
                );
            }


            yield return new WaitForSeconds(0.5f);
        }


        EndTurn();
    }


    // ========================================
    // Playerの行動
    // ========================================

    private void ExecutePlayerAction(
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

                BattleManager.Instance.ExecutePlayerSkill(
                    action.skillData,
                    enemyDefending
                );

                break;
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


        // ====================================
        // Buff / Debuffのターン経過
        // ====================================

        BattleManager.Instance.Player
            .EndTurnEffects();

        BattleManager.Instance.Enemy
            .EndTurnEffects();


        CurrentTurn++;

        IsExecuting = false;


        // 次のターン
        BattleManager.Instance.StartTurn();
    }
}