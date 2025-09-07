// HandMenuUIController.cs
// Unity 6.2+ (UGUI + TextMeshPro)
// Attach this to your Hand Menu root (e.g., "Follow GameObject" or "Main Menu")

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandMenuUIController : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        [Tooltip("Display name for this tab (also used to update the title text).")]
        public string displayName = "Settings";

        [Tooltip("Toggle that activates this tab.")]
        public Toggle toggle;

        [Tooltip("Root GameObject of the page/panel for this tab.")]
        public GameObject pageRoot;
    }

    [Header("Tabs")]
    [Tooltip("List of tabs. Each needs a name, a Toggle, and a Page Root.")]
    public List<Tab> tabs = new List<Tab>();

    [Header("Title")]
    [Tooltip("Optional TMP text that will show the current tab name.")]
    public TMP_Text titleText;

    [Tooltip("Optional prefix for the title text (e.g., \"Hand Menu • \")")]
    public string titlePrefix = "";

    [Header("Defaults")]
    [Tooltip("Start on this tab index (0-based). If < 0, uses first tab with Toggle.isOn or index 0.")]
    public int defaultTabIndex = 0;

    // runtime
    int _activeIndex = -1;
    bool _initialized = false;

    void OnEnable()
    {
        WireUp();
        // Initialize the visible state safely every time we enable
        InitializeActiveTab();
    }

    void OnDisable()
    {
        Unwire();
    }

    void WireUp()
    {
        if (_initialized) return;

        // Add listeners for each tab toggle
        for (int i = 0; i < tabs.Count; i++)
        {
            int capture = i; // capture index for closure
            var t = tabs[i];

            if (t?.toggle != null)
                t.toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                        SelectByIndex(capture);
                    else
                        OnToggleTurnedOff(capture); // usually no-op, but keeps state tidy if needed
                });
        }

        _initialized = true;
    }

    void Unwire()
    {
        if (!_initialized) return;

        foreach (var t in tabs)
            if (t?.toggle != null)
                t.toggle.onValueChanged.RemoveAllListeners();

        _initialized = false;
    }

    void InitializeActiveTab()
    {
        // Decide which tab to start on
        int startIndex = -1;

        // 1) honor defaultTabIndex if valid
        if (defaultTabIndex >= 0 && defaultTabIndex < tabs.Count)
            startIndex = defaultTabIndex;

        // 2) otherwise pick first Toggle already on
        if (startIndex < 0)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                var t = tabs[i];
                if (t?.toggle != null && t.toggle.isOn)
                {
                    startIndex = i;
                    break;
                }
            }
        }

        // 3) fallback to 0
        if (startIndex < 0 && tabs.Count > 0)
            startIndex = 0;

        if (startIndex >= 0)
            SelectByIndex(startIndex, updateToggleState: true);
        else
            ApplyVisibility(-1); // hide all if nothing present
    }

    void OnToggleTurnedOff(int index)
    {
        // Usually ignored. If your ToggleGroup allows multiple on/off,
        // we still keep the active page visible until another toggle turns on.
        // You can add custom behavior here if needed.
    }

    /// <summary>
    /// Select a tab by zero-based index.
    /// </summary>
    public void SelectByIndex(int index, bool updateToggleState = false)
    {
        if (index < 0 || index >= tabs.Count)
            return;

        if (updateToggleState)
        {
            // Ensure only this toggle is on (works with or without ToggleGroup)
            for (int i = 0; i < tabs.Count; i++)
            {
                var t = tabs[i];
                if (t?.toggle != null)
                    t.toggle.isOn = (i == index);
            }
        }

        ApplyVisibility(index);
    }

    /// <summary>
    /// Select a tab by its display name (case-insensitive).
    /// </summary>
    public void SelectByName(string name, bool updateToggleState = false)
    {
        if (string.IsNullOrEmpty(name)) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            var t = tabs[i];
            if (t != null && string.Equals(t.displayName, name, StringComparison.OrdinalIgnoreCase))
            {
                SelectByIndex(i, updateToggleState);
                return;
            }
        }
    }

    void ApplyVisibility(int newActiveIndex)
    {
        _activeIndex = newActiveIndex;

        for (int i = 0; i < tabs.Count; i++)
        {
            var t = tabs[i];
            if (t?.pageRoot == null) continue;

            bool active = (i == _activeIndex);
            if (t.pageRoot.activeSelf != active)
                t.pageRoot.SetActive(active);
        }

        UpdateTitle();
    }

    void UpdateTitle()
    {
        if (titleText == null) return;

        string nameToShow = "";
        if (_activeIndex >= 0 && _activeIndex < tabs.Count && tabs[_activeIndex] != null)
            nameToShow = tabs[_activeIndex].displayName ?? "";

        titleText.text = string.IsNullOrEmpty(titlePrefix)
            ? nameToShow
            : $"{titlePrefix}{nameToShow}";
    }

    // Optional utility – get current tab name
    public string CurrentTabName =>
        (_activeIndex >= 0 && _activeIndex < tabs.Count && tabs[_activeIndex] != null)
            ? tabs[_activeIndex].displayName
            : string.Empty;
}
