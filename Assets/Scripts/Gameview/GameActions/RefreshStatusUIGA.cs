public class RefreshStatusUIGA : GameAction
{
    public bool refreshBothSides;
    public bool afflictedUnitIsPlayer;

    public RefreshStatusUIGA()
    {
        refreshBothSides = true;
    }

    public RefreshStatusUIGA(bool afflictedUnitIsPlayer)
    {
        refreshBothSides = false;
        this.afflictedUnitIsPlayer = afflictedUnitIsPlayer;
    }
}
