using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Important game objects
    private GameObject map;
    public GameObject target { get; set; }
    // The speed at which the camera moves
    private float speed;

    private void Awake()
    {
        this.map = GameObject.Find("Map");
        this.target = GameObject.Find("Player");
        this.speed = 4;
    }

    private void Start()
    {
        // Find the target position
        Vector2 targetPosition = this.GetTargetPosition();

        // Clamp to make sure camera does not move outside of the map
        Vector2 bottomLeft = Utils.size.MultiplyScalar(0.5f);
        Vector2 topRight = ((RectTransform) this.map.transform).rect.size.Subtract(bottomLeft);
        Vector2 targetPositionClamped = new Vector2(
            targetPosition.x.Clamp(bottomLeft.x, topRight.x),
            targetPosition.y.Clamp(bottomLeft.y, topRight.y));

        // Instantly teleport to player's position
        this.gameObject.transform.position = targetPositionClamped.AttachZ(this.gameObject.transform.position.z);
    }

    // Get the position of the player
    private Vector2 GetTargetPosition()
    {
        return this.target.transform.localPosition.DropZ().Clone();
    }

    private void Update()
    {
        // Make clones because we do not want to make change to the original value
        float z = this.gameObject.transform.position.z;
        Vector2 currentPosition = this.gameObject.transform.position.DropZ().Clone();

        // Make the camera move towards player's position at a ratio determined by deltaTime
        Vector2 nextPosition = Vector2.Lerp(currentPosition, this.GetTargetPosition(), this.speed * Time.deltaTime);
        
        // Clamp AFTER the lerp to get better effect
        Vector2 bottomLeft = Utils.size.MultiplyScalar(0.5f);
        Vector2 topRight = ((RectTransform)this.map.transform).rect.size.Subtract(bottomLeft);
        Vector2 nextPositionClamped = new Vector2(
            nextPosition.x.Clamp(bottomLeft.x, topRight.x),
            nextPosition.y.Clamp(bottomLeft.y, topRight.y));

        // Update the position of the camera
        this.gameObject.transform.position = nextPositionClamped.AttachZ(z);
    }
}
