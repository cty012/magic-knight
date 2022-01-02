using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/** DataManager
 * DataManager works like a folder-file system
 * DataManager contains lots of DataGroups which are the "folders"
 * DataGroups contains lots of DataGroups (folders) and Data (files)
 * DataGroups can be accessed with [] (e.g. DataManager.instance["path"]["to"]["my"]["data-group"])
 * Data can be accessed with DataGroup.Get<T> (e.g. dataGroup.Get<string>("a-saved-string"))
 * To create a new DataGroup in the hierarchy: DataManager.instance["my"]["new"]["data-group"] = new DataGroup();
 * Names of data groups and data should be "lower-case-connected-with-hyphens"
 * The same name cannot be used for naming two folders and files
 * 
 * General structure:
 * DataManager
 *   - globalSave: saved info applies for all saved files (e.g. which slot was played last time)
 *     - slot: the slot number currently playing
 *     - ...
 *   - save: this data group is for loading and saving game data on the disk.
 *     [WARNING] Although not compulsory, this section only allows data of type:
 *        int/float/bool/string
 *        List<int/float/bool/string>
 *        Dictionary<int/float/bool/string>
 *     Otherwise the data will not be saved on disk.
 *     - player: Player information
 *       - attr: attributes of player (hp, mp, xp, etc.)
 *       - weapon: weapon name
 *       - inventory: details of what the player have in the bag
 *       - ...
 *     - npc: records all npc states. Useful for recording storyline progression
 *       - narrator: the narrator is in charge of the pop-ups
 *       - ...
 *     - scene: records the location of the Player
 *       - name: scene name
 *       - checkpoint: the id number of the checkpoint that the player saved at
 *       - ...
 *     - ...
 *   - backupSaves: a list of backup-saves for storing extra savedfiles.
 *     - 0: same structure as DataManager.save. Can store another version of savedfiles.
 *     - 1
 *     - ...
 *   - temp: record any temperary data. Will be discarded when game session ends.
 */

public class DataManager : MonoBehaviour
{
    public static DataManager instance { get; private set; }

    public readonly DataGroup globalSave = new DataGroup();
    public readonly DataGroup save = new DataGroup();
    public readonly List<DataGroup> backupSaves = new List<DataGroup>();
    public readonly DataGroup temp = new DataGroup();

    // Save paths
    public string savePath { get { return Path.Combine(Settings.instance.localStoragePath, "save"); } }
    public string GetSlotPath(int slotNumber)
    {
        return Path.Combine(this.savePath, string.Format("slot{0}.json", slotNumber.ToString()));
    }

    private DataManager() { if (DataManager.instance == null) DataManager.instance = this; }

    // Load the global save
    public void LoadGlobalSave()
    {
        Utils.CreateFileIfNotExist(Path.Combine(this.savePath, "global.json"), "{}");
        this.globalSave.LoadFromDisk(Path.Combine(this.savePath, "global.json"));
    }

    public bool SlotExists(int slotNumber)
    {
        return File.Exists(this.GetSlotPath(slotNumber));
    }

    // Load from a save slot to the save DataGroup
    public void LoadFromDisk(int slotNumber)
    {
        if (!Utils.CreateFileIfNotExist(this.GetSlotPath(slotNumber)))
        {
            this.save.LoadFromDisk(this.GetSlotPath(slotNumber));
        }
        else
        {
            this.save.CreateBasicSave();
            this.save.SaveToDisk(this.GetSlotPath(slotNumber));
        }
        this.temp["current-game", true].Set("slot", slotNumber);
    }

    public void SaveToDisk()
    {
        if (this.temp["current-game", true].Get("slot", out int slotNumber))
        {
            this.save.SaveToDisk(this.GetSlotPath(slotNumber));
        }
    }

    public void SaveToDisk(int slotNumber)
    {
        this.save.SaveToDisk(this.GetSlotPath(slotNumber));
    }

    // Copy the save DataGroup to a backup DataGroup
    public void SaveBackup(int backupNumber)
    {
        this.backupSaves[backupNumber].Copy(this.save);
    }

    // Copy a backup DataGroup to the save DataGroup
    public void LoadBackup(int backupNumber)
    {
        this.save.Copy(this.backupSaves[backupNumber]);
    }
}

