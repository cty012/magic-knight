using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Settings
{
    public static readonly Settings instance = new Settings();
    public static readonly string defaultSettingsPath = "Assets/Setup/Settings.json";
    public string localStoragePath { get; private set; }
    public string userSettingsPath {
        get
        {
            return this.localStoragePath == null ? null : Path.Combine(this.localStoragePath, "settings.json");
        }
    }

    private JsonObjectNode defaultSettings;
    private JsonObjectNode userSettings;

    private Settings() { }

    public void LoadSettings()
    {
        this.defaultSettings = (JsonObjectNode)JsonParser.Parse(File.ReadAllText(Settings.defaultSettingsPath));

        // Get local storage path
        string localStoragePath = null;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                localStoragePath = this.Get<string>("windows-local-storage");
                break;
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                localStoragePath = this.Get<string>("local-storage");
                break;
        }
        this.localStoragePath = Path.GetFullPath(System.Environment.ExpandEnvironmentVariables(localStoragePath));

        // Create/read user settings
        Utils.CreateFolderIfNotExist(DataManager.instance.savePath);
        Utils.CreateFileIfNotExist(this.userSettingsPath, "{}");
        this.userSettings = (JsonObjectNode)JsonParser.Parse(File.ReadAllText(this.userSettingsPath));
    }

    public T Get<T>(string key)
    {
        this.Get(key, out T result);
        return result;
    }

    public bool Get<T>(string key, out T data)
    {
        // Retrieve the user defined value;
        if (this.userSettings != null)
        {
            Dictionary<string, JsonNode> userValues = this.userSettings.values;
            if (userValues.TryGetValue(key, out JsonNode userDataNode))
            {
                data = ((JsonDataNode<T>)userDataNode).value;
                return true;
            }
        }
        
        // Retrieve the default value;
        if (this.defaultSettings != null)
        {
            Dictionary<string, JsonNode> defaultValues = this.defaultSettings.values;
            if (defaultValues.TryGetValue(key, out JsonNode defaultDataNode))
            {
                data = ((JsonDataNode<T>)defaultDataNode).value;
                return true;
            }
        }

        // Value is not found
        data = default;
        return false;
    }
}
