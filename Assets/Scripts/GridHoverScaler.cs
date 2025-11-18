using UnityEngine;
using UnityEngine.EventSystems;

public class GridHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    public float defaultScale = 0.8f;
    public float hoverScale = 1.2f;
    public float animationDuration = 0.2f;

    private Vector3 defaultScaleVec;
    private Vector3 hoverScaleVec;
    private bool isHovered = false;
    private float animTime = 0f;

    void Awake()
    {
        defaultScaleVec = Vector3.one * defaultScale;
        hoverScaleVec = Vector3.one * hoverScale;
        transform.localScale = defaultScaleVec;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        animTime = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        animTime = 0f;
    }

    void Update()
    {
        if (animTime < animationDuration)
        {
            animTime += Time.deltaTime;
            float t = Mathf.Clamp01(animTime / animationDuration);
            Vector3 targetScale = isHovered ? hoverScaleVec : defaultScaleVec;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
            ScaleChildren(Vector3.Lerp(transform.localScale, targetScale, t));
        }
    }

    public void ScaleChildren(Vector3 scale)
    {
        foreach (Transform child in transform)
        {
            child.localScale = scale;
        }
    }

}