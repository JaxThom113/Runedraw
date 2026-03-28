using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSystem : Singleton<ShieldSystem>
{
    [SerializeField] public PlayerView playerView;
    [SerializeField] public EnemyView enemyView;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyShieldGA>(ApplyShieldPerformer);
        ActionSystem.AttachPerformer<ClearAllShieldsGA>(ClearAllShieldsPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyShieldGA>();
        ActionSystem.DetachPerformer<ClearAllShieldsGA>();
    }

    public void Setup(PlayerView playerView, EnemyView enemyView)
    {
        this.playerView = playerView;
        this.enemyView = enemyView;
    }


    public void ClearAllShields()
    {
        if (playerView != null) playerView.ClearShield();
        if (enemyView != null) enemyView.ClearShield();
    }

    private IEnumerator ClearAllShieldsPerformer(ClearAllShieldsGA clearAllShieldsGA)
    {
        ClearAllShields();
        yield return null;
    }

    private IEnumerator ApplyShieldPerformer(ApplyShieldGA applyShieldGA)
    {
        int amount = applyShieldGA.Amount;
        if (applyShieldGA.isPlayer)
            playerView.AddShield(amount);
        else
            enemyView.AddShield(amount);
        yield return null;
    }
}
