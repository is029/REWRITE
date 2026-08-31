using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSkill",
    menuName = "REWRITE/Skill Data"
)]
public class SkillData : ScriptableObject
{
    [Header("Šî–{î•ñ")]
    public string skillName;

    [TextArea(2, 4)]
    public string description;

    [Header("í“¬İ’è")]
    public int speed = 5;

    public int power = 0;

    [Header("Œø‰Ê")]
    public SkillEffectType effectType;

    [Header("’Ç‰Áİ’è")]
    public int duration = 1;
}

public enum SkillEffectType
{
    None,

    Damage,
    Heal,

    Slow,
    SpeedUp,

    AttackUp,
    AttackDown,

    DefenseDown,

    Counter,
    Rewrite,

    Burn,
    Poison
}