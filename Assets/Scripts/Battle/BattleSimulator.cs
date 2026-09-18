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
    // 初期状態を作成
    // ========================================

    public BattleSimulationState CreateInitialState()
    {
        return new BattleSimulationState(
            BattleManager.Instance.Player,
            BattleManager.Instance.Enemy
        );
    }


    // ========================================
    // 3行動分を未来シミュレーション
    // ========================================
    public List<BattleSimulationState> SimulateTurn(
     List<BattleAction> playerActions,
     List<BattleAction> enemyActions)
    {
        List<BattleSimulationState> states =
            new List<BattleSimulationState>();

        // ========================================
        // 敵行動がない場合
        // ========================================
        if (enemyActions == null ||
            enemyActions.Count == 0)
        {
            Debug.LogWarning(
                "BattleSimulator：敵の行動がありません。"
            );

            return states;
        }

        // 敵の行動数を基準にする
        int actionPerTurn = enemyActions.Count;

        // ========================================
        // 初期状態
        // ========================================
        BattleSimulationState currentState =
            CreateInitialState();

        // ========================================
        // 1～3（または4）行動をシミュレート
        // ========================================
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

            // ====================================
            // すでにどちらかが死亡している場合
            // ====================================
            if (currentState.playerHP <= 0 ||
                currentState.enemyHP <= 0)
            {
                currentState.playerHP =
                    Mathf.Max(0, currentState.playerHP);

                currentState.enemyHP =
                    Mathf.Max(0, currentState.enemyHP);

                states.Add(
                    currentState.Clone()
                );

                continue;
            }

            // ====================================
            // 防御状態
            // ====================================
            currentState.playerDefending =
                playerAction != null &&
                playerAction.actionType ==
                ActionType.Defend;

            currentState.enemyDefending =
                enemyAction != null &&
                enemyAction.actionType ==
                ActionType.Defend;

            // ====================================
            // SPEED計算
            // ====================================
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

            // ====================================
            // SPEED比較
            // ====================================
            if (playerSpeed > enemySpeed)
            {
                // ------------------------------
                // プレイヤーが先
                // ------------------------------
                SimulatePlayerAction(
                    currentState,
                    playerAction
                );

                // プレイヤー攻撃で敵死亡
                if (currentState.enemyHP <= 0)
                {
                    currentState.enemyHP = 0;

                    states.Add(
                        currentState.Clone()
                    );

                    continue;
                }

                // ------------------------------
                // 敵行動
                // ------------------------------
                SimulateEnemyAction(
                    currentState,
                    enemyAction
                );

                // ------------------------------
                // 敵攻撃でプレイヤー死亡
                // ------------------------------
                if (currentState.playerHP <= 0)
                {
                    currentState.playerHP = 0;
                }
            }
            else
            {
                // ------------------------------
                // 敵が先
                // ------------------------------
                SimulateEnemyAction(
                    currentState,
                    enemyAction
                );

                // ------------------------------
                // 敵攻撃でプレイヤー死亡
                // ------------------------------
                if (currentState.playerHP <= 0)
                {
                    currentState.playerHP = 0;

                    states.Add(
                        currentState.Clone()
                    );

                    continue;
                }

                // ------------------------------
                // プレイヤー行動
                // ------------------------------
                SimulatePlayerAction(
                    currentState,
                    playerAction
                );

                // ------------------------------
                // プレイヤー攻撃で敵死亡
                // ------------------------------
                if (currentState.enemyHP <= 0)
                {
                    currentState.enemyHP = 0;
                }
            }

            // ====================================
            // HPを0未満にしない
            // ====================================
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

            // ====================================
            // この行動終了時の状態を保存
            // ====================================
            states.Add(
                currentState.Clone()
            );
        }

        // ========================================
        // ターン終了処理
        // ========================================
        //
        // 実際のBattleUnitでは
        //
        // 1. バフ・デバフのターン減少
        // 2. 毒・火傷などの状態異常処理
        // 3. スキルクールダウン減少
        //
        // の順番
        //
        EndSimulationTurn(
            currentState
        );

        // ========================================
        // 毒・火傷などの持続ダメージ
        // ========================================
        ProcessSimulationStatusEffects(
            currentState
        );

        // ========================================
        // 最終HPを補正
        // ========================================
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

        // ========================================
        // 最後の予測結果を
        // DoT反映後の状態に更新
        // ========================================
        if (states.Count > 0)
        {
            states[states.Count - 1] =
                currentState.Clone();
        }

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
    // Player行動
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
    // Enemy行動
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

                // 敵スキルは後で追加
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
            // ダメージ
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
            // 回復
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
            // Enemy Speed低下
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
            // Enemy 攻撃力低下
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
            // Player Speed上昇
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
            // Player 攻撃力上昇
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
    if (state == null)
        return;

    // ========================================
    // プレイヤー
    // ========================================

    if (state.playerHP > 0)
    {
        for (int i = state.playerStatusEffects.Count - 1;
            i >= 0;
            i--)
        {
            StatusEffect effect =
                state.playerStatusEffects[i];

            if (effect == null)
                continue;

            // ================================
            // 火傷
            // ================================

            if (effect.type ==
                StatusEffectType.Burn)
            {
                state.playerHP -=
                    effect.power;

                Debug.Log(
                    "Simulation：プレイヤーが火傷で " +
                    effect.power +
                    " ダメージ"
                );
            }

            // ================================
            // 毒
            // ================================

            else if (effect.type ==
                     StatusEffectType.Poison)
            {
                state.playerHP -=
                    effect.power;

                Debug.Log(
                    "Simulation：プレイヤーが毒で " +
                    effect.power +
                    " ダメージ"
                );
            }

            // ================================
            // 残りターン減少
            // ================================

            effect.remainingTurns--;

            // ================================
            // 効果終了
            // ================================

            if (effect.remainingTurns <= 0)
            {
                state.playerStatusEffects.RemoveAt(i);
            }
        }

        state.playerHP =
            Mathf.Max(
                0,
                state.playerHP
            );
    }
    else
    {
        state.playerHP = 0;
    }


    // ========================================
    // 敵
    // ========================================

    if (state.enemyHP > 0)
    {
        for (int i = state.enemyStatusEffects.Count - 1;
            i >= 0;
            i--)
        {
            StatusEffect effect =
                state.enemyStatusEffects[i];

            if (effect == null)
                continue;

            // ================================
            // 火傷
            // ================================

            if (effect.type ==
                StatusEffectType.Burn)
            {
                state.enemyHP -=
                    effect.power;

                Debug.Log(
                    "Simulation：敵が火傷で " +
                    effect.power +
                    " ダメージ"
                );
            }

            // ================================
            // 毒
            // ================================

            else if (effect.type ==
                     StatusEffectType.Poison)
            {
                state.enemyHP -=
                    effect.power;

                Debug.Log(
                    "Simulation：敵が毒で " +
                    effect.power +
                    " ダメージ"
                );
            }

            // ================================
            // 残りターン減少
            // ================================

            effect.remainingTurns--;

            // ================================
            // 効果終了
            // ================================

            if (effect.remainingTurns <= 0)
            {
                state.enemyStatusEffects.RemoveAt(i);
            }
        }

        state.enemyHP =
            Mathf.Max(
                0,
                state.enemyHP
            );
    }
    else
    {
        state.enemyHP = 0;
    }
}

    // ========================================
    // 敵未来Speedだけ取得
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


            // Playerのスキルによる
            // 次の行動への影響を適用
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