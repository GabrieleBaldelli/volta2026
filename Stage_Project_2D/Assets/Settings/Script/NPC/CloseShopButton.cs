using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseShopButton : MonoBehaviour
{
    public Canvas Shop;

    public void CloseShop ()
    {
        Shop.gameObject.SetActive(false);
    }
}
