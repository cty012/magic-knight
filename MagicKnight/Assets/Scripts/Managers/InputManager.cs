using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Singleton
    public static InputManager instance { get; private set; }

    // Player control
    public KeyCode keyUp;
    public KeyCode keyDown;
    public KeyCode keyLeft;
    public KeyCode keyRight;
    public KeyCode keySprint;
    public KeyCode keyAttack;

    // UI control
    public KeyCode keyPause;

    // Other control
    public KeyCode keySave;

    private InputManager() { if (InputManager.instance == null) InputManager.instance = this; }

    // Detect input to control UI
    private void ControlUI()
    {
        if (UIManager.instance.uiInventory == null) return;
        if (Input.GetKeyDown(this.keyPause)) GameManager.instance.TogglePause();
    }

    // Detect input to control player
    private void ControlPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;
        PlayerController playerController = player.GetComponent<PlayerController>();

        // Detect orientation change
        if (Input.GetKeyDown(this.keyRight)) playerController.facingRight = true;
        else if (Input.GetKeyDown(this.keyLeft)) playerController.facingRight = false;

        // Detect movement commands
        playerController.moveLeft = Input.GetKey(this.keyLeft);
        playerController.moveRight = Input.GetKey(this.keyRight);
        playerController.jump = Input.GetKeyDown(this.keyUp);
        playerController.drop = Input.GetKeyDown(this.keyDown);
        if (playerController.allowSprint && playerController.sprintCD.stopped && Input.GetKeyDown(this.keySprint) && playerController.rigidbody.velocity.x != 0)
        {
            playerController.sprintTime.Reset();
            playerController.sprintCD.Reset();
            playerController.sprintingRight = playerController.facingRight;
        }

        // Detect attack commands
        if (playerController.weaponController != null && playerController.canAttack && playerController.attackCD.stopped && Input.GetKeyDown(this.keyAttack))
        {
            playerController.weaponController.Attack();
            playerController.attackCD.Reset();
        }
    }

    private void ControlOther()
    {
        if (Input.GetKeyDown(keySave))
        {
            // Iterate checkpoints
            foreach (Transform checkpoint in GameObject.Find("Checkpoint").transform)
            {
                if (checkpoint.GetComponent<CheckpointController>().InRange())
                {
                    DataManager.instance.save["scene"].Set("checkpoint", int.Parse(checkpoint.name));
                    DataManager.instance.SaveToDisk();
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.ControlUI();
        if (!GameManager.instance.paused)
        {
            this.ControlPlayer();
            this.ControlOther();
        }
    }
}
