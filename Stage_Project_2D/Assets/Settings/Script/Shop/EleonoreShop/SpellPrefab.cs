using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpellPrefab : MonoBehaviour
{
    public SpellData spell;
    public Canvas shop;
    public Sprite sprite;
    
    public int quantity;

    private TMP_Text nameText;
    private TMP_Text descriptionText;
    private TMP_Text priceText;
    private Image iconImage;
    private Transform artefactRoot;

    private void Awake()
    {
        SetSpellData();
    }

    private void OnValidate()
    {
        SetSpellData();
    }

    private void SetSpellData()
    {
        if(spell == null)
            return;

        FindShopReferences();

        sprite = spell.icon;

        if(nameText != null)
            nameText.SetText(spell.spellName);

        if(descriptionText != null)
            descriptionText.SetText(spell.description);

        if(priceText != null)
            priceText.SetText(spell.price.ToString());

        if(iconImage != null)
            iconImage.sprite = spell.icon;
    }

    private void FindShopReferences()
    {
        if(shop == null)
            shop = GetComponentInParent<Canvas>();

        if(shop == null)
            return;

        artefactRoot = FindArtefactRoot();

        if(artefactRoot == null)
            return;

        TMP_Text[] texts = artefactRoot.GetComponentsInChildren<TMP_Text>(true);
        Image[] images = artefactRoot.GetComponentsInChildren<Image>(true);

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

    private Transform FindArtefactRoot()
    {
        if(artefactRoot != null)
            return artefactRoot;

        Transform current = transform;

        while(current != null)
        {
            if(current.name.ToLower().StartsWith("artefact"))
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
                if(child.name == "Artefact " + quantity)
                    return child;
            }
        }

        foreach(Transform child in children)
        {
            if(child.name.ToLower().StartsWith("artefact") && child.GetComponentInChildren<SpellPrefab>() == this)
                return child;
        }

        foreach(Transform child in children)
        {
            if(child.name.ToLower().StartsWith("artefact"))
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
