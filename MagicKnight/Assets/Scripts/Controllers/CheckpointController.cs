using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    // Check if the player is inside the checkpoint save zone
    public bool InRange()
    {
        return this.InRange(GameObject.Find("Player"));
    }

    // Check if a point is inside the checkpoint save zone
    public bool InRange(GameObject target)
    {
        return ((RectTransform)this.transform).rect.Contains(target.transform.position.DropZ());
    }
}
