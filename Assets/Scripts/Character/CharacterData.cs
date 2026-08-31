using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCharacter",
    menuName = "REWRITE/Character Data"
)]
public class CharacterData : ScriptableObject
{
    [Header("基本情報")]
    public string characterName;

    [TextArea(2, 4)]
    public string description;

    [Header("ステータス")]
    public int maxHP = 100;

    public int attackPower = 20;

    public int normalAttackSpeed = 5;

    [Header("スキル")]
    public SkillData skill1;
    public SkillData skill2;
    public SkillData skill3;
}