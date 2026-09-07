using UnityEngine;
using UnityEngine.SceneManagement;

public class RoguelikeManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private CharacterData playerCharacterData;

    public static RoguelikeManager Instance { get; private set; }

    public PlayerRunData PlayerData { get; private set; }

    public enum StageType
    {
        NormalBattle,
        CharacterSelect,
        Reward,
        Upgrade,
        Boss,
        Clear
    }

    [Header("設定")]
    [SerializeField] private int normalBattleCount = 3;

    [Header("コイン報酬")]
    [SerializeField] private int baseBattleCoins = 50;
    [SerializeField] private int fastClearBonus = 30;
    [SerializeField] private int mediumClearBonus = 20;
    [SerializeField] private int slowClearBonus = 10;

    public int CurrentBattle { get; private set; } = 1;
    public StageType CurrentStage { get; private set; }

    private void Awake()
    {
        Debug.Log(
            "【RoguelikeManager Awake開始】" +
            " Instance=" + (Instance != null)
        );

        if (Instance != null && Instance != this)
        {
            Debug.Log(
                "【重複RoguelikeManagerを削除】"
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        PlayerData = new PlayerRunData();

        if (playerCharacterData != null)
        {
            PlayerData.Initialize(
                playerCharacterData.maxHP
            );

            Debug.Log(
                "【PlayerData初期化】" +
                " SpeedBonus=" +
                PlayerData.speedBonus
            );
        }
        else
        {
            Debug.LogError(
                "RoguelikeManagerにPlayer CharacterDataが設定されていません。"
            );
        }
    }

    public CharacterData PlayerCharacterData
    {
        get { return playerCharacterData; }
    }

    public void StartCharacterSelect()
    {
        CurrentStage = StageType.CharacterSelect;

        SceneManager.LoadScene("CharacterSelectScene");
    }

    // 通常戦闘開始
    public void StartNormalBattle()
    {
        CurrentStage = StageType.NormalBattle;

        SceneManager.LoadScene("BattleScene");
    }

    // 通常戦闘クリア
    public void NormalBattleClear(int turnCount)
    {
        Debug.Log(
            "通常戦闘 " +
            CurrentBattle +
            " クリア！"
        );

        // コイン報酬を計算
        int bonus = GetTurnBonus(turnCount);

        int totalCoins =
            baseBattleCoins + bonus;

        PlayerData.AddCoins(totalCoins);

        Debug.Log(
            "【コイン報酬】" +
            " ターン数=" + turnCount +
            " 基本=" + baseBattleCoins +
            " ボーナス=" + bonus +
            " 合計=" + totalCoins +
            " 所持コイン=" + PlayerData.coins
        );

        // 3戦目？
        if (CurrentBattle >= normalBattleCount)
        {
            StartUpgrade();
        }
        else
        {
            StartReward();
        }
    }

    // ターン数によるボーナス
    private int GetTurnBonus(int turnCount)
    {
        if (turnCount <= 5)
        {
            return fastClearBonus;
        }

        if (turnCount <= 10)
        {
            return mediumClearBonus;
        }

        if (turnCount <= 15)
        {
            return slowClearBonus;
        }

        return 0;
    }

    // 報酬画面
    public void StartReward()
    {
        CurrentStage = StageType.Reward;

        Debug.Log("===== REWARD =====");
    }

    // 報酬選択完了
    public void RewardComplete()
    {
        CurrentBattle++;

        Debug.Log(
            "次の通常戦闘へ：" +
            CurrentBattle
        );

        StartNormalBattle();
    }

    // 強化画面
    public void StartUpgrade()
    {
        CurrentStage = StageType.Upgrade;

        Debug.Log("===== UPGRADE =====");
    }

    // 強化完了
    public void UpgradeComplete()
    {
        StartBoss();
    }

    // ボス戦
    public void StartBoss()
    {
        CurrentStage = StageType.Boss;

        Debug.Log("===== BOSS BATTLE =====");

        SceneManager.LoadScene("BossBattleScene");
    }

    public void SetPlayerCharacter(
    CharacterData character)
    {
        if (character == null)
        {
            Debug.LogError(
                "CharacterDataが設定されていません。"
            );

            return;
        }

        playerCharacterData = character;

        PlayerData.Initialize(
            character.maxHP
        );

        Debug.Log(
            "プレイヤーキャラクター設定：" +
            character.characterName
        );
    }

    // ゲームクリア
    public void GameClear()
    {
        CurrentStage = StageType.Clear;

        Debug.Log("===== GAME CLEAR =====");

        SceneManager.LoadScene("ClearScene");
    }
}