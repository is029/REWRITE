using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private EnemyAI enemyAI;

    [Header("Units")]
    [SerializeField] private BattleUnit player;
    [SerializeField] private BattleUnit enemy;

    [Header("UI")]
    [SerializeField] private EnemyFutureUI enemyFutureUI;

    public EnemyFutureUI EnemyFutureUI => enemyFutureUI;
    public TurnManager TurnManager => turnManager;
    public BattleUnit Player => player;
    public BattleUnit Enemy => enemy;

    public EnemyAI EnemyAI => enemyAI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartTurn();
    }

    public void StartTurn()
    {
        enemyAI.CreateActions();

        enemyFutureUI.ShowEnemyFuture();

        turnManager.StartTurn();

        Debug.Log(
            "===== TURN " +
            turnManager.CurrentTurn +
            " START ====="
        );
    }

    public void SelectAction(ActionType actionType)
    {
        turnManager.AddPlayerAction(actionType);
    }

    public void StartBattleTurn()
    {
        turnManager.ExecuteTurn();
    }

    // プレイヤー攻撃
    public void PlayerAttack(bool enemyDefending)
    {
        int damage = player.AttackPower;

        if (enemyDefending)
        {
            damage /= 2;

            Debug.Log(
                "Enemyは防御中！" +
                "ダメージ半減！"
            );
        }

        enemy.TakeDamage(damage);

        if (enemy.HasCounter())
        {
            int counterDamage =
                enemy.GetCounterDamage();

            Debug.Log(
                enemy.UnitName +
                " のカウンター発動！"
            );

            player.TakeDamage(
                counterDamage
            );

            enemy.ConsumeCounter();
        }
    }

    public void ExecutePlayerSkill(
    SkillData skill,
    bool enemyDefending)
    {
        if (skill == null)
        {
            Debug.LogWarning(
                "SkillDataがありません。"
            );

            return;
        }

        Debug.Log(
            "Player：" +
            skill.skillName +
            " 発動！"
        );

        switch (skill.effectType)
        {
            case SkillEffectType.Damage:

                int damage = skill.power;

                if (enemyDefending)
                {
                    damage /= 2;

                    Debug.Log(
                        "Enemyは防御中！" +
                        "スキルダメージ半減！"
                    );
                }

                enemy.TakeDamage(damage);

                break;


            case SkillEffectType.Heal:

                player.Heal(skill.power);

                break;


            case SkillEffectType.Slow:

                enemy.ChangeSpeed(
                    -skill.power,
                    skill.duration
                );

                Debug.Log(
                    "EnemyのSpeedが " +
                    skill.power +
                    " 下がった！"
                );

                break;


            case SkillEffectType.AttackDown:

                enemy.ChangeAttack(
                    -skill.power,
                    skill.duration
                );

                Debug.Log(
                    "Enemyの攻撃力が " +
                    skill.power +
                    " 下がった！"
                );

                break;


            case SkillEffectType.DefenseDown:

                Debug.Log(
                    "Enemyの防御力を低下！"
                );

                break;


            case SkillEffectType.SpeedUp:

                player.ChangeSpeed(
                    skill.power,
                    skill.duration
                );

                Debug.Log(
                    "PlayerのSpeedが " +
                    skill.power +
                    " 上がった！"
                );

                break;


            case SkillEffectType.AttackUp:

                player.ChangeAttack(
                    skill.power,
                    skill.duration
                );

                Debug.Log(
                    "Playerの攻撃力が " +
                    skill.power +
                    " 上がった！"
                );

                break;

            case SkillEffectType.Counter:

                player.AddStatusEffect(
                    StatusEffectType.Counter,
                    skill.power,
                    skill.duration
                );

                Debug.Log(
                    "Playerはカウンターを構えた！"
                );

                break;


            case SkillEffectType.Rewrite:

                Debug.Log(
                    "未来を書き換える！"
                );

                break;

            case SkillEffectType.Burn:

                enemy.TakeDamage(
                    skill.power
                );

                enemy.AddStatusEffect(
                    StatusEffectType.Burn,
                    skill.power / 2,
                    skill.duration
                );

                break;

            case SkillEffectType.Poison:

                enemy.AddStatusEffect(
                    StatusEffectType.Poison,
                    skill.power,
                    skill.duration
                );

                break;
        }
    }

    // 敵攻撃
    public void EnemyAttack(bool playerDefending)
    {
        int damage = enemy.AttackPower;

        if (playerDefending)
        {
            damage /= 2;

            Debug.Log(
                "Playerは防御中！" +
                "ダメージ半減！"
            );
        }

        player.TakeDamage(damage);
    }

    public void EnemyHeal()
    {
        enemy.Heal(15);

        Debug.Log(
            "Enemy が15回復した！"
        );
    }

    public void ExecuteEnemyAction(
        BattleAction action,
        bool playerDefending)
    {
        switch (action.actionType)
        {
            case ActionType.Attack:

                Debug.Log(
                    "Enemy：攻撃！ SPEED " +
                    action.speed
                );

                EnemyAttack(playerDefending);

                break;

            case ActionType.Defend:

                Debug.Log(
                    "Enemy：防御！ SPEED " +
                    action.speed
                );

                break;

            case ActionType.Skill:

                Debug.Log(
                    "Enemy：スキル！ SPEED " +
                    action.speed
                );

                break;

            case ActionType.Heal:

                Debug.Log(
                    "Enemy：回復！ SPEED " +
                    action.speed
                );

                EnemyHeal();

                break;
        }
    }
}