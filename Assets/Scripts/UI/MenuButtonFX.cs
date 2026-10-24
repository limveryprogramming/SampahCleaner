using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonFX : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    public Image fill;
    public Image outline;
    public Text label;

    public Color normalFill, hoverFill, pressedFill;
    public Color normalOutline, hoverOutline;
    public Color normalText, hoverText;

    public float hoverScale = 1.05f;
    public float speed = 14f;

    bool hovered, pressed, selected;
    Vector3 baseScale = Vector3.one;

    void Awake() => baseScale = transform.localScale;

    void OnEnable()
    {
        hovered = pressed = selected = false;
        Apply(1f);
    }

    public void OnPointerEnter(PointerEventData e) => hovered = true;
    public void OnPointerExit(PointerEventData e) { hovered = false; pressed = false; }
    public void OnPointerDown(PointerEventData e) => pressed = true;
    public void OnPointerUp(PointerEventData e) => pressed = false;
    public void OnSelect(BaseEventData e) => selected = true;
    public void OnDeselect(BaseEventData e) => selected = false;

    void Update() => Apply(1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));

    void Apply(float t)
    {
        bool active = hovered || selected;
        Color fillTarget = pressed ? pressedFill : active ? hoverFill : normalFill;
        Color outlineTarget = active ? hoverOutline : normalOutline;
        Color textTarget = active ? hoverText : normalText;
        Vector3 scaleTarget = baseScale * (pressed ? 0.98f : active ? hoverScale : 1f);

        if (fill != null) fill.color = Color.Lerp(fill.color, fillTarget, t);
        if (outline != null) outline.color = Color.Lerp(outline.color, outlineTarget, t);
        if (label != null) label.color = Color.Lerp(label.color, textTarget, t);
        transform.localScale = Vector3.Lerp(transform.localScale, scaleTarget, t);
    }
}
