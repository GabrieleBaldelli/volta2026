using UnityEngine;
using UnityEngine.EventSystems;

public class ShopTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private MenuMenager owner;
    private string title;
    private string body;

    public void Configure(MenuMenager owner, string title, string body)
    {
        this.owner = owner;
        this.title = title;
        this.body = body;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(owner != null)
            owner.ShowArtefactTooltip(title, body, eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if(owner != null)
            owner.MoveArtefactTooltip(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(owner != null)
            owner.HideArtefactTooltip();
    }

    private void OnDisable()
    {
        if(owner != null)
            owner.HideArtefactTooltip();
    }
}
