using System.Collections.Generic;
using UnityEngine;

public static class StoryKeyInventory
{
    private static readonly HashSet<string> ownedKeys = new HashSet<string>();
    private static readonly Dictionary<string, Sprite> keyIcons = new Dictionary<string, Sprite>();

    public static bool HasKey(string keyId)
    {
        return !string.IsNullOrWhiteSpace(keyId) && ownedKeys.Contains(keyId);
    }

    public static void AddKey(string keyId, Sprite icon = null)
    {
        if(string.IsNullOrWhiteSpace(keyId))
            return;

        ownedKeys.Add(keyId);

        if(icon != null)
            keyIcons[keyId] = icon;
    }

    public static bool ConsumeKey(string keyId)
    {
        if(!HasKey(keyId))
            return false;

        ownedKeys.Remove(keyId);
        return true;
    }

    public static Sprite GetKeyIcon(string keyId)
    {
        if(string.IsNullOrWhiteSpace(keyId))
            return null;

        return keyIcons.TryGetValue(keyId, out Sprite icon) ? icon : null;
    }
}
