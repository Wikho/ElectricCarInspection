// CarComponentButtonUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CarComponentButtonUI : MonoBehaviour
{
    [Header("Required References")]
    [Tooltip("The actual Button component to click.")]
    public Button button;

    [Tooltip("The TMP Text displaying the component name.")]
    public TMP_Text label;

    /// <summary>
    /// Sets the button's label text.
    /// </summary>
    public void SetText(string text)
    {
        if (label) label.text = text;
    }

    /// <summary>
    /// Returns the current text label (or empty).
    /// </summary>
    public string GetText()
    {
        return label ? label.text : "";
    }

    /// <summary>
    /// Safely returns the Button (if assigned).
    /// </summary>
    public Button GetButton()
    {
        return button;
    }
}
