public class VunerableGA : GameAction
{
    public int magnitude;
    public int duration;
    public bool isPlayer; // afflicted unit is player
    public bool consumeDuration;
    public StatusEffect statusEffect;

    public VunerableGA(int magnitude, int duration, bool isPlayer, StatusEffect statusEffect, bool consumeDuration = true)
    {
        this.magnitude = magnitude;
        this.duration = duration;
        this.isPlayer = isPlayer;
        this.statusEffect = statusEffect;
        this.consumeDuration = consumeDuration;
    }
}
