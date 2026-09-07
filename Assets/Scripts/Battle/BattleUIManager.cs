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

    [Header("HP")]
    [SerializeField] private HPBarUI playerHPBar;
    [SerializeField] private HPBarUI enemyHPBar;

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

        // 追加
        clearButton.onClick.AddListener(OnClearButton);

        BattleUnit player = BattleManager.Instance.Player;
        BattleUnit enemy = BattleManager.Instance.Enemy;

        player.OnHPChanged += UpdatePlayerHP;
        enemy.OnHPChanged += UpdateEnemyHP;

        UpdatePlayerHP(
            player.CurrentHP,
            player.MaxHP
        );

        UpdateEnemyHP(
            enemy.CurrentHP,
            enemy.MaxHP
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

        actionCountText.text =
            "行動 " +
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
                "① " +
                GetActionName(actions[0].actionType) +
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
                GetActionName(actions[1].actionType) +
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
                GetActionName(actions[2].actionType) +
                "\nSPEED " +
                actions[2].speed;
        }
        else
        {
            action3Text.text = "③ ---";
        }
    }

    private string GetActionName(ActionType type)
    {
        switch (type)
        {
            case ActionType.Attack:
                return "攻撃";

            case ActionType.Defend:
                return "防御";

            case ActionType.Skill:
                return "スキル";

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