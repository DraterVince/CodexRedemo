using UnityEngine;
using TMPro;

public class ClearText : MonoBehaviour
{
    [Header("Input Fields to Clear")]
    public TMP_InputField[] inputFields;

    // Call this from your close button's OnClick event
    public void ClearAllFields()
    {
        foreach (var field in inputFields)
        {
            if (field != null)
                field.text = string.Empty;
        }
    }
}
