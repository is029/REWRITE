using System.Collections.Generic;
using UnityEngine;

public class BattleSimulator : MonoBehaviour
{
    public static BattleSimulator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // ========================================
    // ‰Šúó‘Ô‚ğì¬
    // ========================================

    public BattleSimulationState CreateInitialState()
    {
        return new BattleSimulationState(
            BattleManager.Instance.Player,
            BattleManager.Instance.Enemy
        );
    }


    // ========================================
    // 3s“®•ª‚ğ–¢—ˆƒVƒ~ƒ…ƒŒ[ƒVƒ‡ƒ“
    // ========================================

    public List<BattleSimulationState> SimulateTurn(
        List<BattleAction> playerActions,
        List<BattleAction> enemyActions)
    {
        List<BattleSimulationState> states =
            new List<BattleSimulationState>();

        int actionPerTurn = TurnManager.Instance.GetActionsPerTurn();
        if (enemyActions == null ||
            enemyActions.Count < actionPerTurn)
        {
            return states;
        }

        BattleSimulationState currentState =
            CreateInitialState();


        // ‡@ ¨ ‡A ¨ ‡B
        for (int i = 0; i < actionPerTurn; i++)
        {
            BattleAction playerAction = null;

            if (playerActions != null &&
                playerActions.Count > i)
            {
                playerAction = playerActions[i];
            }

            BattleAction enemyAction =
                enemyActions[i];


            // =================================
            // ‚·‚Å‚É‚Ç‚¿‚ç‚©‚ª€–S‚µ‚Ä‚¢‚é
            // =================================

            if (currentState.playerHP <= 0 ||
                currentState.enemyHP <= 0)
            {
                states.Add(
                    currentState.Clone()
                );

                continue;
            }


            // =================================
            // ‚±‚Ì”Ô†‚Ì–hŒäó‘Ô
            // =================================

            currentState.playerDefending =
                playerAction != null &&
                playerAction.actionType ==
                ActionType.Defend;

            currentState.enemyDefending =
                enemyAction != null &&
                enemyAction.actionType ==
                ActionType.Defend;


            // =================================
            // SpeedŒvZ
            // =================================

            int playerSpeed =
                GetPlayerSpeed(
                    currentState,
                    playerAction
                );

            int enemySpeed =
                GetEnemySpeed(
                    currentState,
                    enemyAction
                );


            Debug.Log(
                "[SIMULATION] ACTION " +
                (i + 1)
            );

            Debug.Log(
                "Player Speed : " +
                playerSpeed
            );

            Debug.Log(
                "Enemy Speed : " +
                enemySpeed
            );


            // =================================
            // Speed‡‚És“®
            // =================================

            if (playerSpeed > enemySpeed)
            {
                SimulatePlayerAction(
                    currentState,
                    playerAction
                );


                // Player‚ÌUŒ‚‚ÅEnemy‚ª€–S
                if (currentState.enemyHP <= 0)
                {
                    currentState.enemyHP = 0;

                    states.Add(
                        currentState.Clone()
                    );

                    continue;
                }


                SimulateEnemyAction(
                    currentState,
                    enemyAction
                );
            }
            else
            {
                SimulateEnemyAction(
                    currentState,
                    enemyAction
                );


                // Enemy‚ÌUŒ‚‚ÅPlayer‚ª€–S
                if (currentState.playerHP <= 0)
                {
                    currentState.playerHP = 0;

                    states.Add(
                        currentState.Clone()
                    );

                    continue;
                }


                SimulatePlayerAction(
                    currentState,
                    playerAction
                );
            }


            // =================================
            // HP‚ğ0–¢–‚É‚µ‚È‚¢
            // =================================

            currentState.playerHP =
                Mathf.Max(
                    0,
                    currentState.playerHP
                );

            currentState.enemyHP =
                Mathf.Max(
                    0,
                    currentState.enemyHP
                );


            // =================================
            // ‚±‚Ì”Ô†I—¹“_‚Ìó‘Ô
            // =================================

            states.Add(
                currentState.Clone()
            );
        }

        ProcessSimulationStatusEffects(currentState);

        return states;
    }


