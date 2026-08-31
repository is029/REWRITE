using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyFutureUI : MonoBehaviour
{
    [Header("Enemy Future Text")]
    [SerializeField] private TMP_Text action1Text;
    [SerializeField] private TMP_Text action2Text;
    [SerializeField] private TMP_Text action3Text;


    public void ShowEnemyFuture()
    {
        if (BattleManager.Instance == null)
        {
            return;
        }

        if (BattleManager.Instance.TurnManager == null)
        {
            return;
        }

        ShowEnemyFuture(
            BattleManager.Instance
                .TurnManager
                .GetPlayerActions()
        );
    }


    public void ShowEnemyFuture(
        List<BattleAction> playerActions)
    {
        if (BattleSimulator.Instance == null)
        {
            Debug.LogWarning(
                "BattleSimulatorがありません。"
            );

            return;
        }

        List<BattleAction> enemyActions =
            BattleManager.Instance
                .EnemyAI
                .GetEnemyActions();

        if (enemyActions == null ||
            enemyActions.Count < 3)
        {
            return;
        }


        // ====================================
        // 未来をシミュレーション
        // ====================================

        List<BattleSimulationState> states =
            BattleSimulator.Instance
                .SimulateTurn(
                    playerActions,
                    enemyActions
                );


        if (states == null ||
            states.Count < 3)
        {
            return;
        }


        // ====================================
        // UI表示
        // ====================================

        ShowAction(
            action1Text,
            1,
            enemyActions[0],
            states[0]
        );

        ShowAction(
            action2Text,
            2,
            enemyActions[1],
            states[1]
        );

        ShowAction(
            action3Text,
            3,
            enemyActions[2],
            states[2]
        );
    }


    private void ShowAction(
        TMP_Text text,
        int index,
        BattleAction action,
        BattleSimulationState state)
    {
        if (text == null)
        {
            return;
        }

        if (action == null ||
            state == null)
        {
            text.text =
                index +
                ". ---";

            return;
        }


        // ====================================
        // Speed
        // ====================================

        int speed =
            action.speed +
            state.enemySpeedModifier;

        speed =
            Mathf.Max(
                0,
                speed
            );


        // ====================================
        // HP
        // ====================================

        int hp =
            Mathf.Max(
                0,
                state.enemyHP
            );


        // ====================================
        // 元Speedとの比較
        // ====================================

        string speedText =
            speed.ToString();

        if (speed != action.speed)
        {
            if (speed < action.speed)
            {
                speedText +=
                    " ↓";
            }
            else
            {
                speedText +=
                    " ↑";
            }
        }


        // ====================================
        // HP
        // ====================================

        string hpText =
            "HP " +
            hp +
            "/" +
            state.enemyMaxHP;


        // ====================================
        // 表示
        // ====================================

        text.text =
            index +
            ". " +
            GetActionName(
                action
            ) +
            "\n" +
            "SPEED " +
            speedText +
            "\n" +
            hpText;
    }


    private string GetActionName(
        BattleAction action)
    {
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