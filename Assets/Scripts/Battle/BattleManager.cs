using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Result UI")]
    [SerializeField] private BattleResultUI battleResultUI;

    [Header("Managers")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private EnemyAI enemyAI;

    [Header("Battle Settings")]
    [SerializeField] private bool isBossBattle = false;

    [Header("Units")]
    [SerializeField] private BattleUnit player;
    [SerializeField] private BattleUnit enemy;

    [Header("UI")]
    [SerializeField] private EnemyFutureUI enemyFutureUI;

    private bool battleEnded = false;

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
        Debug.Log(
            "===== START TURN " +
            turnManager.CurrentTurn +
            " ====="
        );

        // 新しいターンなので予約行動をリセット
        turnManager.StartTurn();

        // 敵の行動を再生成
        enemyAI.CreateActions();

        // 敵未来UI更新
        if (enemyFutureUI != null)
        {
            enemyFutureUI.ShowEnemyFuture();
        }
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

        CheckBattleResult();

        if (battleEnded)
        {
            return;
        }

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

            CheckBattleResult();
        }
    }

    // プレイヤースキル
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
                CheckBattleResult();

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

        CheckBattleResult();
    }

    // 敵回復
    public void EnemyHeal()
    {
        enemy.Heal(15);

        Debug.Log(
            "Enemy が15回復した！"
        );
    }

    // 敵スキル
    public void ExecuteEnemySkill(
        SkillData skill,
        bool playerDefending)
    {
        if (skill == null)
        {
            Debug.LogWarning(
                "Enemy SkillDataがありません。"
            );

            return;
        }

        Debug.Log(
            "Enemy：" +
            skill.skillName +
            " 発動！"
        );

        switch (skill.effectType)
        {
            case SkillEffectType.Damage:

                int damage = skill.power;

                if (playerDefending)
                {
                    damage /= 2;

                    Debug.Log(
                        "Playerは防御中！" +
                        "スキルダメージ半減！"
                    );
                }

                player.TakeDamage(damage);

                break;


            case SkillEffectType.Heal:

                enemy.Heal(skill.power);

                Debug.Log(
                    "Enemyが" +
                    skill.power +
                    "回復！"
                );

                break;


            case SkillEffectType.Slow:

                player.ChangeSpeed(
                    -skill.power,
                    skill.duration
                );

                Debug.Log(
                    "PlayerのSpeedが " +
                    skill.power +
                    " 下がった！"
                );

                break;


            case SkillEffectType.SpeedUp:

                enemy.ChangeSpeed(
                    skill.power,
                    skill.duration
                );

                Debug.Log(
                    "EnemyのSpeedが " +
                    skill.power +
                    " 上がった！"
                );

                break;


            case SkillEffectType.AttackUp:

                enemy.ChangeAttack(
                    skill.power,
                    skill.duration
                );

                Debug.Log(
                    "Enemyの攻撃力が " +
                    skill.power +
                    " 上がった！"
                );

                break;


            case SkillEffectType.AttackDown:

                player.ChangeAttack(
                    -skill.power,
                    skill.duration
                );

                Debug.Log(
                    "Playerの攻撃力が " +
                    skill.power +
                    " 下がった！"
                );

                break;


            case SkillEffectType.DefenseDown:

                Debug.Log(
                    "Playerの防御力を低下！"
                );

                break;


            case SkillEffectType.Counter:

                enemy.AddStatusEffect(
                    StatusEffectType.Counter,
                    skill.power,
                    skill.duration
                );

                Debug.Log(
                    "Enemyはカウンターを構えた！"
                );

                break;


            case SkillEffectType.Burn:

                player.TakeDamage(
                    skill.power
                );

                player.AddStatusEffect(
                    StatusEffectType.Burn,
                    skill.power / 2,
                    skill.duration
                );

                Debug.Log(
                    "Playerに火傷を付与！"
                );

                break;


            case SkillEffectType.Poison:

                player.AddStatusEffect(
                    StatusEffectType.Poison,
                    skill.power,
                    skill.duration
                );

                Debug.Log(
                    "Playerに毒を付与！"
                );

                break;


            case SkillEffectType.Rewrite:

                Debug.Log(
                    "===== BOSS REWRITE ====="
                );

                break;


            case SkillEffectType.None:

                Debug.Log(
                    "Enemy Skill：効果なし"
                );

                break;
        }

        CheckBattleResult();
    }

    // 敵行動
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
                    "Enemy：スキル！ " +
                    action.skillData.skillName +
                    " / SPEED " +
                    action.speed
                );

                ExecuteEnemySkill(
                    action.skillData,
                    playerDefending
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

    // 勝利判定
    private void CheckBattleResult()
    {
        if (battleEnded)
        {
            return;
        }

        if (Player.IsDead)
        {
            battleEnded = true;

            Debug.Log("===== PLAYER LOSE =====");

            TurnManager.StopBattle();

            if (battleResultUI != null)
            {
                battleResultUI.ShowDefeat();
            }

            return;
        }

        if (Enemy.IsDead)
        {
            battleEnded = true;

            Debug.Log("===== PLAYER WIN =====");

            TurnManager.StopBattle();

            if (RoguelikeManager.Instance != null)
            {
                if (isBossBattle)
                {
                    // ボス撃破
                    Debug.Log("===== BOSS DEFEATED =====");

                    RoguelikeManager.Instance.GameClear();
                }
                else
                {
                    // 通常戦闘クリア
                    RoguelikeManager.Instance.NormalBattleClear(
                        turnManager.CurrentTurn
                    );
                }
            }


            // 勝利UI表示
            if (battleResultUI != null)
            {
                battleResultUI.ShowVictory();
            }

            return;
        }
    }
}