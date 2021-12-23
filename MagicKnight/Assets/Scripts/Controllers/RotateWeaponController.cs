using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWeaponController : WeaponController
{
    // Rotation
    public float idleRotation;
    public float startRotation;
    public float endRotation;
    public float attackTime;
    private Timer attackTimer;

    protected override bool isAttacking
    { 
        get { return !this.attackTimer.stopped; }
        set { }
    }

    protected override void Awake()
    {
        base.Awake();
        this.attackTimer = new Timer(this.attackTime);
    }

    public override void Attack()
    {
        if (!attackTimer.stopped) return;
        this.attackTimer.Reset();
    }

    protected override void Move()
    {
        float angleZ = this.isAttacking ? this.startRotation * attackTimer.percentage + this.endRotation * (1 - attackTimer.percentage) : this.idleRotation;
        this.transform.eulerAngles = new Vector3(0, 0, angleZ * (this.facingRight ? 1 : -1));
    }

    protected override void UpdateTimers()
    {
        base.UpdateTimers();
        this.attackTimer.Update();
        if (!isAttacking) this.attackedObjects.Clear();
    }
}
