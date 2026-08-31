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
        switch (type)
        {
            case ActionType.Attack:
                return unit.NormalAttackSpeed;

            case ActionType.Defend:
                return 8;

            case ActionType.Skill:

                if (skill != null)
                {
                    return skill.speed;
                }

                return 0;

            case ActionType.Heal:
                return 6;

            default:
                return 0;
        }
    }
}