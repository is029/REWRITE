using System;

[Serializable]
public class StatusEffect
{
    public StatusEffectType type;

    public int power;

    public int remainingTurns;

    public StatusEffect(
        StatusEffectType type,
        int power,
        int duration)
    {
        this.type = type;
        this.power = power;
        this.remainingTurns = duration;
    }

    public StatusEffect Clone()
    {
        return new StatusEffect(
            type,
            power,
            remainingTurns
        );
    }
}