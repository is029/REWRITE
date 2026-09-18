using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("敵スキル")]
    [SerializeField] protected SkillData[] skills;

    // スキルごとの重み
    private int skill1Weight = 33;
    private int skill2Weight = 33;
    private int skill3Weight = 34;

    // 行動の重みづけ
    private int attackWeight = 60;
    private int defendWeight = 20;
    private int healWeight = 0;
    private int skillWeight = 20;

    private List<BattleAction> enemyActions =
        new List<BattleAction>();

    public List<BattleAction> GetEnemyActions()
    {
        return enemyActions;
    }

    // ========================================
    // ランダムスキル取得
    // ========================================
    protected SkillData GetRandomSkill()
    {
        if (skills == null || skills.Length == 0)
        {
            return null;
        }

        int[] weights =
        {
        skill1Weight,
        skill2Weight,
        skill3Weight
    };

        int totalWeight = 0;

        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] != null)
            {
                totalWeight += weights[i];
            }
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int random =
            Random.Range(0, totalWeight);

        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] == null)
            {
                continue;
            }

            random -= weights[i];

            if (random < 0)
            {
                return skills[i];
            }
        }

        return null;
    }

    // ========================================
    // スキルの重み取得
    // ========================================

    private int GetSkillWeight(int index)
    {
        switch (index)
        {
            case 0:
                return skill1Weight;

            case 1:
                return skill2Weight;

            case 2:
                return skill3Weight;

            default:
                return 0;
        }
    }

    // ========================================
    // 敵の行動作成
    // ========================================

    public virtual void CreateActions()
    {
        enemyActions.Clear();

        int actionsPerTurn = 3;

        if (RuleManager.Instance != null)
        {
            actionsPerTurn =
                RuleManager.Instance
                    .CurrentEnemyActionsPerTurn;
        }

        for (int i = 0; i < actionsPerTurn; i++)
        {
            ActionType actionType =
                GetRandomAction();

            SkillData skill = null;

            // スキル行動なら
            // スキルごとの重みから決定
            if (actionType == ActionType.Skill)
            {
                skill = GetRandomSkill();

                // スキルが存在しない場合は攻撃に変更
                if (skill == null)
                {
                    actionType =
                        ActionType.Attack;
                }
            }

            BattleAction action =
                new BattleAction(
                    actionType,
                    false,
                    BattleManager.Instance.Enemy,
                    skill
                );

            enemyActions.Add(action);
        }

        Debug.Log(
            "敵の未来を決定しました。" +
            " 行動数：" +
            enemyActions.Count
        );

        for (int i = 0; i < enemyActions.Count; i++)
        {
            BattleAction action =
                enemyActions[i];

            Debug.Log(
                "敵 ACTION " +
                (i + 1) +
                " : " +
                GetActionName(action) +
                " / SPEED " +
                action.speed
            );
        }
    }

    // ========================================
    // 行動の重み
    // ========================================

    private ActionType GetRandomAction()
    {
        int totalWeight =
            Mathf.Max(0, attackWeight) +
            Mathf.Max(0, defendWeight) +
            Mathf.Max(0, skillWeight) +
            Mathf.Max(0, healWeight);

        if (totalWeight <= 0)
        {
            return ActionType.Attack;
        }

        int random =
            Random.Range(0, totalWeight);

        // 攻撃
        if (random < attackWeight)
        {
            return ActionType.Attack;
        }

        random -= attackWeight;

        // 防御
        if (random < defendWeight)
        {
            return ActionType.Defend;
        }

        random -= defendWeight;

        // スキル
        if (random < skillWeight)
        {
            return ActionType.Skill;
        }

        // 回復
        return ActionType.Heal;
    }

    // ========================================
    // 予測Speed
    // ========================================

    public int GetPredictedSpeed(
        int actionIndex,
        int speedModifier)
    {
        List<BattleAction> actions =
            GetEnemyActions();

        if (actions == null ||
            actionIndex < 0 ||
            actionIndex >= actions.Count)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            actions[actionIndex].speed +
            speedModifier
        );
    }

    // ========================================
    // キャラクター変更
    // ========================================
    public void SetCharacterData(
    CharacterData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "EnemyAIに渡されたCharacterDataがnullです。"
            );

            return;
        }

        // スキル
        skills = new SkillData[]
        {
        data.skill1,
        data.skill2,
        data.skill3
        };

        // 行動の重み
        attackWeight =
            Mathf.Max(0, data.attackWeight);

        defendWeight =
            Mathf.Max(0, data.defendWeight);

        healWeight =
            Mathf.Max(0, data.healWeight);

        skillWeight =
            Mathf.Max(0, data.skillWeight);

        // スキルごとの重み
        skill1Weight =
            Mathf.Max(0, data.skill1Weight);

        skill2Weight =
            Mathf.Max(0, data.skill2Weight);

        skill3Weight =
            Mathf.Max(0, data.skill3Weight);

        Debug.Log(
            "===== ENEMY AI SET =====\n" +
            "Character : " + data.characterName + "\n" +
            "Attack : " + attackWeight + "\n" +
            "Defend : " + defendWeight + "\n" +
            "Heal : " + healWeight + "\n" +
            "Skill : " + skillWeight + "\n" +
            "Skill1 : " + skill1Weight + "\n" +
            "Skill2 : " + skill2Weight + "\n" +
            "Skill3 : " + skill3Weight
        );
    }

    // ========================================
    // 行動名
    // ========================================

    private string GetActionName(
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
}