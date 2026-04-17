using UnityEngine;

/// <summary>
/// Hides the battle <see cref="CameraTransitionSystem.GameViewContainer"/> after mana UI has been reset.
/// Not to be confused with <see cref="GameOverGA"/> (player death), which toggles <see cref="PlayerSystem.GameView"/>.
/// </summary>
public class EndBattleViewGA : GameAction
{
}