    // ========================================
    // Player Speed
    // ========================================

    private int GetPlayerSpeed(
        BattleSimulationState state,
        BattleAction action)
    {
        if (action == null)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            action.speed +
            state.playerSpeedModifier
        );
    }


    // ========================================
    // Enemy Speed
    // ========================================

    private int GetEnemySpeed(
        BattleSimulationState state,
        BattleAction action)
    {
        if (action == null)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            action.speed +
            state.enemySpeedModifier
        );
    }


    // ========================================
    // Players“®
    // ========================================

    private void SimulatePlayerAction(
        BattleSimulationState state,
        BattleAction action)
    {
        if (action == null)
        {
            return;
        }


        switch (action.actionType)
        {
            case ActionType.Attack:

                int damage =
                    state.playerAttack;

                if (state.enemyDefending)
                {
                    damage /= 2;
                }

                state.enemyHP =
                    Mathf.Max(
                        0,
                        state.enemyHP - damage
                    );

                Debug.Log(
                    "[SIM] Player Attack " +
                    damage +
                    " damage"
                );

                break;


            case ActionType.Defend:

                break;


            case ActionType.Skill:

                SimulatePlayerSkill(
                    state,
                    action.skillData
                );

                break;
        }
    }


    // ========================================
    // Enemys“®
    // ========================================

    private void SimulateEnemyAction(
        BattleSimulationState state,
        BattleAction action)
    {
        if (action == null)
        {
            return;
        }


        switch (action.actionType)
        {
            case ActionType.Attack:

                int damage =
                    state.enemyAttack;

                if (state.playerDefending)
                {
                    damage /= 2;
                }

                state.playerHP =
                    Mathf.Max(
                        0,
                        state.playerHP - damage
                    );


                // ==============================
                // Counter
                // ==============================

                StatusEffect counter =
                    state.playerStatusEffects.Find(
                        x => x.type ==
                        StatusEffectType.Counter
                    );

                if (counter != null &&
                    state.playerHP > 0)
                {
                    state.enemyHP =
                        Mathf.Max(
                            0,
                            state.enemyHP -
                            counter.power
                        );

                    state.playerStatusEffects.Remove(
                        counter
                    );

                    Debug.Log(
                        "[SIM] Counter " +
                        counter.power
                    );
                }

                break;


            case ActionType.Defend:

                break;


            case ActionType.Skill:

                // “GƒXƒLƒ‹‚ÍŒã‚Å’Ç‰Á
                break;


            case ActionType.Heal:

                state.enemyHP =
                    Mathf.Min(
                        state.enemyMaxHP,
                        state.enemyHP + 15
                    );

                Debug.Log(
                    "[SIM] Enemy Heal"
                );

                break;
        }
    }


    // ========================================
    // Player Skill
    // ========================================
    private void SimulatePlayerSkill(
        BattleSimulationState state,
        SkillData skill)
    {
        if (skill == null)
        {
            return;
        }

        Debug.Log(
            "[SIM] Player Skill : " +
            skill.skillName
        );


        switch (skill.effectType)
        {
            // ==========================
            // ƒ_ƒ[ƒW
            // ==========================

            case SkillEffectType.Damage:

                int damage =
                    skill.power;

                if (state.enemyDefending)
                {
                    damage /= 2;
                }

                state.enemyHP =
                    Mathf.Max(
                        0,
                        state.enemyHP - damage
                    );

                break;


            // ==========================
            // ‰ñ•œ
            // ==========================

            case SkillEffectType.Heal:

                state.playerHP =
                    Mathf.Min(
                        state.playerMaxHP,
                        state.playerHP +
                        skill.power
                    );

                break;


            // ==========================
            // Enemy Speed’á‰º
            // ==========================

            case SkillEffectType.Slow:

                state.enemySpeedModifier -=
                    skill.power;

                state.enemySpeedDebuffTurns =
                    Mathf.Max(
                        state.enemySpeedDebuffTurns,
                        skill.duration
                    );

                Debug.Log(
                    "[SIM] Enemy Slow +" +
                    skill.duration +
                    " turns"
                );

                break;


            // ==========================
            // Enemy UŒ‚—Í’á‰º
            // ==========================

            case SkillEffectType.AttackDown:

                state.enemyAttack -=
                    skill.power;

                state.enemyAttack =
                    Mathf.Max(
                        0,
                        state.enemyAttack
                    );

                state.enemyAttackDebuffTurns =
                    Mathf.Max(
                        state.enemyAttackDebuffTurns,
                        skill.duration
                    );

                Debug.Log(
                    "[SIM] Enemy AttackDown +" +
                    skill.duration +
                    " turns"
                );

                break;


            // ==========================
            // Player Speedã¸
            // ==========================

            case SkillEffectType.SpeedUp:

                state.playerSpeedModifier +=
                    skill.power;

                state.playerSpeedBuffTurns =
                    Mathf.Max(
                        state.playerSpeedBuffTurns,
                        skill.duration
                    );

                Debug.Log(
                    "[SIM] Player SpeedUp +" +
                    skill.duration +
                    " turns"
                );

                break;


            // ==========================
            // Player UŒ‚—Íã¸
            // ==========================

            case SkillEffectType.AttackUp:

                state.playerAttack +=
                    skill.power;

                state.playerAttackBuffTurns =
                    Mathf.Max(
                        state.playerAttackBuffTurns,
                        skill.duration
                    );

                Debug.Log(
                    "[SIM] Player AttackUp +" +
                    skill.duration +
                    " turns"
                );

                break;


            case SkillEffectType.DefenseDown:

                break;


            case SkillEffectType.Counter:

                AddPlayerStatus(
                    state,
                    StatusEffectType.Counter,
                    skill.power,
                    skill.duration
                );

                break;


            case SkillEffectType.Rewrite:

                break;

            case SkillEffectType.Burn:

                damage = skill.power;

                if (state.enemyDefending)
                {
                    damage /= 2;
                }

                state.enemyHP =
                    Mathf.Max(
                        0,
                        state.enemyHP - damage
                    );


                AddEnemyStatus(
                    state,
                    StatusEffectType.Burn,
                    Mathf.Max(
                        1,
                        skill.power / 2
                    ),
                    skill.duration
                );

                break;

            case SkillEffectType.Poison:

                AddEnemyStatus(
                    state,
                    StatusEffectType.Poison,
                    skill.power,
                    skill.duration
                );

                break;
        }
    }

    private void AddEnemyStatus(
    BattleSimulationState state,
    StatusEffectType type,
    int power,
    int duration)
    {
        StatusEffect existing =
            state.enemyStatusEffects.Find(
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

            return;
        }


        state.enemyStatusEffects.Add(
            new StatusEffect(
                type,
                power,
                duration
            )
        );
    }

    private void AddPlayerStatus(
    BattleSimulationState state,
    StatusEffectType type,
    int power,
    int duration)
    {
        StatusEffect existing =
            state.playerStatusEffects.Find(
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

            return;
        }


        state.playerStatusEffects.Add(
            new StatusEffect(
                type,
                power,
                duration
            )
        );
    }

    private void ProcessSimulationStatusEffects(
    BattleSimulationState state)
    {
        // =================================
        // Enemy
        // =================================

        for (
            int i = state.enemyStatusEffects.Count - 1;
            i >= 0;
            i--)
        {
            StatusEffect effect =
                state.enemyStatusEffects[i];


            switch (effect.type)
            {
                case StatusEffectType.Burn:

                    state.enemyHP =
                        Mathf.Max(
                            0,
                            state.enemyHP -
                            effect.power
                        );

                    Debug.Log(
                        "[SIM] Enemy Burn " +
                        effect.power
                    );

                    break;


                case StatusEffectType.Poison:

                    state.enemyHP =
                        Mathf.Max(
                            0,
                            state.enemyHP -
                            effect.power
                        );

                    Debug.Log(
                        "[SIM] Enemy Poison " +
                        effect.power
                    );

                    break;
            }


            effect.remainingTurns--;


            if (effect.remainingTurns <= 0)
            {
                state.enemyStatusEffects
                    .RemoveAt(i);
            }
        }


        // =================================
        // Player
        // =================================

        for (
            int i = state.playerStatusEffects.Count - 1;
            i >= 0;
            i--)
        {
            StatusEffect effect =
                state.playerStatusEffects[i];


            switch (effect.type)
            {
                case StatusEffectType.Burn:

                    state.playerHP =
                        Mathf.Max(
                            0,
                            state.playerHP -
                            effect.power
                        );

                    break;


                case StatusEffectType.Poison:

                    state.playerHP =
                        Mathf.Max(
                            0,
                            state.playerHP -
                            effect.power
                        );

                    break;
            }


            effect.remainingTurns--;


            if (effect.remainingTurns <= 0)
            {
                state.playerStatusEffects
                    .RemoveAt(i);
            }
        }
    }

    // ========================================
    // “G–¢—ˆSpeed‚¾‚¯æ“¾
    // ========================================

    public List<int> PredictEnemySpeeds(
        List<BattleAction> playerActions,
        List<BattleAction> enemyActions)
    {
        List<int> predictedSpeeds =
            new List<int>();

        int actionPerTurn = TurnManager.Instance.GetActionsPerTurn();

        if (enemyActions == null ||
            enemyActions.Count <
            actionPerTurn)
        {
            return predictedSpeeds;
        }


        BattleSimulationState state =
            CreateInitialState();


        for (int i = 0;
             i < actionPerTurn;
             i++)
        {
            BattleAction playerAction = null;

            if (playerActions != null &&
                playerActions.Count > i)
            {
                playerAction =
                    playerActions[i];
            }


            BattleAction enemyAction =
                enemyActions[i];


            int speed =
                GetEnemySpeed(
                    state,
                    enemyAction
                );

            predictedSpeeds.Add(
                speed
            );


            // Player‚ÌƒXƒLƒ‹‚É‚æ‚é
            // Ÿ‚Ìs“®‚Ö‚Ì‰e‹¿‚ğ“K—p
            if (playerAction != null &&
                playerAction.actionType ==
                ActionType.Skill)
            {
                if (playerAction.skillData != null &&
                    playerAction.skillData.effectType ==
                    SkillEffectType.Slow)
                {
                    state.enemySpeedModifier -=
                        playerAction.skillData.power;
                }
            }
        }


        return predictedSpeeds;
    }

    private void EndSimulationTurn(
    BattleSimulationState state)
    {
        // ==========================
        // Player Speed
        // ==========================

        if (state.playerSpeedBuffTurns > 0)
        {
            state.playerSpeedBuffTurns--;

            if (state.playerSpeedBuffTurns <= 0)
            {
                state.playerSpeedModifier = 0;
            }
        }


        // ==========================
        // Player Attack
        // ==========================

        if (state.playerAttackBuffTurns > 0)
        {
            state.playerAttackBuffTurns--;

            if (state.playerAttackBuffTurns <= 0)
            {
                state.playerAttack =
                    BattleManager.Instance
                        .Player
                        .AttackPower;
            }
        }


        // ==========================
        // Enemy Speed
        // ==========================

        if (state.enemySpeedDebuffTurns > 0)
        {
            state.enemySpeedDebuffTurns--;

            if (state.enemySpeedDebuffTurns <= 0)
            {
                state.enemySpeedModifier = 0;
            }
        }


        // ==========================
        // Enemy Attack
        // ==========================

        if (state.enemyAttackDebuffTurns > 0)
        {
            state.enemyAttackDebuffTurns--;

            if (state.enemyAttackDebuffTurns <= 0)
            {
                state.enemyAttack =
                    BattleManager.Instance
                        .Enemy
                        .AttackPower;
            }
        }
    }
}