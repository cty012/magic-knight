using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Useful components
    public new Rigidbody2D rigidbody { get; private set; }
    public new Collider2D collider { get; private set; }
    public new RectTransform transform { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    // Child nodes
    public GameObject weapon { get; set; }

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
    public bool facingRight {
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
    public int hp { get; set; }
    public int maxHp { get; set; }

    // inventory TODO

    // Reset is called when the script is attached to a game object
    void Awake()
    {
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

        this.moveStep = 250.0f;

        this.curJumpNum = 0;
        this.maxJumpNum = 1;
        this.jumpStep = 1000.0f;
        this.maxVertStep = 2500.0f;

        this.allowSprint = true;
        this.sprintStep = 1200.0f;
        this.curSprintTime = 0.0f;
        this.maxSprintTime = 0.20f;
        this.curSprintCD = 0.0f;
        this.maxSprintCD = 2.0f;
        this.sprintingRight = true;

        this.maxHp = 100;
        this.hp = this.maxHp;

        // Attach weapon TODO
    }

    void Start()
    {
        // Register event listeners

    }

    private void OnDestroy()
    {
        // Unregister the event listeners
    }

    // OnAttacked TODO

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
        // TODO: add get rect to the utils
        Vector2 start = new Vector2(
            this.transform.rect.xMin * this.transform.localScale.x + this.transform.position.x,
            this.transform.rect.yMin * this.transform.localScale.y + this.transform.position.y - 0.2f
        );
        Vector2 end = new Vector2(start.x + this.transform.localScale.x, start.y + 0.1f);
        if (Physics2D.OverlapArea(start, end))
        {
            this.drop = false;
            this.curJumpNum = this.maxJumpNum;
        }
    }

    // Update all time and cd by subtracting deltaTime
    void UpdateCD()
    {
        curSprintTime = System.Math.Max(curSprintTime - Time.deltaTime, 0);
        curSprintCD = System.Math.Max(curSprintCD - Time.deltaTime, 0);
    }

    void Update()
    {
        this.GetKeyboardInput();
        this.CheckGrounded();
        if (curSprintTime > 0)
        {
            int sprintDirection = this.sprintingRight ? 1 : -1;
            this.rigidbody.velocity = new Vector2(this.sprintStep * sprintDirection, 0.0f);
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
            velocity.y = System.Math.Min(System.Math.Max(velocity.y, -this.maxVertStep), this.maxVertStep);

            // apply the calculated velocity
            this.rigidbody.velocity = velocity;
        }
        this.UpdateCD();
    }
}
