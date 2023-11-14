using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeActions : MonoBehaviour
{
    private void Start()
    {
        ShootAction.OnAnyShoot += OnAnyShoot;
    }

    private void OnDestroy()
    {
        ShootAction.OnAnyShoot -= OnAnyShoot;
    }

    private void OnAnyShoot(object sender, ShootAction.OnShootEventArgs args)
    {
        ScreenShake.Instance.Shake();
    }
}
