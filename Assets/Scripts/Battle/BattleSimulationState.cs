using System;
using System.Collections.Generic;

[Serializable]
public class BattleSimulationState
{
    // ==============================
    // Player
    // ==============================

    public int playerHP;
    public int playerMaxHP;

    public int playerAttack;
    public int playerSpeedModifier;

    public int playerSpeedBuffTurns;
    public int playerAttackBuffTurns;


    // ==============================
    // Enemy
    // ==============================

    public int enemyHP;
    public int enemyMaxHP;

    public int enemyAttack;
    public int enemySpeedModifier;

    public int enemySpeedDebuffTurns;
    public int enemyAttackDebuffTurns;


    // ==============================
    // 状態異常
    // ==============================

    public List<StatusEffect> playerStatusEffects =
        new List<StatusEffect>();

    public List<StatusEffect> enemyStatusEffects =
        new List<StatusEffect>();


    // ==============================
    // 防御
    // ==============================

    public bool playerDefending;
    public bool enemyDefending;


    // ==============================
    // コンストラクタ
    // ==============================

    public BattleSimulationState(
        BattleUnit player,
        BattleUnit enemy)
    {
        playerHP = player.CurrentHP;
        playerMaxHP = player.MaxHP;

        playerAttack = player.AttackPower;

        playerSpeedModifier = 0;

        playerSpeedBuffTurns = 0;
        playerAttackBuffTurns = 0;


        enemyHP = enemy.CurrentHP;
        enemyMaxHP = enemy.MaxHP;

        enemyAttack = enemy.AttackPower;

        enemySpeedModifier = 0;

        enemySpeedDebuffTurns = 0;
        enemyAttackDebuffTurns = 0;


        playerDefending = false;
        enemyDefending = false;
    }


    // ==============================
    // コピー
    // ==============================

    public BattleSimulationState Clone()
    {
        BattleSimulationState copy =
            new BattleSimulationState();


        copy.playerHP =
            playerHP;

        copy.playerMaxHP =
            playerMaxHP;

        copy.playerAttack =
            playerAttack;

        copy.playerSpeedModifier =
            playerSpeedModifier;

        copy.playerSpeedBuffTurns =
            playerSpeedBuffTurns;

        copy.playerAttackBuffTurns =
            playerAttackBuffTurns;


        copy.enemyHP =
            enemyHP;

        copy.enemyMaxHP =
            enemyMaxHP;

        copy.enemyAttack =
            enemyAttack;

        copy.enemySpeedModifier =
            enemySpeedModifier;

        copy.enemySpeedDebuffTurns =
            enemySpeedDebuffTurns;

        copy.enemyAttackDebuffTurns =
            enemyAttackDebuffTurns;


        copy.playerDefending =
            playerDefending;

        copy.enemyDefending =
            enemyDefending;


        // ==========================
        // 状態異常をコピー
        // ==========================

        foreach (
            StatusEffect effect
            in playerStatusEffects)
        {
            copy.playerStatusEffects.Add(
                effect.Clone()
            );
        }


        foreach (
            StatusEffect effect
            in enemyStatusEffects)
        {
            copy.enemyStatusEffects.Add(
                effect.Clone()
            );
        }


        return copy;
    }


    private BattleSimulationState()
    {
    }
}