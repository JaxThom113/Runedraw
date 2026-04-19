public class NextAreaGA : GameAction
{
    public int nextArea;
    public bool applyLevelTransition;

    public NextAreaGA(int nextArea, bool applyLevelTransition = true)
    {
        this.nextArea = nextArea;
        this.applyLevelTransition = applyLevelTransition;
    }
}
