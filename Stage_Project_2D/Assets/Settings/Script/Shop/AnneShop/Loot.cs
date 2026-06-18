using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Loot : MonoBehaviour
{
    public ItemData item;
    public Canvas shop;
    public Sprite sprite;
    
    public int quantity;

    private TMP_Text nameText;
    private TMP_Text descriptionText;
    private TMP_Text priceText;
    private Image iconImage;
    private Transform itemRoot;

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

        FindShopReferences();

        sprite = item.icon;

        if(nameText != null)
            nameText.SetText(item.Name);

        if(descriptionText != null)
            descriptionText.SetText(item.Description);

        if(priceText != null)
            priceText.SetText(item.price.ToString());

        if(iconImage != null)
            iconImage.sprite = item.icon;
    }

    public bool Consume(PlayerMovement player, MonoBehaviour coroutineRunner)
    {
        if(item == null)
            return false;

        return item.Consume(player, coroutineRunner);
    }

    private void FindShopReferences()
    {
        if(shop == null)
            shop = GetComponentInParent<Canvas>();

        if(shop == null)
            return;

        itemRoot = FindItemRoot();

        if(itemRoot == null)
            return;

        TMP_Text[] texts = itemRoot.GetComponentsInChildren<TMP_Text>(true);
        Image[] images = itemRoot.GetComponentsInChildren<Image>(true);

        nameText = FindText(texts, "name item", "name", "nome");
        descriptionText = FindText(texts, "description", "descrizione", "desc");
        priceText = FindText(texts, "price", "prezzo", "cost", "costo");
        iconImage = FindImage(images, "portrait", "icon", "icona", "sprite", "image", "immagine");

        if(nameText == null && texts.Length > 0)
            nameText = texts[0];

        if(descriptionText == null && texts.Length > 1)
            descriptionText = texts[1];

        if(priceText == null && texts.Length > 2)
            priceText = texts[2];
    }

    private Transform FindItemRoot()
    {
        if(itemRoot != null)
            return itemRoot;

        Transform current = transform;

        while(current != null)
        {
            if(current.name.ToLower().StartsWith("item"))
                return current;

            if(shop != null && current == shop.transform)
                break;

            current = current.parent;
        }

        Transform[] children = shop.GetComponentsInChildren<Transform>(true);

        if(quantity > 0)
        {
            foreach(Transform child in children)
            {
                if(child.name == "Item " + quantity)
                    return child;
            }
        }

        foreach(Transform child in children)
        {
            if(child.name.ToLower().StartsWith("item") && child.GetComponentInChildren<Loot>() == this)
                return child;
        }

        foreach(Transform child in children)
        {
            if(child.name.ToLower().StartsWith("item"))
                return child;
        }

        return null;
    }

    private TMP_Text FindText(TMP_Text[] texts, params string[] keywords)
    {
        foreach(TMP_Text text in texts)
        {
            string objectName = text.gameObject.name.ToLower();

            foreach(string keyword in keywords)
            {
                if(objectName.Contains(keyword))
                    return text;
            }
        }

        return null;
    }

    private Image FindImage(Image[] images, params string[] keywords)
    {
        foreach(Image image in images)
        {
            string objectName = image.gameObject.name.ToLower();

            foreach(string keyword in keywords)
            {
                if(objectName.Contains(keyword))
                    return image;
            }
        }

        return null;
    }
}
