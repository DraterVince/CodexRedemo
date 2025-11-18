using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Image image;
    [HideInInspector] public Transform parentAfterDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    Transform potentialParent = eventData.pointerCurrentRaycast.gameObject?.transform;

    //    while (potentialParent != null && potentialParent.GetComponent<InventorySlot>() == null)
    //    {
    //        potentialParent = potentialParent.parent;
    //    }

    //    if (potentialParent != null)
    //    {
    //        transform.SetParent(potentialParent);
    //    }
    //    else
    //    {
    //        transform.SetParent(parentAfterDrag);
    //    }

    //    image.raycastTarget = true;

    //}
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }

}