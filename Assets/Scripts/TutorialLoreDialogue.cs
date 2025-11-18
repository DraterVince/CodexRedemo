using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Displays lore dialogue with letter-by-letter animation before tutorial
/// Click to advance through dialogue, then shows tutorial panel
/// </summary>
public class TutorialLoreDialogue : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel containing the lore dialogue")]
    public GameObject loreDialoguePanel;

    [Tooltip("Text component for displaying dialogue")]
    public TextMeshProUGUI dialogueText;
    
    [Tooltip("Optional: Continue indicator (arrow, icon, etc.)")]
    public GameObject continueIndicator;

    [Tooltip("Tutorial panel to show after lore is complete")]
    public GameObject tutorialPanel;
    
    [Header("Animation Settings")]
    [Tooltip("Speed of letter-by-letter animation (seconds per character)")]
    [Range(0.01f, 0.2f)]
    public float letterDelay = 0.05f;
    
    [Tooltip("Delay between dialogue lines (seconds)")]
    [Range(0f, 2f)]
    public float lineDelay = 0.5f;
    
    [Tooltip("Allow click to skip letter animation and show full text immediately")]
    public bool canSkipAnimation = true;
  
    [Header("Dialogue Content")]
    [Tooltip("List of dialogue entries to display")]
  public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();
    
    // State tracking
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueComplete = false;
    private Coroutine typingCoroutine;
    
    private void Start()
    {
        // Setup default lore if empty
        if (dialogueEntries.Count == 0)
      {
    SetupDefaultLore();
 }
    
        // Hide tutorial panel initially
      if (tutorialPanel != null)
        {
       tutorialPanel.SetActive(false);
        }
    
     // Hide continue indicator initially
    if (continueIndicator != null)
   {
         continueIndicator.SetActive(false);
  }

  // Start dialogue automatically
     StartDialogue();
    }
  
    /// <summary>
    /// Setup default lore dialogue (customize this!)
    /// </summary>
    private void SetupDefaultLore()
    {
 dialogueEntries = new List<DialogueEntry>
        {
         new DialogueEntry
            {
     dialogueText = "Welcome, young warrior. You stand at the threshold of a great journey."
     },
    new DialogueEntry
        {
        dialogueText = "Long ago, this land was filled with magic and wonder. But darkness crept in, corrupting all it touched."
            },
 new DialogueEntry
         {
           dialogueText = "The ancient cards hold the power to restore balance. But only those who master them can hope to succeed."
 },
   new DialogueEntry
       {
       dialogueText = "Let me teach you the ways of combat. Pay close attention, for your enemies will not be merciful."
     }
   };
    }
    
    /// <summary>
    /// Start the dialogue sequence
    /// </summary>
  public void StartDialogue()
    {
        if (loreDialoguePanel != null)
  {
         loreDialoguePanel.SetActive(true);
  }
        
        currentDialogueIndex = 0;
      dialogueComplete = false;
  
  DisplayNextDialogue();
    }
    
    /// <summary>
    /// Display the next dialogue entry
    /// </summary>
    private void DisplayNextDialogue()
    {
        if (currentDialogueIndex >= dialogueEntries.Count)
        {
  // All dialogue complete - show tutorial panel
     CompleteDialogue();
 return;
 }
        
   DialogueEntry entry = dialogueEntries[currentDialogueIndex];
      
        // Hide continue indicator while typing
        if (continueIndicator != null)
        {
    continueIndicator.SetActive(false);
        }
        
     // Start typing animation
if (typingCoroutine != null)
        {
  StopCoroutine(typingCoroutine);
     }
     typingCoroutine = StartCoroutine(TypeDialogue(entry.dialogueText));
    }
    
    /// <summary>
    /// Animate text letter by letter
  /// </summary>
    private IEnumerator TypeDialogue(string text)
  {
     isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in text)
  {
      dialogueText.text += letter;
         yield return new WaitForSeconds(letterDelay);
        }

    isTyping = false;
 
// Show continue indicator
 if (continueIndicator != null)
        {
      continueIndicator.SetActive(true);
        }
    }
    
    /// <summary>
    /// Handle click to advance dialogue
    /// Call this from a UI button or detect clicks in Update
    /// </summary>
    public void OnDialogueClick()
    {
    if (dialogueComplete)
{
 return;
        }
    
     if (isTyping)
    {
   // Skip animation and show full text
            if (canSkipAnimation)
    {
      if (typingCoroutine != null)
    {
     StopCoroutine(typingCoroutine);
       }
      
 dialogueText.text = dialogueEntries[currentDialogueIndex].dialogueText;
  isTyping = false;
    
       if (continueIndicator != null)
       {
        continueIndicator.SetActive(true);
    }
          }
        }
    else
        {
     // Move to next dialogue
   currentDialogueIndex++;
 DisplayNextDialogue();
        }
    }
    
    /// <summary>
/// Detect clicks anywhere on screen
    /// </summary>
    private void Update()
  {
        // Allow click anywhere to advance dialogue
 if (Input.GetMouseButtonDown(0) && !dialogueComplete)
        {
 OnDialogueClick();
   }
        
// Optional: Allow space or enter to advance
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) && !dialogueComplete)
        {
 OnDialogueClick();
        }
    }
    
    /// <summary>
    /// Complete dialogue and show tutorial panel
    /// </summary>
    private void CompleteDialogue()
    {
     dialogueComplete = true;
    
     Debug.Log("[TutorialLoreDialogue] Lore dialogue complete, showing tutorial panel");
     
        // Hide lore panel
   if (loreDialoguePanel != null)
  {
    loreDialoguePanel.SetActive(false);
        }
     
  // Show tutorial panel
   if (tutorialPanel != null)
   {
            tutorialPanel.SetActive(true);
        }
    }
    
    /// <summary>
  /// Skip all dialogue and go directly to tutorial
    /// </summary>
    public void SkipDialogue()
{
        if (typingCoroutine != null)
    {
        StopCoroutine(typingCoroutine);
        }
   
    CompleteDialogue();
    }
}

/// <summary>
/// Single dialogue entry with text only
/// </summary>
[System.Serializable]
public class DialogueEntry
{
  [TextArea(3, 10)]
    [Tooltip("The dialogue text to display")]
    public string dialogueText = "";
}
