using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item")] 

public class ItemData : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public int price;
    public Sprite icon;
}
