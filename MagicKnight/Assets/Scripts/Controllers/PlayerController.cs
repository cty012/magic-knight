using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MovableController
{
    // Controll keys
    private KeyCode keyUp;
    private KeyCode keyDown;
    private KeyCode keyLeft;
    private KeyCode keyRight;
    private KeyCode keySprint;
    private KeyCode keyAttack;

    // Movement command (only indicate player input)
    private bool moveLeft;
    private bool moveRight;
    private bool jump;
    private bool drop;

    // Move
    private float moveStep;

    // Jump
    private DiscreteTimer jumpNum;
    private float jumpStep;
    private float maxVertStep;

    // sprint
    private bool allowSprint;
    private float sprintStep;
    private Timer sprintTime;
    private Timer sprintCD;
    private bool sprintingRight;

    // attack
    public bool canAttack { get; set; }

    private Timer attackCD;

    // Emit event when changing HP
    public override int hp
    {
        get { return base.hp; }
        set
        {
            base.hp = value;
            EventManager.instance.Emit("hp-change", new PlayerStatusChangeEvent(
                PlayerStatusChangeEventType.HP_CHANGE, base.hp));
        }
    }

    // inventory TODO

    // Reset is called when the script is attached to a game object
    protected override void Awake()
    {
        base.Awake();
        
        // Fields defined in PlayerController
        this.weaponAnchor = new Vector2(0.9f, 0.55f);

        this.keyUp = KeyCode.W;
        this.keyDown = KeyCode.S;
        this.keyLeft = KeyCode.A;
        this.keyRight = KeyCode.D;
        this.keySprint = KeyCode.LeftShift;
        this.keyAttack = KeyCode.J;

        this.moveLeft = false;
        this.moveRight = false;
        this.jump = false;
        this.drop = true;

        this.moveStep = 250;

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
    }

    public override bool IsHostile(GameObject obj)
    {
        return Utils.IsEnemy(obj);
    }

    // Detect keyboard status to decide player input
    private void GetKeyboardInput()
    {
        // Detect orientation change
        if (Input.GetKeyDown(this.keyRight)) this.facingRight = true;
        else if (Input.GetKeyDown(this.keyLeft)) this.facingRight = false;

        // Detect movement commands
        this.moveLeft = Input.GetKey(this.keyLeft);
        this.moveRight = Input.GetKey(this.keyRight);
        this.jump = Input.GetKeyDown(this.keyUp);
        this.drop = Input.GetKeyDown(this.keyDown);
        if (this.allowSprint && this.sprintCD.stopped && Input.GetKeyDown(this.keySprint) && this.rigidbody.velocity.x != 0)
        {
            this.sprintTime.Reset();
            this.sprintCD.Reset();
            this.sprintingRight = this.facingRight;
        }

        // Detect attack commands
        if (this.weaponController != null && this.canAttack && this.attackCD.stopped && Input.GetKeyDown(this.keyAttack))
        {
            this.weaponController.Attack();
            this.attackCD.Reset();
        }
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
        // Get Player Commands
        this.GetKeyboardInput();

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
