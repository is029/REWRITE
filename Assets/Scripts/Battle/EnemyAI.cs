using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public const int ActionsPerTurn = 3;

    private List<BattleAction> enemyActions =
        new List<BattleAction>();

    public List<BattleAction> GetEnemyActions()
    {
        return enemyActions;
    }

    public void CreateActions()
    {
        enemyActions.Clear();

        for (int i = 0; i < ActionsPerTurn; i++)
        {
            ActionType actionType =
                GetRandomAction();

            BattleAction action =
                new BattleAction(
                    actionType,
                    false,
                    BattleManager.Instance.Enemy
                );

            enemyActions.Add(action);
        }

        Debug.Log("“G‚Ì–¢—ˆ‚ðŒˆ’è‚µ‚Ü‚µ‚½B");

        for (int i = 0; i < enemyActions.Count; i++)
        {
            Debug.Log(
                "“G ACTION " +
                (i + 1) +
                " : " +
                enemyActions[i].actionType +
                " / SPEED " +
                enemyActions[i].speed
            );
        }
    }

    private ActionType GetRandomAction()
    {
        int random = Random.Range(0, 100);

        if (random < 60)
        {
            return ActionType.Attack;
        }
        else if (random < 80)
        {
            return ActionType.Defend;
        }
        else
        {
            return ActionType.Heal;
        }
    }

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
            actions[actionIndex].speed + speedModifier
        );
    }
}