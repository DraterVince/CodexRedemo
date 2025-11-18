using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SaveLoadSlot : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI slotNumberText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI progressLabelText; // Label text (e.g., "Completion Rate:")
    public TextMeshProUGUI progressText; // Progress percentage text (e.g., "45%")
    public Button loadButton;
    public Button deleteButton;
    public GameObject emptySlotIndicator;
    public GameObject dataContainer;

    private int slotNumber;
    private bool isEmpty;
    private Action<int, bool> onSlotClicked;
    private Action<int> onDeleteClicked;

    public void SetupSlot(int slot, NewAndLoadGameManager.GameData data, Action<int, bool> loadCallback, Action<int> deleteCallback)
    {
        slotNumber = slot;
        isEmpty = data.isEmpty;
        onSlotClicked = loadCallback;
        onDeleteClicked = deleteCallback;

        // Set slot number
        if (slotNumberText != null)
        {
            slotNumberText.text = "Slot " + slot;
        }

        if (isEmpty)
        {
            // Show empty slot UI
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(true);
            if (dataContainer != null) dataContainer.SetActive(false);
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
            
            if (loadButton != null)
            {
                // ENSURE BUTTON IS VISIBLE AND ACTIVE
                loadButton.gameObject.SetActive(true);
                loadButton.interactable = true;
                
                var buttonText = loadButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "New Game";
                }
                else
                {
                    var tmpText = loadButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        tmpText.text = "New Game";
                    }
                }
            }
        }
        else
        {
            // Show slot data
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(false);
            if (dataContainer != null) dataContainer.SetActive(true);
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);

            if (levelText != null)
            {
                // Convert build index to level number (build index 6 = level 1, since tutorial is at index 5)
                int displayLevel = data.levelsUnlocked - 5;
                levelText.text = "Level: " + displayLevel;
            }

            if (moneyText != null)
            {
                moneyText.text = "Money: " + data.currentMoney;
            }

            // Display progress label and percentage
            if (progressLabelText != null)
            {
                progressLabelText.text = "Completion Rate:";
            }

            if (progressText != null && NewAndLoadGameManager.Instance != null)
            {
                float progress = NewAndLoadGameManager.Instance.GetSlotProgressPercentage(slot);
                progressText.text = $"{progress:F0}%";
            }

            if (loadButton != null)
            {
                // ENSURE BUTTON IS VISIBLE AND ACTIVE
                loadButton.gameObject.SetActive(true);
                loadButton.interactable = true;
                
                var buttonText = loadButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "Load";
                }
                else
                {
                    var tmpText = loadButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        tmpText.text = "Load";
                    }
                }
            }
        }

        // Setup button listeners
        if (loadButton != null)
        {
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => {
                onSlotClicked?.Invoke(slotNumber, isEmpty);
            });
        }

        if (deleteButton != null && !isEmpty)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => {
                onDeleteClicked?.Invoke(slotNumber);
            });
        }
    }
}
