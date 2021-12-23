using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class of all movable characters
public abstract class MovableController : MonoBehaviour
{
    // Useful components
    public new RectTransform transform { get; protected set; }
    public new Rigidbody2D rigidbody { get; protected set; }
    public new Collider2D collider { get; protected set; }
    public SpriteRenderer spriteRenderer { get; protected set; }

    // Weapon
    public WeaponController weaponController { get; set; }
    protected Vector2 weaponAnchor;
    protected Vector2 weaponAnchorReversed
    {
        get { return new Vector2(1 - weaponAnchor.x, weaponAnchor.y); }
    }

    // Orientation (always face right at the beginning)
    private bool _facingRight;
    public bool facingRight
    {
        get { return this._facingRight; }
        protected set
        {
            this._facingRight = value;
            this.spriteRenderer.flipX = !value;
            if (this.weaponController != null)
            {
                ((RectTransform)this.weaponController.transform).anchorMin = value ? this.weaponAnchor.Clone() : this.weaponAnchorReversed.Clone();
                ((RectTransform)this.weaponController.transform).anchorMax = value ? this.weaponAnchor.Clone() : this.weaponAnchorReversed.Clone();
                this.weaponController.facingRight = value;
            }
        }
    }

    // Whether object is on the ground
    protected bool onGround;

    // HP (NEED TO CUSTOMIZE ALL FIElDS IN THIS SECTION)
    private int _hp;
    public virtual int hp
    {
        get { return this._hp; }
        set
        {
            this._hp = value.Clamp(0, maxHp);
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

    [Header("Health Settings")]
    public int maximumHp;

    // Defence (NEED TO CUSTOMIZE ALL FIElDS IN THIS SECTION)
    [Header("Defence Settings")]
    public int physicalDefence;
    public float physicalResist;
    public int magicalDefence;
    public float magicalResist;

    [Header("Knockback Settings")]
    public float knockbackResist;
    public float knockbackDecay;
    protected Timer knockback;

    // Reset is called when the script is attached to a game object
    protected virtual void Awake()
    {
        this.transform = (RectTransform)this.gameObject.transform;
        this.rigidbody = this.GetComponent<Rigidbody2D>();
        this.collider = this.GetComponent<Collider2D>();
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();

        this.weaponAnchor = new Vector2(1, 0.5f);

        this.facingRight = true;

        this.onGround = false;

        this.hp = this.maximumHp;
        this.maxHp = this.maximumHp;

        this.knockback = new Timer(0, this.knockbackDecay);
    }

    // Assign a weapon to the character
    public virtual void AssignWeapon(WeaponController controller, bool destroyCurrentWeapon = true)
    {
        if (destroyCurrentWeapon) Destroy(this.weaponController?.gameObject);
        // If controller is null just unassign current weapon
        if (controller == null)
        {
            this.weaponController = null;
            return;
        }

        // Monogamy
        if (controller?.userController != null) controller.userController.weaponController = null;

        // Create link
        this.weaponController = controller;
        controller.userController = this;

        // Update facingRight
        this.facingRight = true;
    }

    // Check if a game object is hostile
    public abstract bool IsHostile(GameObject obj);

    // Called when object is being attacked
    public virtual void OnAttacked(AttackType type, int power)
    {
        int damage = 0;
        switch (type)
        {
            case AttackType.PHYSICAl_ATTACK:
                damage = (power > this.physicalDefence) ?
                    (int)Math.Ceiling(this.physicalDefence / (double)this.physicalResist) + power - this.physicalDefence :
                    (int)Math.Ceiling(power / (double)this.physicalResist);
                break;
            case AttackType.MAGICAL_ATTACK:
                damage = (power > this.magicalDefence) ?
                    (int)Math.Ceiling(this.magicalDefence / (double)this.magicalResist) + power - this.magicalDefence :
                    (int)Math.Ceiling(power / (double)this.magicalResist);
                break;
            case AttackType.DIRECT_DAMAGE:
                damage = power;
                break;
        }
        this.hp -= damage;
    }

    // Called when object is being pushed
    public virtual void OnPushed(PushType type, int power)
    {
        switch (type)
        {
            case PushType.NORMAL_PUSH:
                this.knockback.curValue = power / this.knockbackResist;
                break;
            case PushType.DIRECT_PUSH:
                this.knockback.curValue = power;
                break;
        }
    }

    // Check if player is on the ground
    protected virtual void CheckOnGround()
    {
        Vector2 start = this.transform.GetWorldPointAtBottomLeft().Add(new Vector2(1, -1));
        Vector2 end = this.transform.GetWorldPointByAnchor(new Vector2(1, 0)).Add(new Vector2(-1, 0));
        Collider2D[] colliders = Physics2D.OverlapAreaAll(start, end);
        this.onGround = colliders.Length > 1 || colliders.Length == 1 && !this.gameObject.name.Equals(colliders[0].name);
    }

    // Update all time and cd by subtracting deltaTime
    protected virtual void UpdateTimers()
    {
        this.knockback.Update();
    }

    // Defines the behavior of the object
    protected abstract void Move();

    protected virtual void Update()
    {
        this.UpdateTimers();
        this.CheckOnGround();
        this.Move();
    }
}
