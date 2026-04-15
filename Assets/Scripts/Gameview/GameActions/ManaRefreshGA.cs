using UnityEngine;

/// <summary>
/// Runs after <see cref="KillEnemyGA"/> resolves battle teardown (performer) and before the battle view is hidden.
/// Refills mana state and resets mana UI while the game view is still active.
/// </summary>
public class ManaRefreshGA : GameAction
{
}