public class DataGroup
{
    private Dictionary<string, DataGroup> dataGroups;
    private Dictionary<string, object> datas;

    public DataGroup()
    {
        this.Clear();
    }

    public DataGroup this[string groupName, bool createIfNotExist = false]
    {
        get
        {
            this.dataGroups.TryGetValue(groupName, out DataGroup dataGroup);
            if (createIfNotExist && dataGroup == null) {
                dataGroup = new DataGroup();
                this.dataGroups[groupName] = dataGroup;
            }
            return dataGroup;
        }
        set
        {
            this.dataGroups[groupName] = value;
        }
    }

    // Getting and Setting Data
    public T Get<T>(string key)
    {
        this.Get(key, out T result);
        return result;
    }

    public bool Get<T>(string dataName, out T data)
    {
        bool result = this.datas.TryGetValue(dataName, out object originalData);
        data = originalData is T ? (T)originalData : default(T);
        return result;
    }

    public void Set<T>(string dataName, T data)
    {
        this.datas[dataName] = data;
    }

    // Clear
    public void Clear()
    {
        this.dataGroups = new Dictionary<string, DataGroup>();
        this.datas = new Dictionary<string, object>();
    }

    // Copy the contents of another DataGroup
    public DataGroup Copy(DataGroup other)
    {
        this.Clear();
        foreach (var item in other.dataGroups)
        {
            this.dataGroups[item.Key] = new DataGroup().Copy(other);
        }
        foreach (var item in other.datas)
        {
            this.datas[item.Key] = item.Value;
        }
        return this;
    }

    // Save data to the disk
    public DataGroup SaveToDisk(string path)
    {
        File.WriteAllText(path, this.ToJsonNode().GetDecoratedJsonString());
        return this;
    }

    // Load an existing save from the disk
    public DataGroup LoadFromDisk(string path)
    {
        string jsonString = File.ReadAllText(path);
        return this.FromJsonNode((JsonObjectNode)JsonParser.Parse(jsonString));
    }

    // Create a initial file that starts from the beginning of the game
    public DataGroup CreateBasicSave()
    {
        return LoadFromDisk("Assets/Setup/InitSave.json");
    }

    /** Note: can only add int/float/string/bool, List<int/float/string/bool>, and Dictionary<int/float/string/bool, int/float/string/bool> values
     * To represent a data in the json:
     * int/float/string/bool: ["int/float/string/bool", VALUE]
     * List<int/float/string/bool>: [["int/float/string/bool"], VALUE1, VALUE2, ...]
     * Dictionary<int/float/string/bool>: [{"int/float/string/bool": "int/float/string/bool"}, [KEY1, VALUE1], [KEY2: VALUE2], ...]
     * For more information, see Assets/Resources/Setup/InitSave.json
     */
    public DataGroup FromJsonNode(JsonObjectNode jsonNode)
    {
        this.Clear();
        foreach (var pair in jsonNode.values)
        {
            // [] represent data
            if (pair.Value is JsonArrayNode dataNode) this.ParseData(pair.Key, dataNode);
            // {} represent folders
            else if (pair.Value is JsonObjectNode folderNode) this[pair.Key] = new DataGroup().FromJsonNode(folderNode);
        }
        return this;
    }

