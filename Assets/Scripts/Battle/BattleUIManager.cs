using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    [Header("Turn")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text actionCountText;

    [Header("Action Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button executeButton;
    [SerializeField] private Button clearButton;

    [Header("Reserved Actions")]
    [SerializeField] private TMP_Text action1Text;
    [SerializeField] private TMP_Text action2Text;
    [SerializeField] private TMP_Text action3Text;
    [SerializeField] private TMP_Text action4Text;

    [Header("HP")]
    [SerializeField] private HPBarUI playerHPBar;
    [SerializeField] private HPBarUI enemyHPBar;

    [Header("Shield")]
    [SerializeField] private TMP_Text playerShieldText;
    [SerializeField] private TMP_Text enemyShieldText;

    [SerializeField]
    private SkillUIManager skillUIManager;

    private TurnManager turnManager;

    private int selectedActionIndex = -1;

    private void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        turnManager.OnActionsReset += OnTurnReset;

        attackButton.onClick.AddListener(OnAttackButton);
        defendButton.onClick.AddListener(OnDefendButton);
        skillButton.onClick.AddListener(OnSkillButton);
        executeButton.onClick.AddListener(OnExecuteButton);

        clearButton.onClick.AddListener(OnClearButton);

        BattleUnit player = BattleManager.Instance.Player;
        BattleUnit enemy = BattleManager.Instance.Enemy;

        player.OnHPChanged += UpdatePlayerHP;
        enemy.OnHPChanged += UpdateEnemyHP;

        player.OnShieldChanged += UpdatePlayerShield;
        enemy.OnShieldChanged += UpdateEnemyShield;

        UpdatePlayerHP(
            player.CurrentHP,
            player.MaxHP
        );

        UpdateEnemyHP(
            enemy.CurrentHP,
            enemy.MaxHP
        );

        UpdatePlayerShield(
            player.Shield,
            player.MaxShield
        );

        UpdateEnemyShield(
            enemy.Shield,
            enemy.MaxShield
        );

        RefreshUI();
    }

    private void OnTurnReset()
    {
        Debug.Log(
            "BattleUIManager：次のターンのUIをリセット"
        );

        // 選択中の行動を解除
        selectedActionIndex = -1;

        // UI更新
        RefreshUI();

        // 行動ボタンを有効化
        SetActionButtonsInteractable(true);

        // スキルパネルを閉じる
        if (skillUIManager != null)
        {
            skillUIManager.CloseSkillPanel();
        }
    }

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnActionsReset -= OnTurnReset;
        }

        if (BattleManager.Instance != null)
        {
            BattleUnit player =
                BattleManager.Instance.Player;

            BattleUnit enemy =
                BattleManager.Instance.Enemy;

            if (player != null)
            {
                player.OnHPChanged -= UpdatePlayerHP;
            }

            if (enemy != null)
            {
                enemy.OnHPChanged -= UpdateEnemyHP;
            }

            player.OnShieldChanged -= UpdatePlayerShield;
            enemy.OnShieldChanged -= UpdateEnemyShield;
        }
    }

    private void OnAttackButton()
    {
        SelectAction(ActionType.Attack);
    }

    private void OnDefendButton()
    {
        SelectAction(ActionType.Defend);
    }

    private void OnSkillButton()
    {
        skillUIManager.OpenSkillPanel();
    }

    private void OnClearButton()
    {
        if (turnManager == null)
        {
            return;
        }

        // 実行中はクリアできない
        if (turnManager.IsExecuting)
        {
            return;
        }

        turnManager.ClearPlayerActions();

        RefreshUI();

        Debug.Log("プレイヤーの行動をすべてクリアしました。");
    }

    private void SelectAction(ActionType actionType)
    {
        // すでに選択されている行動がある場合
        if (selectedActionIndex >= 0)
        {
            if (turnManager.ReplacePlayerAction(
                selectedActionIndex,
                actionType))
            {
                selectedActionIndex = -1;
                RefreshUI();
            }

            return;
        }

        // 通常の新規予約
        if (turnManager.AddPlayerAction(actionType))
        {
            RefreshUI();
        }
    }

    private void OnExecuteButton()
    {
        turnManager.ExecuteTurn();

        SetActionButtonsInteractable(false);
        executeButton.interactable = false;
    }

    public void RefreshUI()
    {
        turnText.text =
            "TURN " +
            turnManager.CurrentTurn;

        int count =
            turnManager.CurrentActionIndex;

        int actionsPerTurn =
            turnManager.GetActionsPerTurn();

        if(action4Text != null)
        {
            action4Text.gameObject.SetActive(actionsPerTurn >= 4);
        }

        actionCountText.text =
            count +
            " / " +
            actionsPerTurn;

        UpdateActionText();

        UpdateButtons();
    }

    private void UpdateActionText()
    {
        var actions =
            turnManager.GetPlayerActions();

        if (actions.Count > 0)
        {
            action1Text.text =
                "① " +
                GetActionName(actions[0]) +
                "\nSPEED " +
                actions[0].speed;
        }
        else
        {
            action1Text.text = "① ---";
        }

        if (actions.Count > 1)
        {
            action2Text.text =
                "② " +
                GetActionName(actions[1]) +
                "\nSPEED " +
                actions[1].speed;
        }
        else
        {
            action2Text.text = "② ---";
        }

        if (actions.Count > 2)
        {
            action3Text.text =
                "③ " +
                GetActionName(actions[2]) +
                "\nSPEED " +
                actions[2].speed;
        }
        else
        {
            action3Text.text = "③ ---";
        }

        // ④
        if (actions.Count > 3)
        {
            action4Text.text =
                "④ " +
                GetActionName(actions[3]) +
                "\nSPEED " +
                actions[3].speed;
        }
        else
        {
            action4Text.text = "④ ---";
        }
    }

    private string GetActionName(BattleAction action)
    {
        if (action == null)
        {
            return "---";
        }

        switch (action.actionType)
        {
            case ActionType.Attack:
                return "攻撃";

            case ActionType.Defend:
                return "防御";

            case ActionType.Skill:

                if (action.skillData != null)
                {
                    return action.skillData.skillName;
                }

                return "スキル";

            default:
                return "---";
        }
    }

    private void UpdateButtons()
    {
        int actionsPerTurn =
            turnManager.GetActionsPerTurn();

        bool canSelect =
            turnManager.CurrentActionIndex <
            actionsPerTurn;

        attackButton.interactable =
            canSelect;

        defendButton.interactable =
            canSelect;

        skillButton.interactable =
            canSelect;

        executeButton.interactable =
            turnManager.CurrentActionIndex ==
            actionsPerTurn;
    }

    private void SetActionButtonsInteractable(bool value)
    {
        attackButton.interactable = value;
        defendButton.interactable = value;
        skillButton.interactable = value;
    }

    private void UpdatePlayerHP(int currentHP, int maxHP)
    {
        playerHPBar.SetHP(currentHP, maxHP);
    }

    private void UpdateEnemyHP(int currentHP, int maxHP)
    {
        enemyHPBar.SetHP(currentHP, maxHP);
    }

    private void UpdatePlayerShield(int shield, int maxShield)
    {
        if (playerShieldText == null)
        {
            return;
        }

        playerShieldText.text =
            shield.ToString();
    }

    private void UpdateEnemyShield(int shield, int maxShield)
    {
        if (enemyShieldText == null)
        {
            return;
        }

        enemyShieldText.text =
            shield.ToString();
    }
}