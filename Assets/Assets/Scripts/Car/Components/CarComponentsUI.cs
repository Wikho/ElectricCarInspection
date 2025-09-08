// CarComponentsUI.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarComponentsUI : MonoBehaviour
{
    [Header("Scene References")]
    public CarSpawner spawner;

    [Tooltip("Parent where component buttons will be instantiated.")]
    public Transform buttonsParent;

    [Tooltip("A prefab with Button + TMP_Text for a component entry.")]
    public CarComponentButtonUI componentButtonPrefab; // <-- CHANGED: use wrapper

    [Header("Controls")]
    public Button normalButton; // hook to -> OnNormalPressed
    public Button allButton;    // hook to -> OnAllPressed
    public Toggle buildComponentsToggle; // hook to -> OnBuildToggleChanged

    [Header("Optional UI")]
    public TMP_Text carTitle;   // optional display of car name
    public TMP_Text modeHint;   // optional hint text

    CarFeatures _current;

    GameObject _lastBoundInstance; // NEW

    void OnEnable()
    {
        if (spawner != null) spawner.OnSpawned += HandleCarSpawned;

        RebindToCurrentCar(resetIfNew: false); // CHANGED
        WireButtons();
    }

    void OnDisable()
    {
        if (spawner != null) spawner.OnSpawned -= HandleCarSpawned;
        UnwireButtons();
    }

    void WireButtons()
    {
        if (normalButton) normalButton.onClick.AddListener(OnNormalPressed);
        if (allButton) allButton.onClick.AddListener(OnAllPressed);
        if (buildComponentsToggle) buildComponentsToggle.onValueChanged.AddListener(OnBuildToggleChanged);
    }

    void UnwireButtons()
    {
        if (normalButton) normalButton.onClick.RemoveAllListeners();
        if (allButton) allButton.onClick.RemoveAllListeners();
        if (buildComponentsToggle) buildComponentsToggle.onValueChanged.RemoveAllListeners();
    }

    void HandleCarSpawned(GameObject instance)
    {
        // When the spawner notifies a NEW instance, we do want to reset
        RebindToCurrentCar(resetIfNew: true); // NEW
    }

    // CHANGED: allow caller to decide whether to reset visual state (only when car actually changed)
    void RebindToCurrentCar(bool resetIfNew) // CHANGED
    {
        _current = null;
        var inst = spawner ? spawner.CurrentInstance : null;

        // If no instance, clear UI state
        if (!inst)
        {
            _lastBoundInstance = null; // NEW
            ClearButtons();
            if (carTitle) carTitle.text = "";
            UpdateModeHint();
            return;
        }

        // Detect whether this is a different car than last time
        bool instanceChanged = (inst != _lastBoundInstance); // NEW
        _lastBoundInstance = inst; // NEW

        _current = inst.GetComponentInChildren<CarFeatures>(true);
        if (!_current)
        {
            ClearButtons();
            if (carTitle) carTitle.text = inst.name; // still show name if you want
            UpdateModeHint();
            return;
        }

        // reset modes on car change ONLY
        if (resetIfNew && instanceChanged) // CHANGED
        {
            _current.Mode_Normal();
            if (buildComponentsToggle) buildComponentsToggle.isOn = false;
        }

        // update header
        if (carTitle) carTitle.text = inst.name;

        // rebuild buttons (safe to do always; does not change component visibility)
        BuildButtons(_current.GetComponentsList());
        UpdateModeHint();
    }

    void BuildButtons(IReadOnlyList<CarComponentEntry> list)
    {
        ClearButtons();
        if (list == null || componentButtonPrefab == null || buttonsParent == null) return;

        foreach (var entry in list)
        {
            if (entry == null || string.IsNullOrEmpty(entry.displayName)) continue;

            // Instantiate wrapper prefab
            var ui = Instantiate(componentButtonPrefab, buttonsParent);
            ui.name = $"Btn_{entry.displayName}";

            // Set label via wrapper
            ui.SetText(entry.displayName);

            // Hook click via wrapper's Button
            var btn = ui.GetButton();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (_current == null) return;

                    if (buildComponentsToggle != null && buildComponentsToggle.isOn)
                        _current.ToggleAdditive(entry.displayName);
                    else
                        _current.ShowOnly(entry.displayName);

                    UpdateModeHint();
                });
            }
        }
    }

    void ClearButtons()
    {
        if (!buttonsParent) return;
        for (int i = buttonsParent.childCount - 1; i >= 0; i--)
            Destroy(buttonsParent.GetChild(i).gameObject);
    }

    // ===== Button handlers =====

    public void OnNormalPressed()
    {
        if (_current == null) return;
        _current.Mode_Normal();
        UpdateModeHint();
    }

    public void OnAllPressed()
    {
        if (_current == null) return;
        _current.Mode_All();
        UpdateModeHint();
    }

    public void OnBuildToggleChanged(bool on)
    {
        if (_current == null) return;
        _current.SetBuildMode(on);
        // When entering build mode, go to ghost body (but keep current components as-is).
        if (on) _current.ApplySeeThroughBody();
        UpdateModeHint();
    }

    void UpdateModeHint()
    {
        if (!modeHint) return;
        if (!_current)
        {
            modeHint.text = "No car";
            return;
        }

        modeHint.text = (buildComponentsToggle && buildComponentsToggle.isOn)
            ? "Build Components: tap to add/remove"
            : "Single Component: tap to isolate";
    }
}
