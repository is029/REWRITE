using System;
using UnityEngine;
using System.Collections.Generic;

public class BattleUnit : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private CharacterData characterData;

    [Header("アニメーション")]
    private Animator animator;

    private int currentHP;

    private bool isDead = false;
    [SerializeField] private bool isPlayer;

    [SerializeField] private int defendShield = 30;

    // シールド最大値
    [SerializeField] private int maxShield = 100;

    public int Shield { get; private set; }

    public int MaxShield => maxShield;

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

    // ========================================
    // スキルクールタイム
    // ========================================

    private Dictionary<SkillData, int> skillCooldowns =
        new Dictionary<SkillData, int>();

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
    public event Action<int, int> OnShieldChanged;

    // ========================================
    // 初期化
    // ========================================
    private void Awake()
    {
        animator = GetComponent<Animator>();

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

        ApplyAnimatorController();

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
        // アニメーション
        PlayHitAnimation();

        // まずシールドで受ける
        if (Shield > 0)
        {
            int shieldDamage = Mathf.Min(Shield, damage);

            Shield -= shieldDamage;
            damage -= shieldDamage;

            Debug.Log(
                UnitName +
                " のシールドが " +
                shieldDamage +
                " 減った！"
            );
        }

        // 残ったダメージをHPへ
        if (damage > 0)
        {
            currentHP -= damage;
        }

        currentHP = Mathf.Max(currentHP, 0);

        OnHPChanged?.Invoke(currentHP, MaxHP);
        OnShieldChanged?.Invoke(Shield, MaxShield);

        CheckDead();
    }

    private void CheckDead()
    {
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Defend()
    {
        Shield += defendShield;

        Shield = Mathf.Min(Shield, MaxShield);

        Debug.Log(
            UnitName +
            " は防御！ シールド +" +
            defendShield +
            " / Shield：" +
            Shield +
            "/" +
            MaxShield
        );

        OnShieldChanged?.Invoke(
            Shield,
            MaxShield
        );
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

        // ========================================
        // スキルクールタイム
        // ========================================
        ReduceSkillCooldowns();
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

        // アニメーション
        PlayDeathAnimation();

        // ========================================
        // BattleManagerに死亡を通知
        // ========================================

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.CheckBattleResultFromUnit();
        }
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

    // ========================================
    // スキルクールタイム
    // ========================================

    public bool IsSkillAvailable(SkillData skill)
    {
        if (skill == null)
            return false;

        if (!skillCooldowns.ContainsKey(skill))
            return true;

        return skillCooldowns[skill] <= 0;
    }

    public int GetSkillCooldown(SkillData skill)
    {
        if (skill == null)
            return 0;

        if (!skillCooldowns.ContainsKey(skill))
            return 0;

        return skillCooldowns[skill];
    }

    public void StartSkillCooldown(SkillData skill)
    {
        if (skill == null)
            return;

        if (skill.cooldown <= 0)
            return;

        skillCooldowns[skill] = skill.cooldown;

        Debug.Log(
            UnitName +
            " のスキル「" +
            skill.skillName +
            "」クールタイム開始：" +
            skill.cooldown
        );
    }

    private void ReduceSkillCooldowns()
    {
        List<SkillData> skills =
            new List<SkillData>(skillCooldowns.Keys);

        foreach (SkillData skill in skills)
        {
            skillCooldowns[skill]--;

            if (skillCooldowns[skill] <= 0)
            {
                skillCooldowns[skill] = 0;

                Debug.Log(
                    UnitName +
                    " のスキル「" +
                    skill.skillName +
                    "」が使用可能になりました。"
                );
            }
        }
    }

    public void SetCharacterData(CharacterData data)
    {
        characterData = data;

        currentHP = characterData.maxHP;
        isDead = false;

        // キャラクターデータからキャラクター固有のAnimatorControllerを設定
        if(animator != null && characterData.animatorController != null)
        {
            // キャラクター固有のAnimator Controllerを設定
            ApplyAnimatorController();
        }

        OnHPChanged?.Invoke(
            currentHP,
            characterData.maxHP
        );
    }

    private void ApplyAnimatorController()
    {
        if (animator == null)
        {
            Debug.LogWarning(
                UnitName +
                " にAnimatorがありません。"
            );

            return;
        }

        if (characterData == null)
        {
            Debug.LogWarning(
                "CharacterDataがありません。"
            );

            return;
        }

        if (characterData.animatorController == null)
        {
            Debug.LogWarning(
                UnitName +
                " のAnimator ControllerがCharacterDataに設定されていません。"
            );

            return;
        }

        animator.runtimeAnimatorController =
            characterData.animatorController;

        Debug.Log(
            "===== ANIMATOR CONTROLLER SET =====\n" +
            "Character : " +
            characterData.characterName +
            "\nController : " +
            characterData.animatorController.name
        );
    }

    // ========================================
    // アニメーション関係
    // ========================================
    public void PlayAttackAnimation()
    {
        if (animator == null)
            return;

        animator.SetTrigger("Attack");
    }

    public void PlayDefendAnimation()
    {
        if (animator == null)
            return;

        animator.SetTrigger("Defend");
    }

    public void PlayHealAnimation()
    {
        if (animator == null)
            return;

        animator.SetTrigger("Heal");
    }

    public void PlaySkillAnimation(SkillData skill)
    {
        if (animator == null || skill == null)
            return;

        if (string.IsNullOrEmpty(skill.animationTrigger))
            return;

        animator.SetTrigger(skill.animationTrigger);
    }

    public void PlayHitAnimation()
    {
        if (animator == null)
            return;

        animator.SetTrigger("Hit");
    }

    public void PlayDeathAnimation()
    {
        if (animator == null)
            return;

        animator.SetTrigger("Death");
    }
}