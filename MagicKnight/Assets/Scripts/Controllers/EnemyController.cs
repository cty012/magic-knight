using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class of all enemies
public abstract class EnemyController : MovableController
{
    // Target
    protected PlayerController playerController;

    // Navigation
    [Header("Navigation Settings")]
    public float horiDetectionDist;
    public float vertDetectionDist;
    public float attackDist;
    public float moveStep;

    // The slime is freezed for a certain amount of time if attacked
    [Header("Freeze Settings")]
    public float freezeResist;
    protected Timer freezeTime;

    protected override void Awake()
    {
        base.Awake();

        // Fields defined in EnemyController
        this.playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        this.freezeTime = new Timer(0, freezeResist);
    }

    public virtual void Freeze(float freezePower)
    {
        this.freezeTime.curValue = freezePower;
    }

    protected override void UpdateTimers()
    {
        if (!this.freezeTime.stopped)
        {
            this.freezeTime.Update();
            return;
        }
        base.UpdateTimers();
    }

    public override bool IsHostile(GameObject obj)
    {
        return Utils.IsPlayer(obj);
    }
}
