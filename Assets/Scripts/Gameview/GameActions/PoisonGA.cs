public class PoisonGA : GameAction
{
    public int damage;
    public int duration;
    public bool isPlayer;
    public StatusEffect statusEffect;

    public PoisonGA(int damage, int duration, bool isPlayer, StatusEffect statusEffect)
    {
        this.damage = damage;
        this.duration = duration;
        this.isPlayer = isPlayer;
        this.statusEffect = statusEffect;
    }
}
