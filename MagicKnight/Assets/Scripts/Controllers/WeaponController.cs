using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The base script for weapons
public abstract class WeaponController : MonoBehaviour
{
    // The object using the weapon
    public MovableController userController { get; set; }

    // Useful components
    public new RectTransform transform { get; private set; }
    public new Rigidbody2D rigidbody { get; private set; }
    public new Collider2D collider { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }

    // Status
    protected bool _isAttacking;
    protected virtual bool isAttacking
    {
        get { return this._isAttacking; }
        set
        {
            this._isAttacking = value;
            if (!value) this.attackedObjects.Clear();
        }
    }
    private bool _facingRight;
    public bool facingRight
    {
        get { return this._facingRight; }
        set
        {
            this._facingRight = value;
            this.spriteRenderer.flipX = !value;
        }
    }

    // Objects already attacked
    protected List<GameObject> attackedObjects;

    // Power
    public int attackPower;
    public int pushPower;
    public float freezePower;

    protected virtual void Awake()
    {
        this.transform = (RectTransform)this.gameObject.transform;
        this.rigidbody = this.GetComponent<Rigidbody2D>();
        this.collider = this.GetComponent<Collider2D>();
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();

        this.isAttacking = false;
        this.facingRight = true;

        this.attackedObjects = new List<GameObject>();
    }

    protected virtual void Start()
    {
        // Intend to get derived classes of MovableController such as PlayerController or EnemyController
        this.transform.parent?.gameObject?.GetComponent<MovableController>()?.AssignWeapon(this);
    }

    // Callback of collision start
    protected virtual void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (this.isAttacking && this.userController != null && this.userController.IsHostile(otherCollider.gameObject) && !this.attackedObjects.Contains(otherCollider.gameObject))
        {
            MovableController controller = otherCollider.gameObject.GetComponent<MovableController>();
            if (controller == null) return;

            // Push the hostile target
            int direction = (int)Utils.GetDirection(
                this.userController.transform.position.DropZ(),
                controller.transform.position.DropZ()).x;
            controller.OnPushed(PushType.NORMAL_PUSH, this.pushPower * direction);
        }
    }

    // Callback of collision update
    protected virtual void OnTriggerStay2D(Collider2D otherCollider)
    {
        if (this.isAttacking && this.userController != null && this.userController.IsHostile(otherCollider.gameObject) && !this.attackedObjects.Contains(otherCollider.gameObject))
        {
            MovableController controller = otherCollider.gameObject.GetComponent<MovableController>();
            if (controller == null) return;

            // Attack the hostile target
            controller.OnAttacked(AttackType.PHYSICAl_ATTACK, this.attackPower);
            
            // Freeze the hostile target if it is an Enemy
            if (controller is EnemyController enemyController) enemyController.Freeze(this.freezePower);

            // Add the hostile target to the list to avoid inflicting further damage
            this.attackedObjects.Add(otherCollider.gameObject);
        }
    }
    
    // Execute the attack command
    public abstract void Attack();

    // Update the postion of the weapon
    protected abstract void Move();

    protected virtual void UpdateTimers() { }

    protected virtual void Update()
    {
        this.UpdateTimers();
        this.Move();
    }
}

public enum AttackType
{
    PHYSICAl_ATTACK,
    MAGICAL_ATTACK,
    DIRECT_DAMAGE  // ignore defence
}

public enum PushType
{
    NORMAL_PUSH,
    DIRECT_PUSH  // ignore defence
}
