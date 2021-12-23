using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All rotational weapons can use this class
public class RotateWeaponController : WeaponController
{
    // Rotation
    public float idleRotation;
    public float startRotation;
    public float endRotation;
    public Timer attackTime;

    // Redefine the field isAttacking to link it with attackTimer.stopped
    protected override bool isAttacking
    { 
        get { return !this.attackTime.stopped; }
        set { }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Attack()
    {
        if (!attackTime.stopped) return;
        this.attackTime.Reset();
    }

    protected override void Move()
    {
        float angleZ = this.isAttacking ? this.startRotation * attackTime.percentage + this.endRotation * (1 - attackTime.percentage) : this.idleRotation;
        this.transform.eulerAngles = new Vector3(0, 0, angleZ * (this.facingRight ? 1 : -1));
    }

    protected override void UpdateTimers()
    {
        base.UpdateTimers();
        this.attackTime.Update();
        if (!isAttacking) this.attackedObjects.Clear();
    }
}
