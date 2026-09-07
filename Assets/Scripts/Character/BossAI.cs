using System.Collections.Generic;
using UnityEngine;

public class BossAI : EnemyAI
{
    private void Start()
    {
        Debug.Log("===== BOSS AI START =====");
    }

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
                "BossAIFEnemy‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñB"
            );

            return;
        }

        float hpRate =
            (float)boss.CurrentHP / boss.MaxHP;

        if (hpRate > 0.5f)
        {
            CreatePhase1Actions(boss);
        }
        else
        {
            CreatePhase2Actions(boss);
        }

        Debug.Log(
            "===== BOSS‚Ì–¢—ˆ‚ðŒˆ’è ====="
        );

        for (int i = 0; i < actions.Count; i++)
        {
            Debug.Log(
                "BOSS ACTION " +
                (i + 1) +
                " : " +
                actions[i].actionType +
                " / SPEED " +
                actions[i].speed
            );
        }
    }

    // HP50%ˆÈã
    private void CreatePhase1Actions(
        BattleUnit boss)
    {
        List<BattleAction> actions =
            GetEnemyActions();

        actions.Add(
            new BattleAction(
                ActionType.Attack,
                false,
                boss
            )
        );

        actions.Add(
            new BattleAction(
                ActionType.Attack,
                false,
                boss
            )
        );

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
        }
        else
        {
            actions.Add(
                new BattleAction(
                    ActionType.Defend,
                    false,
                    boss
                )
            );
        }
    }

    // HP50%ˆÈ‰º
    private void CreatePhase2Actions(
        BattleUnit boss)
    {
        List<BattleAction> actions =
            GetEnemyActions();

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
        }
        else
        {
            actions.Add(
                new BattleAction(
                    ActionType.Attack,
                    false,
                    boss
                )
            );
        }

        actions.Add(
            new BattleAction(
                ActionType.Attack,
                false,
                boss
            )
        );

        actions.Add(
            new BattleAction(
                ActionType.Attack,
                false,
                boss
            )
        );
    }
}