using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    [Header("Skill Panel")]
    [SerializeField] private GameObject skillPanel;

    [Header("Skill Buttons")]
    [SerializeField] private Button skillButton1;
    [SerializeField] private Button skillButton2;
    [SerializeField] private Button skillButton3;

    [Header("Skill Text")]
    [SerializeField] private TMP_Text skillText1;
    [SerializeField] private TMP_Text skillText2;
    [SerializeField] private TMP_Text skillText3;

    private CharacterData characterData;

    private void Start()
    {
        characterData =
            BattleManager.Instance.Player.CharacterData;

        SetupSkills();

        skillPanel.SetActive(false);

        skillButton1.onClick.AddListener(
            () => SelectSkill(characterData.skill1)
        );

        skillButton2.onClick.AddListener(
            () => SelectSkill(characterData.skill2)
        );

        skillButton3.onClick.AddListener(
            () => SelectSkill(characterData.skill3)
        );
    }

    public void OpenSkillPanel()
    {
        SetupSkills();
        skillPanel.SetActive(true);
    }

    public void CloseSkillPanel()
    {
        skillPanel.SetActive(false);
    }

    private void SetupSkills()
    {
        SetSkillText(
            skillText1,
            characterData.skill1
        );

        SetSkillText(
            skillText2,
            characterData.skill2
        );

        SetSkillText(
            skillText3,
            characterData.skill3
        );
    }

    private void SetSkillText(
    TMP_Text text,
    SkillData skill)
    {
        if (skill == null)
        {
            text.text = "---";
            return;
        }

        int speed = skill.speed;

        BattleUnit player =
            BattleManager.Instance.Player;

        if (player != null)
        {
            speed =
                player.GetActionSpeed(
                    ActionType.Skill,
                    skill
                );
        }

        text.text =
            skill.skillName +
            "\nSPEED " +
            speed;
    }

    private void SelectSkill(SkillData skill)
    {
        if (skill == null)
        {
            return;
        }

        bool success =
            FindObjectOfType<TurnManager>()
            .AddPlayerSkill(skill);

        if (success)
        {
            CloseSkillPanel();

            // BattleUIÇçXêV
            FindObjectOfType<BattleUIManager>()
                .RefreshUI();
        }
    }
}