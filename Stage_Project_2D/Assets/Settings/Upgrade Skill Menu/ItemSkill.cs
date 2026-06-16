using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_Skill", menuName = "Item Skill")] 

public class ItemSkill : ScriptableObject
{
    [Header("Item Info")]
    public string ItemName;
    [TextArea] public string description;
    public Sprite icon;
    public int price;

    [Header("Item Stats")]
    public string statToIncrease;
    public float statIncreaseAmount;
}
