using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance { get; private set; }

    [Header("キャラクター")]
    [SerializeField] private CharacterData[] characters;

    private int selectedIndex = -1;

    public CharacterData SelectedCharacter
    {
        get
        {
            if (selectedIndex < 0 ||
                selectedIndex >= characters.Length)
            {
                return null;
            }

            return characters[selectedIndex];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public CharacterData[] Characters
    {
        get { return characters; }
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 ||
            index >= characters.Length)
        {
            return;
        }

        selectedIndex = index;

        Debug.Log(
            "キャラクター選択：" +
            characters[index].characterName
        );
    }

    public void StartGame()
    {
        CharacterData character =
            SelectedCharacter;

        if (character == null)
        {
            Debug.LogWarning(
                "キャラクターが選択されていません。"
            );

            return;
        }

        // 選択したキャラクターでPlayerDataを初期化
        RoguelikeManager.Instance
            .SetPlayerCharacter(character);

        Debug.Log(
            "ゲーム開始：" +
            character.characterName
        );

        SceneManager.LoadScene("BattleScene");
    }
}