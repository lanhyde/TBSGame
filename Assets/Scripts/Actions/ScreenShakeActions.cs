using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeActions : MonoBehaviour
{
    private void Start()
    {
        ShootAction.OnAnyShoot += OnAnyShoot;
        GrenadeProjectile.OnAnyGrenadeExploded += OnAnyGrenadeExploded;
        SwordAction.OnAnySwordHit += OnAnySwordHit;
    }

    private void OnDestroy()
    {
        ShootAction.OnAnyShoot -= OnAnyShoot;
        GrenadeProjectile.OnAnyGrenadeExploded -= OnAnyGrenadeExploded;
        SwordAction.OnAnySwordHit -= OnAnySwordHit;
    }

    private void OnAnyShoot(object sender, ShootAction.OnShootEventArgs args)
    {
        ScreenShake.Instance.Shake();
    }
    
    private void OnAnySwordHit(object sender, EventArgs args)
    {
        ScreenShake.Instance.Shake(2f);
    }
    private void OnAnyGrenadeExploded(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(5);
    }
}
