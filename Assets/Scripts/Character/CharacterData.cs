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

    [Header("バトル画像")]
    public Sprite battleSprite;
    public Vector2 battleImageSize =
        new Vector2(300f, 300f);

    [Header("アニメーション")]
    public RuntimeAnimatorController animatorController;

    [Header("スキル使用の重み")]
    [Min(0)]
    public int skill1Weight = 33;

    [Min(0)]
    public int skill2Weight = 33;

    [Min(0)]
    public int skill3Weight = 34;

    [Header("敵AI 行動の重み")]
    [Min(0)]
    public int attackWeight = 60;

    [Min(0)]
    public int defendWeight = 20;

    [Min(0)]
    public int healWeight = 0;

    [Min(0)]
    public int skillWeight = 20;

}