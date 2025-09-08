using UnityEngine;

using UnityEngine.EventSystems;

/// <summary>
/// Tracks pointer down/up/exit so we can detect press-and-hold on UGUI Buttons
/// using the Input System / XR UI. Keep on same file for simplicity.
/// </summary>
public class ButtonHoldTracker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public System.Action OnHoldStart;
    public System.Action OnHoldEnd;

    bool _holding;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_holding) return;
        _holding = true;
        OnHoldStart?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_holding) return;
        _holding = false;
        OnHoldEnd?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // treat pointer leaving the button as release
        if (!_holding) return;
        _holding = false;
        OnHoldEnd?.Invoke();
    }
}