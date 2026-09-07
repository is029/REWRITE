using UnityEngine;

[System.Serializable]
public class BattleAction
{
    public ActionType actionType;

    public int speed;

    public bool isPlayer;

    public SkillData skillData;

    public BattleAction(
        ActionType type,
        bool playerAction,
        BattleUnit unit,
        SkillData skill = null)
    {
        actionType = type;
        isPlayer = playerAction;
        skillData = skill;

        speed = GetSpeed(type, unit, skill);
    }

    private int GetSpeed(
    ActionType type,
    BattleUnit unit,
    SkillData skill)
    {
        return unit.GetActionSpeed(
            type,
            skill
        );
    }
}