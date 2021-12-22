using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerEvent : BaseEvent
{
    public readonly int power;

    AttackPlayerEvent(AttackPlayerEventType type, int power) : base(type)
    {
        this.power = power;
    }
}

public enum AttackPlayerEventType
{
    PHYSICAL_ATTACK,
    MAGICAL_ATTACK,
    DIRECT_DAMAGE
}
