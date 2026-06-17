using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public ItemData item;
    public SpriteRenderer sr;
    
    public int quantity;

    private void OnValidate()
    {
        if (item == null)
            return;

        sr.sprite = item.icon;
        this.name = item.name;
    }
}
