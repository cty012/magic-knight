using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : EnemyController
{
    // Attack and jump
    [Header("Slime")]
    public float moveStepReversed;
    public Timer damageCD;

    // Allowed error when attacking
    private float deltaDist;

    // Sprint
    [Header("Sprint")]
    public float sprintStep;
    public Timer sprintTime;
    public Timer sprintCD;
    public Timer forceSprintCD;
    private bool sprintRight;
    private float jumpStep;

    // Attack
    [Header("Attack")]
    public int attackPower;
    public int pushPower;

    protected override void Awake()
    {
        base.Awake();
        this.deltaDist = 10;
        this.sprintRight = true;
        this.jumpStep = 400;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the target is a Movable
        MovableController controller = collision.collider.gameObject.GetComponent<MovableController>();
        if (controller == null) return;

        // Push the hostile target if is currently sprinting
        if (!this.sprintTime.stopped) controller.OnPushed(PushType.NORMAL_PUSH, this.pushPower * (this.sprintRight ? 1 : -1));
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        // Check if the target is a Movable
        MovableController controller = collision.collider.gameObject.GetComponent<MovableController>();
        if (controller == null) return;

        // Attack the hostile target if can inflict damage
        if (this.damageCD.stopped && this.IsHostile(collision.collider.gameObject))
        {
            controller.OnAttacked(AttackType.PHYSICAl_ATTACK, this.attackPower);

            // Avoid inflicting furthur damage to Player
            this.damageCD.Reset();
        }
    }

    protected override void UpdateTimers()
    {
        base.UpdateTimers();
        this.damageCD.Update();
        this.sprintTime.Update();
        this.sprintCD.Update();
        this.forceSprintCD.Update();
    }

    protected override void Move()
    {
        // Calculate distance
        Vector2 velocity = this.rigidbody.velocity;
        Vector2 distance = playerController.transform.position.DropZ().Subtract(this.transform.position.DropZ());

        this.facingRight = distance.x > 0;
        float playerDirection = this.facingRight ? 1 : -1;

        // If is currently sprinting
        if (!this.sprintTime.stopped)
        {
            velocity.x = this.sprintStep * (this.sprintRight ? 1 : -1);
        }
        // If can't see player then don't move
        else if (Math.Abs(distance.y) > this.vertDetectionDist || Math.Abs(distance.x) > this.horiDetectionDist)
        {
            velocity.x = 0;
        }
        // If is not close enough to attack then move toward the player
        else if (Math.Abs(distance.x) > this.attackDist + this.deltaDist)
        {
            velocity.x = this.moveStep * playerDirection;
        }
        // If too close then move backward
        else if (Math.Abs(distance.x) < this.attackDist - this.deltaDist)
        {
            // However if didn't sprint for too long sprint instead
            if (this.forceSprintCD.stopped)
            {
                velocity.x = 0;
                this.sprintRight = this.facingRight;
                velocity.y = this.jumpStep;
                this.sprintTime.Reset();
                this.sprintCD.Reset();
                this.forceSprintCD.Reset();
            }
            else velocity.x = -this.moveStepReversed * playerDirection;
        }
        // Sprint and jump if close enough
        else
        {
            velocity.x = 0;
            if (this.sprintCD.stopped)
            {
                this.sprintRight = this.facingRight;
                velocity.y = this.jumpStep;
                this.sprintTime.Reset();
                this.sprintCD.Reset();
                this.forceSprintCD.Reset();
            }
        }

        // Add knockback effect
        velocity.x += this.knockback.curValue;

        this.rigidbody.velocity = velocity;
    }
}
