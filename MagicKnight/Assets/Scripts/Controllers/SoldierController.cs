using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : EnemyController
{

    // Attack
    private Timer attackCD;

    // the soldier is freezed for a certain amount of time if attacked
    private Timer freezeTimer;

    protected override void Awake()
    {
        base.Awake();

        // Fields defined in CharacterController
        this.maxHp = 100;
        this.hp = this.maxHp;

        this.physicalDefence = 0;
        this.physicalResist = 2;
        this.magicalDefence = 0;
        this.magicalResist = 1.2f;

        this.knockback = new Timer(0, 500);
        this.knockbackResist = 1.2f;

        // Fields defined in EnemyController
        this.horiDetectionDist = 700;
        this.vertDetectionDist = 180;
        this.attackDist = 60;
        this.moveStep = 150;

        // Fields defined in SoldierController
        this.attackCD = new Timer(1.5f);
        this.freezeTimer = new Timer(0.5f);
    }

    public override void OnAttacked(AttackType type, int power)
    {
        base.OnAttacked(type, power);
        this.freezeTimer.Reset();
    }

    protected override void Move()
    {
        // Calculate distance
        Vector2 velocity = this.rigidbody.velocity;
        Vector2 distance = playerController.transform.position.DropZ().Subtract(this.transform.position.DropZ());

        this.facingRight = distance.x > 0;

        // If can't see player then don't move
        if (Math.Abs(distance.y) > this.vertDetectionDist || Math.Abs(distance.x) > this.horiDetectionDist)
        {
            velocity.x = 0;
        }
        // If is not close enough to attack then move toward the player
        else if (distance.x > this.attackDist)
        {
            velocity.x = this.moveStep;
        }
        else if (distance.x < -this.attackDist)
        {
            velocity.x = -this.moveStep;
        }
        // Stop and attack if close enough
        else
        {
            velocity.x = 0;
            if (this.weaponController != null && this.attackCD.stopped)
            {
                this.weaponController.Attack();
                this.attackCD.Reset();
            }
        }

        // Add knockback effect
        velocity.x += this.knockback.curValue;

        this.rigidbody.velocity = velocity;
    }

    protected override void UpdateTimers()
    {
        if (!this.freezeTimer.stopped)
        {
            this.freezeTimer.Update();
            return;
        }
        base.UpdateTimers();
        this.attackCD.Update();
    }
}
