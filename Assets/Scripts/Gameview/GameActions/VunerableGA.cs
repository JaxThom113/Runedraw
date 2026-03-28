public class VunerableGA : GameAction
{
    public int damage;
    public int duration;
    public bool isPlayer; // afflicted unit is player
    public bool consumeDuration;
    public StatusEffect statusEffect;

    public VunerableGA(int damage, int duration, bool isPlayer, StatusEffect statusEffect, bool consumeDuration = true)
    {
        this.damage = damage;
        this.duration = duration;
        this.isPlayer = isPlayer;
        this.statusEffect = statusEffect;
        this.consumeDuration = consumeDuration;
    }
}
