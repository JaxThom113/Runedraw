public class PoisonGA : GameAction
{
    public int magnitude;
    public int duration;
    public bool isPlayer;
    public StatusEffect statusEffect;

    public PoisonGA(int magnitude, int duration, bool isPlayer, StatusEffect statusEffect)
    {
        this.magnitude = magnitude;
        this.duration = duration;
        this.isPlayer = isPlayer;
        this.statusEffect = statusEffect;
    }
}
