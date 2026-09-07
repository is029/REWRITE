using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("キャラクター選択ボタン")]
    [SerializeField] private Button[] characterButtons;

    [Header("キャラクター情報")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text speedText;

    [Header("開始ボタン")]
    [SerializeField] private Button startButton;

    private CharacterSelectManager selectManager;

    private void Start()
    {
        selectManager =
            CharacterSelectManager.Instance;

        startButton.interactable = false;

        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;

            characterButtons[i].onClick.AddListener(
                () => SelectCharacter(index)
            );
        }

        startButton.onClick.AddListener(
            () => selectManager.StartGame()
        );
    }

    private void SelectCharacter(int index)
    {
        selectManager.SelectCharacter(index);

        CharacterData character =
            selectManager.Characters[index];

        // 名前
        nameText.text =
            character.characterName;

        // 説明
        descriptionText.text =
            character.description;

        // ステータス
        hpText.text =
            "HP : " + character.maxHP;

        attackText.text =
            "攻撃力 : " + character.attackPower;

        speedText.text =
            "攻撃速度 : " + character.normalAttackSpeed;

        // キャラクターを選択したので開始可能
        startButton.interactable = true;
    }
}