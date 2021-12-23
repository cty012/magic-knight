using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : CharacterController
{
    // Target
    protected PlayerController playerController;

    // Positioning
    protected float horiDetectionDist;
    protected float vertDetectionDist;
    protected float attackDist;
    protected float moveStep;

    protected override void Awake()
    {
        base.Awake();

        // Fields defined in EnemyController
        this.playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    public override bool IsHostile(GameObject obj)
    {
        return Utils.IsPlayer(obj);
    }
}
