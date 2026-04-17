public class NextAreaGA : GameAction
{
    public int nextLevel;
    public bool applyLevelTransition;

    public NextAreaGA(int nextLevel, bool applyLevelTransition = true)
    {
        this.nextLevel = nextLevel;
        this.applyLevelTransition = applyLevelTransition;
    }
}
