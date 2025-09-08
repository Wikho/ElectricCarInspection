// CarFeatures.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CarFeatures : MonoBehaviour
{
    [Header("Car Body (for ghost mode)")]
    [Tooltip("Renderers that form the car body/shell (made see-through in 'All' or single-component mode).")]
    public Renderer[] carBodyRenderers;

    [Tooltip("Material used to turn the car body see-through (URP Transparent).")]
    public Material seeThroughMaterial;

    [Header("Components")]
    [Tooltip("List the components this car exposes.")]
    public List<CarComponentEntry> components = new();

    // runtime caches
    readonly Dictionary<Renderer, Material[]> _originalMats = new();
    readonly HashSet<string> _selectedNames = new();
    bool _buildMode;

    void Awake()
    {
        CacheOriginalMaterials();
        HideAllComponents(); // default clean
        ApplyNormalBody();   // default normal
    }

    void CacheOriginalMaterials()
    {
        _originalMats.Clear();
        foreach (var r in carBodyRenderers)
        {
            if (!r) continue;
            // Copy the instance materials so we don't mutate shared assets
            var mats = r.materials; // instanced
            _originalMats[r] = mats;
        }
    }

    // ===== Body material states =====

    public void ApplyNormalBody()
    {
        foreach (var kv in _originalMats)
        {
            var r = kv.Key;
            if (!r) continue;
            r.materials = kv.Value;
        }
    }

    public void ApplySeeThroughBody()
    {
        if (!seeThroughMaterial) return;
        foreach (var r in carBodyRenderers)
        {
            if (!r) continue;
            var count = r.sharedMaterials != null ? r.sharedMaterials.Length : 1;
            var arr = new Material[count];
            for (int i = 0; i < count; i++) arr[i] = seeThroughMaterial;
            r.materials = arr; // instanced
        }
    }

    // ===== Component visibility =====

    public void HideAllComponents()
    {
        foreach (var c in components)
            if (c != null && c.root) c.root.SetActive(false);
        _selectedNames.Clear();
    }

    public void ShowAllComponents()
    {
        foreach (var c in components)
            if (c != null && c.root) c.root.SetActive(true);
        _selectedNames.Clear();
        foreach (var c in components)
            if (c != null) _selectedNames.Add(c.displayName);
    }

    public void SetBuildMode(bool on)
    {
        _buildMode = on;
        if (!_buildMode)
            _selectedNames.Clear(); // reset selection when leaving build mode
    }

    public void ShowOnly(string displayName)
    {
        // single selection: ghost body + only that component
        ApplySeeThroughBody();
        foreach (var c in components)
            if (c != null && c.root) c.root.SetActive(c.displayName == displayName);

        _selectedNames.Clear();
        _selectedNames.Add(displayName);
    }

    public void ToggleAdditive(string displayName)
    {
        // build mode additive: ghost body + toggle component visibility
        ApplySeeThroughBody();

        foreach (var c in components)
        {
            if (c == null || c.root == null) continue;
            if (c.displayName != displayName) continue;

            bool newActive = !c.root.activeSelf;
            c.root.SetActive(newActive);

            if (newActive) _selectedNames.Add(displayName);
            else _selectedNames.Remove(displayName);
        }
    }

    public void Mode_Normal()
    {
        // Show just car body, hide all components
        ApplyNormalBody();
        HideAllComponents();
    }

    public void Mode_All()
    {
        // Ghost car body + show all components
        ApplySeeThroughBody();
        ShowAllComponents();
    }

    public IReadOnlyList<CarComponentEntry> GetComponentsList() => components;
}
