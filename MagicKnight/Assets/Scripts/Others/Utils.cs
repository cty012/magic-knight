using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utils
{
    private static readonly object idLock = new object();
    private static int nextId = 1;
    public static readonly Vector2 size = new Vector2(1920, 1080);

    // Generate an id unique in the game session
    public static int GenerateId()
    {
        lock (idLock)
        {
            return Utils.nextId++;
        }
    }

    // Get the rough direction (1, -1, or 0 for each dimension) of two points
    public static Vector2 GetDirection(Vector2 from, Vector2 to)
    {
        return new Vector2(
            (from.x == to.x) ? 0 : ((from.x < to.x) ? 1 : -1),
            (from.y == to.y) ? 0 : ((from.y < to.y) ? 1 : -1));
    }

    // Identify game objects
    public static bool IsPlayer(GameObject obj)
    {
        return obj.GetComponent<PlayerController>() != null;
        // return "Player".Equals(obj?.name);
    }
    public static bool IsEnemy(GameObject obj)
    {
        return obj.GetComponent<EnemyController>() != null;
        // return "Enemy".Equals(obj?.transform?.parent?.parent?.gameObject?.name);
    }
    public static bool IsNPC(GameObject obj)
    {
        return "NPC".Equals(obj?.transform?.parent?.parent?.gameObject?.name);
    }
    public static bool IsTerrain(GameObject obj)
    {
        return "Terrain".Equals(obj?.transform?.parent?.gameObject?.name);
    }
    // File system
    public static bool CreateFolderIfNotExist(string path)
    {
        path = Path.GetFullPath(path);
        if (Directory.Exists(path)) return false;
        // If not exist, check if parent folder exist
        DirectoryInfo parent = Directory.GetParent(path);
        if (!parent.Exists) Utils.CreateFolderIfNotExist(parent.FullName);
        Directory.CreateDirectory(path);
        return true;
    }
    public static bool CreateFileIfNotExist(string path, string contents = "")
    {
        path = Path.GetFullPath(path);
        if (File.Exists(path)) return false;
        // If not exist, check if parent folder exist
        DirectoryInfo parent = Directory.GetParent(path);
        if (!parent.Exists) Utils.CreateFolderIfNotExist(parent.FullName);
        using (StreamWriter sw = File.CreateText(path))
        {
            sw.Write(contents);
        }
        return true;
    }
}
