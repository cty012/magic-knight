using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Important game objects
    private GameObject map;
    public GameObject target { get; set; }
    private float speed;
    void Awake()
    {
        this.map = GameObject.Find("Map");
        this.target = GameObject.Find("Player");
        this.speed = 4;
    }

    void Start()
    {
        // Instantly teleport to player's position
        Vector2 bottomLeft = Utils.size.MultiplyScalar(0.5f);
        Vector2 topRight = ((RectTransform) this.map.transform).rect.size.Subtract(bottomLeft);
        Vector2 targetPosition = this.GetTargetPosition();
        Vector2 targetPositionClamped = new Vector2(
            targetPosition.x.Clamp(bottomLeft.x, topRight.x),
            targetPosition.y.Clamp(bottomLeft.y, topRight.y));
        this.gameObject.transform.position = targetPositionClamped.AttachZ(this.gameObject.transform.position.z);
    }

    // Get the position of the player
    private Vector2 GetTargetPosition()
    {
        return this.target.transform.localPosition.DropZ().Clone();
    }

    void Update()
    {
        // make clones because we do not want to make change to the original value
        float z = this.gameObject.transform.position.z;
        Vector2 currentPosition = this.gameObject.transform.position.DropZ().Clone();

        // make the camera move towards player's position at a ratio determined by dt
        // clamp AFTER the lerp to get better effect
        Vector2 bottomLeft = Utils.size.MultiplyScalar(0.5f);
        Vector2 topRight = ((RectTransform)this.map.transform).rect.size.Subtract(bottomLeft);
        Vector2 nextPosition = Vector2.Lerp(currentPosition, this.GetTargetPosition(), this.speed * Time.deltaTime);
        Vector2 nextPositionClamped = new Vector2(
            nextPosition.x.Clamp(bottomLeft.x, topRight.x),
            nextPosition.y.Clamp(bottomLeft.y, topRight.y));

        // update the position
        this.gameObject.transform.position = nextPositionClamped.AttachZ(z);
    }
}
