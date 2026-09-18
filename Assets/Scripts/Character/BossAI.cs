using System.Collections.Generic;
using UnityEngine;

public class BossAI : EnemyAI
{
    [Header("===== Phase 1 行動重み =====")]

    [SerializeField]
    private int phase1AttackWeight = 50;

    [SerializeField]
    private int phase1DefendWeight = 25;

    [SerializeField]
    private int phase1HealWeight = 0;

    [SerializeField]
    private int phase1SkillWeight = 25;


    [Header("===== Phase 2 行動重み =====")]

    [SerializeField]
    private int phase2AttackWeight = 35;

    [SerializeField]
    private int phase2DefendWeight = 10;

    [SerializeField]
    private int phase2HealWeight = 0;

    [SerializeField]
    private int phase2SkillWeight = 55;


    private void Start()
    {
        Debug.Log("===== BOSS AI START =====");
    }


    // ========================================
    // 行動作成
    // ========================================

    public override void CreateActions()
    {
        List<BattleAction> actions =
            GetEnemyActions();

        actions.Clear();

        BattleUnit boss =
            BattleManager.Instance.Enemy;

        if (boss == null)
        {
            Debug.LogError(
                "BossAI：Enemyが見つかりません。"
            );

            return;
        }


        // ========================================
        // HP割合
        // ========================================

        float hpRate =
            (float)boss.CurrentHP / boss.MaxHP;


        // ========================================
        // 1ターンの行動数
        // ========================================

        int actionsPerTurn = 3;

        if (RuleManager.Instance != null)
        {
            actionsPerTurn =
                RuleManager.Instance.CurrentEnemyActionsPerTurn;
        }


        // ========================================
        // Phase判定
        // ========================================

        if (hpRate > 0.5f)
        {
            Debug.Log(
                "===== BOSS PHASE 1 ====="
            );

            CreatePhase1Actions(
                boss,
                actionsPerTurn
            );
        }
        else
        {
            Debug.Log(
                "===== BOSS PHASE 2 ====="
            );

            CreatePhase2Actions(
                boss,
                actionsPerTurn
            );
        }


        // ========================================
        // デバッグ
        // ========================================

        Debug.Log(
            "===== BOSSの未来を決定 ====="
        );

        for (int i = 0; i < actions.Count; i++)
        {
            string actionName =
                actions[i].actionType.ToString();

            if (
                actions[i].actionType == ActionType.Skill &&
                actions[i].skillData != null
            )
            {
                actionName +=
                    " [" +
                    actions[i].skillData.skillName +
                    "]";
            }

            Debug.Log(
                "BOSS ACTION " +
                (i + 1) +
                " : " +
                actionName +
                " / SPEED " +
                actions[i].speed
            );
        }
    }


    // ========================================
    // Phase 1
    // ========================================

    private void CreatePhase1Actions(
        BattleUnit boss,
        int actionsPerTurn)
    {
        List<BattleAction> actions =
            GetEnemyActions();

        for (int i = 0; i < actionsPerTurn; i++)
        {
            CreateWeightedAction(
                boss,
                actions,

                phase1AttackWeight,
                phase1DefendWeight,
                phase1HealWeight,
                phase1SkillWeight
            );
        }
    }


    // ========================================
    // Phase 2
    // ========================================

    private void CreatePhase2Actions(
        BattleUnit boss,
        int actionsPerTurn)
    {
        List<BattleAction> actions =
            GetEnemyActions();

        for (int i = 0; i < actionsPerTurn; i++)
        {
            CreateWeightedAction(
                boss,
                actions,

                phase2AttackWeight,
                phase2DefendWeight,
                phase2HealWeight,
                phase2SkillWeight
            );
        }
    }


    // ========================================
    // 重み付き行動選択
    // ========================================

    private void CreateWeightedAction(
        BattleUnit boss,
        List<BattleAction> actions,

        int attackWeight,
        int defendWeight,
        int healWeight,
        int skillWeight)
    {
        int totalWeight =
            attackWeight +
            defendWeight +
            healWeight +
            skillWeight;


        // ========================================
        // 全部0
        // ========================================

        if (totalWeight <= 0)
        {
            Debug.LogWarning(
                "BossAI：重みがすべて0です。" +
                "攻撃を選択します。"
            );

            AddAttack(
                boss,
                actions
            );

            return;
        }


        int random =
            Random.Range(0, totalWeight);


        // ========================================
        // Attack
        // ========================================

        if (random < attackWeight)
        {
            AddAttack(
                boss,
                actions
            );

            return;
        }

        random -= attackWeight;


        // ========================================
        // Defend
        // ========================================

        if (random < defendWeight)
        {
            AddDefend(
                boss,
                actions
            );

            return;
        }

        random -= defendWeight;


        // ========================================
        // Heal
        // ========================================

        if (random < healWeight)
        {
            AddHeal(
                boss,
                actions
            );

            return;
        }

        random -= healWeight;


        // ========================================
        // Skill
        // ========================================

        SkillData skill =
            GetRandomSkill();

        if (skill != null)
        {
            actions.Add(
                new BattleAction(
                    ActionType.Skill,
                    false,
                    boss,
                    skill
                )
            );

            return;
        }


        // ========================================
        // スキルが存在しない
        // ========================================

        Debug.LogWarning(
            "BossAI：Skillが設定されていません。" +
            "Attackに変更します。"
        );

        AddAttack(
            boss,
            actions
        );
    }


    // ========================================
    // Attack
    // ========================================

    private void AddAttack(
        BattleUnit boss,
        List<BattleAction> actions)
    {
        actions.Add(
            new BattleAction(
                ActionType.Attack,
                false,
                boss
            )
        );
    }


    // ========================================
    // Defend
    // ========================================

    private void AddDefend(
        BattleUnit boss,
        List<BattleAction> actions)
    {
        actions.Add(
            new BattleAction(
                ActionType.Defend,
                false,
                boss
            )
        );
    }


    // ========================================
    // Heal
    // ========================================

    private void AddHeal(
        BattleUnit boss,
        List<BattleAction> actions)
    {
        actions.Add(
            new BattleAction(
                ActionType.Heal,
                false,
                boss
            )
        );
    }
}