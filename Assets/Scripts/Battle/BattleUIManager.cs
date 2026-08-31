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

    [Header("Reserved Actions")]
    [SerializeField] private TMP_Text action1Text;
    [SerializeField] private TMP_Text action2Text;
    [SerializeField] private TMP_Text action3Text;

    [Header("HP")]
    [SerializeField] private HPBarUI playerHPBar;
    [SerializeField] private HPBarUI enemyHPBar;

    [SerializeField]
    private SkillUIManager skillUIManager;

    private TurnManager turnManager;

    private void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();

        attackButton.onClick.AddListener(OnAttackButton);
        defendButton.onClick.AddListener(OnDefendButton);
        skillButton.onClick.AddListener(OnSkillButton);
        executeButton.onClick.AddListener(OnExecuteButton);

        BattleUnit player = BattleManager.Instance.Player;
        BattleUnit enemy = BattleManager.Instance.Enemy;

        player.OnHPChanged += UpdatePlayerHP;
        enemy.OnHPChanged += UpdateEnemyHP;

        UpdatePlayerHP(player.CurrentHP, player.MaxHP);
        UpdateEnemyHP(enemy.CurrentHP, enemy.MaxHP);

        RefreshUI();
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

    private void SelectAction(ActionType actionType)
    {
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

        actionCountText.text =
            "çsìÆ " +
            count +
            " / 3";

        UpdateActionText();

        UpdateButtons();
    }

    private void UpdateActionText()
    {
        var actions = turnManager.GetPlayerActions();

        if (actions.Count > 0)
        {
            action1Text.text =
                "á@ " +
                GetActionName(actions[0].actionType) +
                "\nSPEED " +
                actions[0].speed;
        }
        else
        {
            action1Text.text = "á@ ---";
        }

        if (actions.Count > 1)
        {
            action2Text.text =
                "áA " +
                GetActionName(actions[1].actionType) +
                "\nSPEED " +
                actions[1].speed;
        }
        else
        {
            action2Text.text = "áA ---";
        }

        if (actions.Count > 2)
        {
            action3Text.text =
                "áB " +
                GetActionName(actions[2].actionType) +
                "\nSPEED " +
                actions[2].speed;
        }
        else
        {
            action3Text.text = "áB ---";
        }
    }

    private string GetActionName(ActionType type)
    {
        switch (type)
        {
            case ActionType.Attack:
                return "çUåÇ";

            case ActionType.Defend:
                return "ñhå‰";

            case ActionType.Skill:
                return "ÉXÉLÉã";

            default:
                return "---";
        }
    }

    private void UpdateButtons()
    {
        bool canSelect = turnManager.CurrentActionIndex < 3;

        attackButton.interactable = canSelect;
        defendButton.interactable = canSelect;
        skillButton.interactable = canSelect;

        executeButton.interactable =
            turnManager.CurrentActionIndex == 3;
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
}