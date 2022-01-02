using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MovableController
{
    // Movement command (only indicate player input)
    public bool moveLeft { get; set; }
    public bool moveRight { get; set; }
    public bool jump { get; set; }
    public bool drop { get; set; }

    // Move
    public float moveStep { get; set; }

    // Jump
    public DiscreteTimer jumpNum { get; set; }
    public float jumpStep { get; set; }
    public float maxVertStep { get; set; }

    // sprint
    public bool allowSprint { get; set; }
    public float sprintStep { get; set; }
    public Timer sprintTime { get; set; }
    public Timer sprintCD { get; set; }
    public bool sprintingRight { get; set; }

    // attack
    public bool canAttack { get; set; }

    public Timer attackCD { get; set; }

    // Emit event when changing HP
    public override int hp
    {
        get { return base.hp; }
        set
        {
            base.hp = value;
            EventManager.instance.Emit("hp-change", new PlayerStatusChangeEvent(
                PlayerStatusChangeEventType.HP_CHANGE, base.hp, base.maxHp));
        }
    }

    public Inventory inventory { get; private set; }

    // Reset is called when the script is attached to a game object
    protected override void Awake()
    {
        base.Awake();
        
        // Fields defined in PlayerController
        this.weaponAnchor = new Vector2(0.9f, 0.55f);

        this.moveLeft = false;
        this.moveRight = false;
        this.jump = false;
        this.drop = true;

        this.moveStep = 300;

        this.jumpNum = new DiscreteTimer(1);
        this.jumpStep = 1000;
        this.maxVertStep = 2500;

        this.allowSprint = true;
        this.sprintStep = 1500;
        this.sprintTime = new Timer(0.15f);
        this.sprintCD = new Timer(2);
        this.sprintingRight = true;

        this.canAttack = true;
        this.attackCD = new Timer(0.5f);

        this.inventory = new Inventory();
    }

    public override bool IsHostile(GameObject obj)
    {
        return Utils.IsEnemy(obj);
    }

    // Check if player is on the ground
    protected override void CheckOnGround()
    {
        base.CheckOnGround();
        if (this.onGround)
        {
            this.drop = false;
            this.jumpNum.Reset();
        }
    }

    // Update all time and cd by subtracting deltaTime
    protected override void UpdateTimers()
    {
        base.UpdateTimers();
        this.sprintTime.Update();
        this.sprintCD.Update();
        this.attackCD.Update();
    }

    protected override void Move()
    {
        // Sprint if is currently sprinting (and ignore keyboard inputs)
        if (!sprintTime.stopped)
        {
            int sprintDirection = this.sprintingRight ? 1 : -1;
            this.rigidbody.velocity = new Vector2(this.sprintStep * sprintDirection, 0);
        }
        // Otherwise use keyboard inputs
        else
        {
            // calculate x direction velocity
            Vector2 velocity = new Vector2(0, this.rigidbody.velocity.y);
            if (this.moveLeft) velocity.x -= this.moveStep;
            if (this.moveRight) velocity.x += this.moveStep;

            // Add knockback effect
            velocity.x += this.knockback.curValue;

            // if jumps (and is able to jump) then set y direction velocity to jump speed
            if (this.jump && !this.jumpNum.stopped)
            {
                velocity.y = this.jumpStep;
                this.jumpNum.Update();
            }

            // if drop set velocity to maximum pointing downward
            if (this.drop)
            {
                velocity.y = -this.maxVertStep;
            }

            // make sure y direction has speed less than this.verticalMaxStep
            velocity.y = velocity.y.Clamp(-this.maxVertStep, this.maxVertStep);

            // apply the calculated velocity
            this.rigidbody.velocity = velocity;
        }
    }
}
