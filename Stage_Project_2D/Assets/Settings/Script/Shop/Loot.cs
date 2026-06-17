using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Loot : MonoBehaviour
{
    public ItemData item;
    public Sprite sprite;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    
    public int quantity;

    private void Awake()
    {
        SetItemData();
    }

    private void OnValidate()
    {
        SetItemData();
    }

    private void SetItemData()
    {
        if (item == null)
            return;

        sprite = item.icon;
        this.name = item.Name;

        if(nameText != null)
            nameText.SetText(item.Name);

        if(descriptionText != null)
            descriptionText.SetText(item.Description);

        if(priceText != null)
            priceText.SetText(item.price.ToString());
    }
}