    // Helper method for reading json data
    private void ParseData(string key, JsonArrayNode dataNode)
    {
        JsonNode typeNode = dataNode.values[0];
        // Identify the type of data by inspecting the first term
        // First is string => represent the type of the value
        if (typeNode is JsonDataNode<string> structTypeNode)
        {
            // Find the generic type
            Type type = this.StringToType(structTypeNode.value);
            // Set the value
            this.Set(key, Convert.ChangeType(((IJsonDataNode)dataNode.values[1]).GetValue(), type));
        }
        // First is array => the value in the array represent the List generic type
        else if (typeNode is JsonArrayNode listTypeNode)
        {
            // Find the generic type
            Type type = this.StringToType(((JsonDataNode<string>)listTypeNode.values[0]).value);

            // Retrieve the values
            List<JsonNode> valueList = dataNode.values.GetRange(1, dataNode.values.Count - 1);

            // Set the values
            IList target = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
            foreach (IJsonDataNode item in valueList) target.Add(Convert.ChangeType(item.GetValue(), type));
            this.Set(key, target);
        }
        // First is object => the value correspond to the key "type" represent the Dictionary's second generic type (first is string)
        else if (typeNode is JsonObjectNode dictionaryTypeNode)
        {
            // Find the generic types
            string keyTemp = dictionaryTypeNode.values.Keys.First();
            Type typeK = this.StringToType(keyTemp);
            Type typeV = this.StringToType(((JsonDataNode<string>)dictionaryTypeNode.values[keyTemp]).value);

            // Retrieve the values
            List<JsonArrayNode> valueList = dataNode.values
                .GetRange(1, dataNode.values.Count - 1)
                .Select(it => (JsonArrayNode)it).ToList();

            // Set the values
            IDictionary target = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeK, typeV));
            foreach (JsonArrayNode pairNode in valueList)
            {
                object dataKey = Convert.ChangeType(((IJsonDataNode)pairNode.values[0]).GetValue(), typeK);
                object dataValue = Convert.ChangeType(((IJsonDataNode)pairNode.values[1]).GetValue(), typeV);
                target[dataKey] = dataValue;
            }
            this.Set(key, target);
        }
    }

    // Convert the DataGroup into a JsonNode
    public JsonNode ToJsonNode()
    {
        JsonObjectNode node = new JsonObjectNode();
        // Include all the data
        foreach (var pair in this.datas) node.values[pair.Key] = this.GetData(pair.Value);
        // Include all the dataGroups
        foreach (var pair in this.dataGroups) node.values[pair.Key] = (pair.Value.ToJsonNode());
        return node;
    }

    // Helper method for converting the data part to JsonNode
    private JsonArrayNode GetData(object value)
    {
        JsonArrayNode dataNode = new JsonArrayNode();
        // Find the type of the object
        // 1. List
        if (value is IList list)
        {
            Type listGenericType = list.GetType().GetGenericTypeDefinition().GetGenericArguments()[0];
            JsonArrayNode typeNode = new JsonArrayNode("[\"" + TypeToString(listGenericType) + "\"]");
            dataNode.values.Add(typeNode);
            // Add the values
            foreach (object eachValue in list)
                dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format((eachValue is string) ? "\"{0}\"" : "{0}", eachValue.ToString())));
        }
        // 2. Dictionary
        else if (value is IDictionary dict)
        {
            Type typeKey = dict.GetType().GetGenericArguments()[0];
            Type typeValue = dict.GetType().GetGenericArguments()[1];
            JsonObjectNode typeNode = new JsonObjectNode("{\"" + TypeToString(typeKey) + "\":\"" + TypeToString(typeValue) + "\"}");
            dataNode.values.Add(typeNode);
            // Add the values
            foreach (object dictKey in dict.Keys)
            {
                dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format("[{0},{1}]",
                    (dictKey is string) ? "\"" + dictKey + "\"" : dictKey.ToString(),
                    (dict[dictKey] is string) ? "\"" + dict[dictKey] + "\"" : dict[dictKey].ToString())));
            }
        }
        // 3. Value
        else
        {
            Type type = value.GetType();
            JsonDataNode<string> typeNode = new JsonDataNode<string>(string.Format("\"{0}\"", TypeToString(type)));
            dataNode.values.Add(typeNode);
            // Add the value
            dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format((value is string) ? "\"{0}\"" : "{0}", value.ToString())));
        }
        return dataNode;
    }

    private Type StringToType(string input)
    {
        if ("int".Equals(input)) return typeof(int);
        else if ("float".Equals(input)) return typeof(float);
        else if ("bool".Equals(input)) return typeof(bool);
        else if ("string".Equals(input)) return typeof(string);
        else return null;
    }

    private string TypeToString(Type input)
    {
        if (typeof(int).Equals(input)) return "int";
        else if (typeof(float).Equals(input)) return "float";
        else if (typeof(bool).Equals(input)) return "bool";
        else if (typeof(string).Equals(input)) return "string";
        else return null;
    }
}
