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
 *   - backup-saves: a list of backup-saves for storing extra savedfiles.
 *     - 0: same structure as DataManager.save. Can store another version of savedfiles.
 *     - 1
 *     - ...
 *   - temp: record any temperary data. Will be discarded when game session ends.
 */

public class DataManager : MonoBehaviour
{
    public static DataManager instance { get; private set; }

    public readonly DataGroup save = new DataGroup();
    public readonly List<DataGroup> backupSaves = new List<DataGroup>();
    public readonly DataGroup temp = new DataGroup();

    // Save paths
    public string savePath { get { return Path.Combine(Settings.instance.Get<string>("data-path"), "save"); } }
    public string GetSlotPath(int slotNumber)
    {
        return Path.Combine(this.savePath, string.Format("slot{0}.mk", slotNumber.ToString()));
    }

    private DataManager() { DataManager.instance = this; }

    // Load from a save slot to the save DataGroup
    public void LoadFromDisk(int slotNumber)
    {
        this.save.LoadFromDisk(this.GetSlotPath(slotNumber));
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

[System.Serializable]
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
        data = (T)originalData;
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

    /** Note: can only add int, float, string, bool, List<int, float, string, bool>, and Dictionary<int, float, string, and bool> values
     * To represent a data in the json:
     * int/float/string/bool: ["int/float/string/bool", VALUE]
     * List<int/float/string/bool>: [["int/float/string/bool"], VALUE1, VALUE2, ...]
     * Dictionary<int/float/string/bool>: [{"type": "int/float/string/bool"}, {KEY1: VALUE1}, {KEY2: VALUE2}, ...]
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
            // Set the value
            if ("string".Equals(structTypeNode.value)) this.Set(key, ((JsonDataNode<string>)dataNode.values[1]).value);
            else if ("int".Equals(structTypeNode.value)) this.Set(key, (int)((JsonDataNode<float>)dataNode.values[1]).value);
            else if ("float".Equals(structTypeNode.value)) this.Set(key, ((JsonDataNode<float>)dataNode.values[1]).value);
            else if ("bool".Equals(structTypeNode.value)) this.Set(key, ((JsonDataNode<bool>)dataNode.values[1]).value);
        }
        // First is array => the value in the array represent the List generic type
        else if (typeNode is JsonArrayNode listTypeNode)
        {
            // Find the generic type
            string type = ((JsonDataNode<string>)listTypeNode.values[0]).value;
            // Retrieve the values
            List<JsonNode> valueList = dataNode.values.GetRange(1, dataNode.values.Count);
            // Set the values
            if ("string".Equals(type)) this.Set(key, valueList.Select(it => ((JsonDataNode<string>)it).value).ToList());
            else if ("int".Equals(type)) this.Set(key, valueList.Select(it => (int)((JsonDataNode<float>)it).value).ToList());
            else if ("float".Equals(type)) this.Set(key, valueList.Select(it => ((JsonDataNode<float>)it).value).ToList());
            else if ("bool".Equals(type)) this.Set(key, valueList.Select(it => ((JsonDataNode<bool>)it).value).ToList());
        }
        // First is object => the value correspond to the key "type" represent the Dictionary's second generic type (first is string)
        else if (typeNode is JsonObjectNode dictionaryTypeNode)
        {
            // Find the generic type
            string type = ((JsonDataNode<string>)dictionaryTypeNode.values["type"]).value;
            // Retrieve the values
            List<JsonObjectNode> valueList = dataNode.values.GetRange(1, dataNode.values.Count - 1).Select(it => (JsonObjectNode)it).ToList();
            IDictionary dictionary = null;
            if ("string".Equals(type))
            {
                dictionary = new Dictionary<string, string>();
                foreach (JsonObjectNode pairNode in valueList) dictionary[pairNode.values.Keys.ToList()[0]] = ((JsonDataNode<string>)pairNode.values.Values.ToList()[0]).value;
            }
            else if ("int".Equals(type))
            {
                dictionary = new Dictionary<string, int>();
                foreach (JsonObjectNode pairNode in valueList) dictionary[pairNode.values.Keys.ToList()[0]] = (int)((JsonDataNode<float>)pairNode.values.Values.ToList()[0]).value;
            }
            else if ("float".Equals(type))
            {
                dictionary = new Dictionary<string, float>();
                foreach (JsonObjectNode pairNode in valueList) dictionary[pairNode.values.Keys.ToList()[0]] = ((JsonDataNode<float>)pairNode.values.Values.ToList()[0]).value;
            }
            else if ("bool".Equals(type))
            {
                dictionary = new Dictionary<string, bool>();
                foreach (JsonObjectNode pairNode in valueList) dictionary[pairNode.values.Keys.ToList()[0]] = ((JsonDataNode<bool>)pairNode.values.Values.ToList()[0]).value;
            }
            this.datas[key] = dictionary;
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
        if (value is IList listValue)
        {
            JsonArrayNode typeNode = new JsonArrayNode("[\"" + GetListType(listValue) + "\"]");
            dataNode.values.Add(typeNode);
            // Add the values
            foreach (object eachValue in listValue)
                dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format((eachValue is string) ? "\"{0}\"" : "{0}", eachValue.ToString())));
        }
        else if (value is IDictionary dictValue)
        {
            JsonObjectNode typeNode = new JsonObjectNode("{\"type\":\"" + GetDictionaryType(dictValue) + "\"}");
            dataNode.values.Add(typeNode);
            // Add the values
            foreach (object pair in dictValue)
            {
                if (pair is KeyValuePair<string, int> intPair)
                    dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format("{0}", intPair.ToString())));
                if (pair is KeyValuePair<string, float> floatPair)
                    dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format("{0}", floatPair.ToString())));
                if (pair is KeyValuePair<string, bool> boolPair)
                    dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format("{0}", boolPair.ToString())));
                if (pair is KeyValuePair<string, string> stringPair)
                    dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format("\"{0}\"", stringPair.ToString())));
            }
        }
        else
        {
            JsonDataNode<string> typeNode = new JsonDataNode<string>("\"" + GetType(value) + "\"}");
            dataNode.values.Add(typeNode);
            // Add the value
            dataNode.values.Add(Json.JsonNodeFactory.GetJsonNode(string.Format((value is string) ? "\"{0}\"" : "{0}", value.ToString())));
        }
        return dataNode;

        // Helper local functions
        static string GetType(object value)
        {
            if (value is int) return "int";
            else if (value is float) return "float";
            else if (value is bool) return "bool";
            else if (value is string) return "string";
            return null;
        }

        static string GetListType(IList e)
        {
            if (e is List<int>) return "int";
            else if (e is List<float>) return "float";
            else if (e is List<bool>) return "bool";
            else if (e is List<string>) return "string";
            return null;
        }

        static string GetDictionaryType(IDictionary e)
        {
            if (e is Dictionary<string, int>) return "int";
            else if (e is Dictionary<string, float>) return "float";
            else if (e is Dictionary<string, bool>) return "bool";
            else if (e is Dictionary<string, string>) return "string";
            return null;
        }
    }
}
