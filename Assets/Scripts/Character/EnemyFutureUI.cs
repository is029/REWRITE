using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyFutureUI : MonoBehaviour
{
    [Header("Enemy Future Text")]
    [SerializeField] private TMP_Text action1Text;
    [SerializeField] private TMP_Text action2Text;
    [SerializeField] private TMP_Text action3Text;
    [SerializeField] private TMP_Text action4Text;


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
                "BattleSimulatorÇ™Ç†ÇËÇ‹ÇπÇÒÅB"
            );

            return;
        }

        if (BattleManager.Instance == null)
        {
            return;
        }

        if (BattleManager.Instance.EnemyAI == null)
        {
            return;
        }

        if (BattleManager.Instance.TurnManager == null)
        {
            return;
        }


        // ====================================
        // åªç›ÇÃçsìÆêî
        // ====================================

        int actionsPerTurn =
            BattleManager.Instance
                .TurnManager
                .GetEnemyActionsPerTurn();


        // ====================================
        // ìGÇÃó\ñÒçsìÆéÊìæ
        // ====================================

        List<BattleAction> enemyActions =
            BattleManager.Instance
                .EnemyAI
                .GetEnemyActions();


        if (enemyActions == null)
        {
            return;
        }


        // ====================================
        // ñ¢óàÇÉVÉ~ÉÖÉåÅ[ÉVÉáÉì
        // ====================================

        List<BattleSimulationState> states =
            BattleSimulator.Instance
                .SimulateTurn(
                    playerActions,
                    enemyActions
                );


        if (states == null)
        {
            return;
        }


        // ====================================
        // á@
        // ====================================

        if (action1Text != null)
        {
            action1Text.gameObject.SetActive(
                actionsPerTurn >= 1 &&
                enemyActions.Count >= 1 &&
                states.Count >= 1
            );
        }

        if (actionsPerTurn >= 1 &&
            enemyActions.Count >= 1 &&
            states.Count >= 1)
        {
            ShowAction(
                action1Text,
                1,
                enemyActions[0],
                states[0]
            );
        }


        // ====================================
        // áA
        // ====================================

        if (action2Text != null)
        {
            action2Text.gameObject.SetActive(
                actionsPerTurn >= 2 &&
                enemyActions.Count >= 2 &&
                states.Count >= 2
            );
        }

        if (actionsPerTurn >= 2 &&
            enemyActions.Count >= 2 &&
            states.Count >= 2)
        {
            ShowAction(
                action2Text,
                2,
                enemyActions[1],
                states[1]
            );
        }


        // ====================================
        // áB
        // ====================================

        if (action3Text != null)
        {
            action3Text.gameObject.SetActive(
                actionsPerTurn >= 3 &&
                enemyActions.Count >= 3 &&
                states.Count >= 3
            );
        }

        if (actionsPerTurn >= 3 &&
            enemyActions.Count >= 3 &&
            states.Count >= 3)
        {
            ShowAction(
                action3Text,
                3,
                enemyActions[2],
                states[2]
            );
        }


        // ====================================
        // áC
        // ====================================

        if (action4Text != null)
        {
            action4Text.gameObject.SetActive(
                actionsPerTurn >= 4 &&
                enemyActions.Count >= 4 &&
                states.Count >= 4
            );
        }

        if (actionsPerTurn >= 4 &&
            enemyActions.Count >= 4 &&
            states.Count >= 4)
        {
            ShowAction(
                action4Text,
                4,
                enemyActions[3],
                states[3]
            );
        }
    }


    // ========================================
    // ãÛï\é¶
    // ========================================

    private void SetEmpty(
        TMP_Text text,
        int index)
    {
        if (text == null)
        {
            return;
        }

        text.text =
            index +
            ". ---";
    }


    // ========================================
    // çsìÆï\é¶
    // ========================================

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
            SetEmpty(
                text,
                index
            );

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
        // å≥SpeedÇ∆ÇÃî‰är
        // ====================================

        string speedText =
            speed.ToString();

        if (speed != action.speed)
        {
            if (speed < action.speed)
            {
                speedText += " Å´";
            }
            else
            {
                speedText += " Å™";
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
        // ï\é¶
        // ====================================

        text.text =
            index +
            ". " +
            GetActionName(action) +
            "\n" +
            "SPEED " +
            speedText +
            "\n" +
            hpText;
    }


    // ========================================
    // çsìÆñº
    // ========================================

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

            return "ÉXÉLÉã";
        }


        switch (action.actionType)
        {
            case ActionType.Attack:
                return "çUåÇ";

            case ActionType.Defend:
                return "ñhå‰";

            case ActionType.Heal:
                return "âÒïú";

            default:
                return "---";
        }
    }
}