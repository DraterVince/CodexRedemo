using UnityEngine;
using UnityEngine.EventSystems;

//public class InventorySlot : MonoBehaviour, IDropHandler
//{
//    public void OnDrop(PointerEventData eventData)
//    {
//        GameObject dropped = eventData.pointerDrag;
//        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

//        if (transform.childCount != 0)
//        {
//            GameObject current = transform.GetChild(0).gameObject;
//            DraggableItem currentDraggable = current.GetComponent<DraggableItem>();

//            currentDraggable.transform.SetParent(draggableItem.parentAfterDrag);
//        }
//        draggableItem.parentAfterDrag = transform;
//    }
//}
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

        if (transform.childCount != 0) // If a card is already in the slot, swap them
        {
            GameObject existingCard = transform.GetChild(0).gameObject;
            DraggableItem existingDraggable = existingCard.GetComponent<DraggableItem>();

            // Swap positions
            Transform previousParent = draggableItem.parentAfterDrag; // Store old parent
            existingDraggable.transform.SetParent(previousParent); // Move the existing card back
            existingDraggable.parentAfterDrag = previousParent; // Update its parent reference
        }

        // Place the newly dropped card into the slot
        draggableItem.transform.SetParent(transform);
        draggableItem.parentAfterDrag = transform;
    }

}

