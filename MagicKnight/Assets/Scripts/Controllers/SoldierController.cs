using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : EnemyController
{
    [Header("Soldier Settings")]
    // Attack
    public Timer attackCD;

    public override void OnAttacked(AttackType type, int power)
    {
        base.OnAttacked(type, power);
        this.freezeTime.Reset();
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
        base.UpdateTimers();
        this.attackCD.Update();
    }
}
