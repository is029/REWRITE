using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSkill",
    menuName = "REWRITE/Skill Data"
)]
public class SkillData : ScriptableObject
{
    [Header("基本情報")]
    public string skillName;

    [TextArea(2, 4)]
    public string description;

    [Header("戦闘設定")]
    public int speed = 5;

    public int power = 0;

    [Header("効果")]
    public SkillEffectType effectType;

    [Header("追加設定")]
    public int duration = 1;

    [Header("クールタイム")]
    [Min(0)]
    public int cooldown = 0;

    [Header("アニメーション")]
    public string animationTrigger = "Skill";
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

    Counter,
    Rewrite,

    Burn,
    Poison
}