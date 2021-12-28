using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private GameManager()
    {
        GameManager.instance = this;
    }

    public void LoadScene(string scene, bool isGameScene)
    {
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

    public void SetupGameScene(Scene scene, LoadSceneMode mode)
    {
        // Assign new weapon by reading from the DataManager
        PlayerController playerController = GameObject.Find("Player").GetComponent<PlayerController>();
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

    // TEMPERARY
    private void DetectChangeScene()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) this.LoadScene("StartMenu", false);
        else if (Input.GetKeyDown(KeyCode.Alpha1)) this.LoadScene("Map1", true);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) this.LoadScene("Map2", true);
    }

    // TEMPARARY
    private void Update()
    {
        this.DetectChangeScene();
    }
}
