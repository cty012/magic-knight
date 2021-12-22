using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Event id from event manager
    private int eventId;

    // Useful components
    public new Rigidbody2D rigidbody { get; private set; }
    public new Collider2D collider { get; private set; }
    public new RectTransform transform { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    // Child nodes
    public GameObject weapon { get; private set; }

    // Controll keys
    private KeyCode keyUp;
    private KeyCode keyDown;
    private KeyCode keyLeft;
    private KeyCode keyRight;
    private KeyCode keySprint;

    // Movement command (only indicate player input)
    private bool moveLeft;
    private bool moveRight;
    private bool jump;
    private bool drop;

    // Player orientation (always face right at the beginning)
    private bool _facingRight;
    public bool facingRight
    {
        get { return this._facingRight; }
        private set
        {
            this._facingRight = value;
            this.spriteRenderer.flipX = !value;
        }
    }

    // Move
    private float moveStep;

    // Jump
    private int curJumpNum;
    private int maxJumpNum;
    private float jumpStep;
    private float maxVertStep;

    // sprint
    private bool allowSprint;
    private float sprintStep;
    private float curSprintTime;
    private float maxSprintTime;
    private float curSprintCD;
    private float maxSprintCD;
    private bool sprintingRight;

    // hp
    private int _hp;
    public int hp
    {
        get { return this._hp; }
        set
        {
            this._hp = value.Clamp(0, maxHp);
            EventManager.instance.Emit("hp-change", new PlayerStatusChangeEvent(
                PlayerStatusChangeEventType.HP_CHANGE, this._hp));
        }
    }
    private int _maxHp;
    public int maxHp
    {
        get { return this._maxHp; }
        set
        {
            this._maxHp = Math.Max(value, 0);
            this.hp = Math.Min(this.hp, this._maxHp);
        }
    }

    // Defence
    private int physicalDefence;
    private float physicalResist;
    private int magicalDefence;
    private float magicalResist;

    // inventory TODO

    // Reset is called when the script is attached to a game object
    void Awake()
    {
        this.eventId = 0;

        this.rigidbody = this.GetComponent<Rigidbody2D>();
        this.collider = this.GetComponent<Collider2D>();
        this.transform = this.GetComponent<RectTransform>();
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();

        this.keyUp = KeyCode.W;
        this.keyDown = KeyCode.S;
        this.keyLeft = KeyCode.A;
        this.keyRight = KeyCode.D;
        this.keySprint = KeyCode.LeftShift;

        this.moveLeft = false;
        this.moveRight = false;
        this.jump = false;
        this.drop = true;

        this.facingRight = true;

        this.moveStep = 250;

        this.curJumpNum = 0;
        this.maxJumpNum = 1;
        this.jumpStep = 1000;
        this.maxVertStep = 2500;

        this.allowSprint = true;
        this.sprintStep = 1500;
        this.curSprintTime = 0;
        this.maxSprintTime = 0.15f;
        this.curSprintCD = 0;
        this.maxSprintCD = 2;
        this.sprintingRight = true;

        this.maxHp = 100;
        this.hp = this.maxHp;

        this.physicalDefence = 0;
        this.physicalResist = 2;
        this.magicalDefence = 0;
        this.magicalResist = 1.2f;

        // Attach weapon TODO
    }

    void Start()
    {
        this.eventId = EventManager.instance.On("attack-player",
            (BaseEvent eventArgs) => { this.OnAttacked((AttackPlayerEvent)eventArgs); });
    }

    private void OnDestroy()
    {
        EventManager.instance.Off(this.eventId);
    }

    private void OnAttacked(AttackPlayerEvent eventArgs)
    {
        int damage = 0;
        switch (eventArgs.type)
        {
            case AttackPlayerEventType.PHYSICAL_ATTACK:
                damage = (eventArgs.power > this.physicalDefence) ?
                    (int)Math.Ceiling(this.physicalDefence / (double)this.physicalResist) + eventArgs.power - this.physicalDefence :
                    (int)Math.Ceiling(eventArgs.power / (double)this.physicalResist);
                break;
            case AttackPlayerEventType.MAGICAL_ATTACK:
                damage = (eventArgs.power > this.magicalDefence) ?
                    (int)Math.Ceiling(this.magicalDefence / (double)this.magicalResist) + eventArgs.power - this.magicalDefence :
                    (int)Math.Ceiling(eventArgs.power / (double)this.magicalResist);
                break;
            case AttackPlayerEventType.DIRECT_DAMAGE:
                damage = eventArgs.power;
                break;
        }
        this.hp -= damage;
    }

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
        if (this.allowSprint && Input.GetKeyDown(this.keySprint))
        {
            if (this.curSprintCD == 0 && this.rigidbody.velocity.x != 0)
            {
                this.curSprintTime = this.maxSprintTime;
                this.curSprintCD = this.maxSprintCD;
                this.sprintingRight = this.facingRight;
            }
        }
    }

    // Check if player is on the ground
    private void CheckGrounded()
    {
        Vector2 start = this.transform.GetWorldPointAtBottomLeft().Add(new Vector2(1, -1));
        Vector2 end = this.transform.GetWorldPointByAnchor(new Vector2(1, 0)).Add(new Vector2(-1, 0));
        Collider2D[] colliders = Physics2D.OverlapAreaAll(start, end);
        if (colliders.Length > 1 || colliders.Length == 1 && !"Player".Equals(colliders[0].name))
        {
            this.drop = false;
            this.curJumpNum = this.maxJumpNum;
        }
    }

    // Update all time and cd by subtracting deltaTime
    void UpdateCD()
    {
        curSprintTime = Math.Max(curSprintTime - Time.deltaTime, 0);
        curSprintCD = Math.Max(curSprintCD - Time.deltaTime, 0);
    }

    void Update()
    {
        this.GetKeyboardInput();
        this.CheckGrounded();
        if (curSprintTime > 0)
        {
            int sprintDirection = this.sprintingRight ? 1 : -1;
            this.rigidbody.velocity = new Vector2(this.sprintStep * sprintDirection, 0);
        }
        else
        {
            // calculate x direction velocity
            Vector2 velocity = new Vector2(0, this.rigidbody.velocity.y);
            if (this.moveLeft) velocity.x -= this.moveStep;
            if (this.moveRight) velocity.x += this.moveStep;

            // if jumps (and is able to jump) then set y direction velocity to jump speed
            if (this.jump && this.curJumpNum > 0)
            {
                velocity.y = this.jumpStep;
                this.curJumpNum--;
            }

            // if drop set velocity to maximum pointing downward
            if (this.drop)
            {
                velocity.y = -this.maxVertStep;
            }

            // make sure y direction has speed less than this.verticalMaxStep
            velocity.y = Math.Min(Math.Max(velocity.y, -this.maxVertStep), this.maxVertStep);

            // apply the calculated velocity
            this.rigidbody.velocity = velocity;
        }
        this.UpdateCD();
    }
}
