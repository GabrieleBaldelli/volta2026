using UnityEngine;
using UnityEngine.UI;

public class LifeBar_Enemy : MonoBehaviour
{

    public void UpdateLifeBar(float vita_attuale, float vita_massima)
    {
        Slider slider = GetComponent<Slider>();
        slider.value = vita_attuale / vita_massima;

        

    }
}