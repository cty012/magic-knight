using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public bool isGameScene { get; private set; }
    public bool paused { get; set; }

    private GameManager() { if (GameManager.instance == null) GameManager.instance = this; }

    private void Awake()
    {
        this.ResetValues();
    }

    public void ResetValues()
    {
        this.isGameScene = false;
        this.paused = false;
    }

    public void TogglePause()
    {
        if (!this.isGameScene) return;
        UIManager.instance.uiInventory.SetActive(!UIManager.instance.uiInventory.activeSelf);
        this.paused = UIManager.instance.uiInventory.activeSelf;
    }

    public void LoadScene(string scene, bool isGameScene = false)
    {
        this.ResetValues();
        this.isGameScene = isGameScene;
        // Callbacks
        if (isGameScene)
        {
            SceneManager.sceneLoaded += this.SetupGameScene;
            SceneManager.sceneLoaded += this.OnGameSceneLoaded;
        }
        else
        {
            SceneManager.sceneLoaded += this.OnSceneLoaded;
        }
        // LoadScene
        SceneManager.LoadSceneAsync(scene);
    }

    // Load the scene saved in DataManager.instance.save
    public void LoadSave()
    {
        this.LoadScene(DataManager.instance.save["scene"].Get<string>("name"), true);
    }

    public void SetupGameScene(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= this.SetupGameScene;

        GameObject map = GameObject.Find("Map");
        GameObject player = map.transform.Find("Player").gameObject;

        // Set player position
        int checkpoint = DataManager.instance.save["scene"].Get<int>("checkpoint");
        player.transform.position = map.transform.Find("Checkpoint").Find(checkpoint.ToString()).position;

        // Assign new weapon by reading from the DataManager
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (DataManager.instance.save["player"].Get<string>("weapon", out string weaponName))
        {
            GameObject weapon = Resources.Load<GameObject>("Prefabs/Weapons/" + weaponName);
            GameObject weaponInstance = Instantiate(weapon, playerController.transform);
            playerController.AssignWeapon(weaponInstance.GetComponent<WeaponController>());
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= this.OnSceneLoaded;

        EventManager.instance.Emit("load-scene", new LoadSceneEvent(LoadSceneEventType.LOAD_SCENE, scene, mode));
    }

    public void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= this.OnGameSceneLoaded;

        EventManager.instance.Emit("load-scene", new LoadSceneEvent(LoadSceneEventType.LOAD_GAME_SCENE, scene, mode));
    }

    private void FixedUpdate()
    {
        if (!paused) Physics2D.Simulate(Time.fixedDeltaTime);
    }
}
