using System;
using UnityEngine;
using System.Collections.Generic;

public class BattleUnit : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private CharacterData characterData;

    private int currentHP;

    private bool isDead = false;
    [SerializeField] private bool isPlayer;

    // ========================================
    // 現在のBuff / Debuff
    // ========================================

    private int speedModifier = 0;
    private int attackModifier = 0;

    private int speedEffectTurns = 0;
    private int attackEffectTurns = 0;


    public CharacterData CharacterData =>
        characterData;

    public string UnitName =>
        characterData.characterName;

    public int CurrentHP =>
        currentHP;

    private List<StatusEffect> statusEffects =
        new List<StatusEffect>();

    public int MaxHP
    {
        get
        {
            int bonus = 0;

            if (isPlayer &&
                RoguelikeManager.Instance != null)
            {
                bonus =
                    RoguelikeManager.Instance
                    .PlayerData.maxHPBonus;
            }

            return characterData.maxHP + bonus;
        }
    }

    public int AttackPower
    {
        get
        {
            int runBonus = 0;

            if (isPlayer &&
                RoguelikeManager.Instance != null)
            {
                runBonus =
                    RoguelikeManager.Instance
                    .PlayerData.attackBonus;
            }

            return Mathf.Max(
                0,
                characterData.attackPower +
                runBonus +
                attackModifier
            );
        }
    }

    public int NormalAttackSpeed
    {
        get
        {
            int runBonus = 0;

            if(isPlayer &&
                RoguelikeManager.Instance != null)
            {
                runBonus =
                    RoguelikeManager.Instance
                    .PlayerData.speedBonus;
            }

            return Mathf.Max(
                0,
                characterData.normalAttackSpeed +
                runBonus +
                speedModifier
            );
        }
    }


    public int SpeedModifier =>
        speedModifier;

    public int AttackModifier =>
        attackModifier;

    public int SpeedEffectTurns =>
        speedEffectTurns;

    public int AttackEffectTurns =>
        attackEffectTurns;


    public event Action<int, int> OnHPChanged;


    // ========================================
    // 初期化
    // ========================================
    private void Awake()
    {
        // プレイヤーだけ選択したキャラクターを使用
        if (isPlayer)
        {
            if (RoguelikeManager.Instance != null)
            {
                characterData =
                    RoguelikeManager.Instance.PlayerCharacterData;
            }
        }

        // EnemyはInspectorで設定したCharacterDataをそのまま使用

        if (characterData == null)
        {
            Debug.LogError(
                gameObject.name +
                " にCharacterDataが設定されていません。"
            );

            return;
        }

        currentHP = characterData.maxHP;
    }


    private void Start()
    {
        if (characterData == null)
        {
            return;
        }

        LoadRunData();

        OnHPChanged?.Invoke(
            currentHP,
            MaxHP
        );
    }

    private void LoadRunData()
    {
        if (!isPlayer)
            return;

        if (RoguelikeManager.Instance == null)
            return;

        PlayerRunData data =
            RoguelikeManager.Instance.PlayerData;

        // 現在HPを引き継ぐ
        currentHP = Mathf.Clamp(
            data.currentHP,
            0,
            MaxHP
        );
    }

    // ========================================
    // ダメージ
    // ========================================

    public void TakeDamage(int damage)
    {
        if(IsDead)
        {
            return;
        }

        damage =
            Mathf.Max(
                0,
                damage
            );

        currentHP -= damage;

        currentHP =
            Mathf.Max(
                0,
                currentHP
            );

        Debug.Log(
            UnitName +
            " が " +
            damage +
            " ダメージを受けた！" +
            " HP：" +
            currentHP +
            "/" +
            MaxHP
        );

        OnHPChanged?.Invoke(
            currentHP,
            MaxHP
        );

        if (isPlayer &&
            RoguelikeManager.Instance != null)
        {
            RoguelikeManager.Instance.PlayerData.currentHP =
                currentHP;
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ========================================
    // 回復
    // ========================================

    public void Heal(int amount)
    {
        amount =
            Mathf.Max(
                0,
                amount
            );

        currentHP += amount;

        currentHP =
            Mathf.Min(
                currentHP,
                MaxHP
            );

        Debug.Log(
            UnitName +
            " が " +
            amount +
            " 回復した！" +
            " HP：" +
            currentHP +
            "/" +
            MaxHP
        );

        OnHPChanged?.Invoke(
            currentHP,
            MaxHP
        );

        if (isPlayer &&
            RoguelikeManager.Instance != null)
        {
            RoguelikeManager.Instance.PlayerData.currentHP =
                currentHP;
        }
    }


    // ========================================
    // Speed Buff / Debuff
    // ========================================

    public void ChangeSpeed(
        int amount,
        int duration)
    {
        speedModifier += amount;

        speedEffectTurns =
            Mathf.Max(
                speedEffectTurns,
                duration
            );

        Debug.Log(
            UnitName +
            " Speed " +
            (amount >= 0 ? "+" : "") +
            amount +
            " / " +
            duration +
            "ターン"
        );
    }


    // ========================================
    // Attack Buff / Debuff
    // ========================================

    public void ChangeAttack(
        int amount,
        int duration)
    {
        attackModifier += amount;

        attackEffectTurns =
            Mathf.Max(
                attackEffectTurns,
                duration
            );

        Debug.Log(
            UnitName +
            " Attack " +
            (amount >= 0 ? "+" : "") +
            amount +
            " / " +
            duration +
            "ターン"
        );
    }

    public bool HasCounter()
    {
        return HasStatusEffect(
            StatusEffectType.Counter
        );
    }


    public void ConsumeCounter()
    {
        RemoveStatusEffect(
            StatusEffectType.Counter
        );

        Debug.Log(
            UnitName +
            " のカウンターが発動して解除された！"
        );
    }

    // ========================================
    // ターン終了処理
    // ========================================

    public void EndTurnEffects()
    {
        // ========================================
        // Buff / Debuffの残りターン
        // ========================================

        if (speedEffectTurns > 0)
        {
            speedEffectTurns--;

            if (speedEffectTurns <= 0)
            {
                speedModifier = 0;

                Debug.Log(
                    UnitName +
                    " のSpeed効果が終了しました。"
                );
            }
        }


        if (attackEffectTurns > 0)
        {
            attackEffectTurns--;

            if (attackEffectTurns <= 0)
            {
                attackModifier = 0;

                Debug.Log(
                    UnitName +
                    " のAttack効果が終了しました。"
                );
            }
        }


        // ========================================
        // 状態異常ダメージ
        // ========================================

        ProcessStatusEffects();
    }


    // ========================================
    // Buff / Debuff解除
    // ========================================

    public void ClearEffects()
    {
        speedModifier = 0;
        attackModifier = 0;

        speedEffectTurns = 0;
        attackEffectTurns = 0;
    }


    public bool IsDead
    {
        get { return isDead; }
    }


    // ========================================
    // 死亡
    // ========================================
    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Debug.Log(
            "================================"
        );

        Debug.Log(
            UnitName +
            " は倒れた！"
        );

        Debug.Log(
            "================================"
        );
    }

    public void AddStatusEffect(
    StatusEffectType type,
    int power,
    int duration)
    {
        StatusEffect existing =
            statusEffects.Find(
                x => x.type == type
            );

        if (existing != null)
        {
            existing.power =
                Mathf.Max(
                    existing.power,
                    power
                );

            existing.remainingTurns =
                Mathf.Max(
                    existing.remainingTurns,
                    duration
                );
        }
        else
        {
            statusEffects.Add(
                new StatusEffect(
                    type,
                    power,
                    duration
                )
            );
        }

        Debug.Log(
            UnitName +
            " に " +
            type +
            " が付与された！"
        );
    }

    public bool HasStatusEffect(
    StatusEffectType type)
    {
        return statusEffects.Exists(
            x => x.type == type
        );
    }

    public StatusEffect GetStatusEffect(
    StatusEffectType type)
    {
        return statusEffects.Find(
            x => x.type == type
        );
    }

    private void ProcessStatusEffects()
    {
        for (int i = statusEffects.Count - 1;
             i >= 0;
             i--)
        {
            StatusEffect effect =
                statusEffects[i];

            switch (effect.type)
            {
                case StatusEffectType.Burn:

                    TakeDamage(
                        effect.power
                    );

                    Debug.Log(
                        UnitName +
                        " は燃焼で " +
                        effect.power +
                        " ダメージ！"
                    );

                    break;


                case StatusEffectType.Poison:

                    TakeDamage(
                        effect.power
                    );

                    Debug.Log(
                        UnitName +
                        " は毒で " +
                        effect.power +
                        " ダメージ！"
                    );

                    break;


                case StatusEffectType.Counter:

                    // カウンターは
                    // 攻撃を受けた時に処理する

                    break;
            }


            effect.remainingTurns--;


            if (effect.remainingTurns <= 0)
            {
                Debug.Log(
                    UnitName +
                    " の " +
                    effect.type +
                    " が解除された！"
                );

                statusEffects.RemoveAt(i);
            }
        }
    }

    // 行動速度
    public int GetActionSpeed(
    ActionType type,
    SkillData skill = null)
    {
        int runSpeedBonus = 0;

        if (isPlayer &&
            RoguelikeManager.Instance != null)
        {
            runSpeedBonus =
                RoguelikeManager.Instance.PlayerData.speedBonus;
        }

        int baseSpeed = 0;

        switch (type)
        {
            case ActionType.Attack:
                baseSpeed = characterData.normalAttackSpeed;
                break;

            case ActionType.Defend:
                baseSpeed = 8;
                break;

            case ActionType.Skill:
                if (skill == null)
                    return 0;

                baseSpeed = skill.speed;
                break;

            case ActionType.Heal:
                baseSpeed = 6;
                break;

            default:
                return 0;
        }

        int finalSpeed =
            baseSpeed +
            runSpeedBonus +
            speedModifier;

        Debug.Log(
            "【速度計算】" +
            " 行動=" + type +
            " 基礎=" + baseSpeed +
            " RoguelikeSpeed=" + runSpeedBonus +
            " Buff=" + speedModifier +
            " 最終=" + finalSpeed
        );

        return Mathf.Max(0, finalSpeed);
    }

    public int GetCounterDamage()
    {
        StatusEffect counter =
            GetStatusEffect(
                StatusEffectType.Counter
            );

        if (counter == null)
        {
            return 0;
        }

        return counter.power;
    }

    public void RemoveStatusEffect(
    StatusEffectType type)
    {
        statusEffects.RemoveAll(
            x => x.type == type
        );
    }
}