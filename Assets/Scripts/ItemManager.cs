using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public List <Item> items = new List <Item>();

    public List<Item> selectedCard = new List<Item>();
    public List<CardDisplay> cardDisplays = new List<CardDisplay>();

    public List <Item> selectedRandomCard = new List <Item> ();

    public GameObject grid;

    public void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(StartRandom());
    }

    public IEnumerator StartRandom() {
        selectedCard = new List<Item>(items);

        for (int i = 0; i < selectedCard.Count; i++)
        {
            int randomIndex = Random.Range(0, selectedCard.Count);

            cardDisplays[i].cardName = selectedCard[randomIndex].cardName;
            cardDisplays[i].cardDesign.sprite = selectedCard[randomIndex].artwork;
            cardDisplays[i].SetAssignedItem(selectedCard[randomIndex]);

            yield return new WaitForSeconds(0.15f);
            cardDisplays[i].gameObject.SetActive(true);
            cardDisplays[i].gameObject.transform.localScale = Vector3.one;

            selectedRandomCard.Add(selectedCard[randomIndex]);
            selectedCard.RemoveAt(randomIndex);
        }
    }

    public void ResetCards()
    {
        selectedCard.Clear();
        for (int i = 0; i < cardDisplays.Count; i++)
        {
            cardDisplays[i].gameObject.SetActive(false);
            cardDisplays[i].gameObject.transform.SetParent(grid.transform);
            cardDisplays[i].gameObject.transform.localScale = Vector3.one;
            cardDisplays[i].SetAssignedItem(null);
        }
        StartCoroutine(StartRandom());
    }
}
