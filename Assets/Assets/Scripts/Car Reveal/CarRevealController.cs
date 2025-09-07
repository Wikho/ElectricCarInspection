using System;
using System.Collections.Generic;
using UnityEngine;

public class CarRevealController : MonoBehaviour
{
    // ---- Assign in Inspector ----

    [Header("Car Renderers (all meshes that should swap materials)")]
    [SerializeField] private Renderer[] carRenderers;

    [Header("Transparent Material (URP, Surface=Transparent)")]
    [SerializeField] private Material transparentMaterial;

    [Header("(Optional) Transparent Override For Glass")]
    [Tooltip("Leave null to use Transparent Material for glass too.")]
    [SerializeField] private Material transparentGlassMaterial;

    [Header("Glass Detection")]
    [Tooltip("If a submesh/material name contains this keyword, it's treated as glass.")]
    [SerializeField] private string glassKeyword = "glass";

    [Header("Component Overlays (assign meshes/VFX you want to show)")]
    [SerializeField] private List<ComponentGroup> componentGroups = new();

    // ---- Runtime ----

    private enum Mode { Normal, All }
    private Mode currentMode = Mode.Normal;
    [SerializeField] private bool showAllComponentsWhenInAllMode = true;

    public enum ComponentType
    {
        None = 0,
        Battery12V,
        EmergencyDisconnect,
        SRSControlUnit,
        GasCylinder,
        Airbags,               // knee airbags not applicable note is content choice, not code logic
        SeatbeltPretensioners,
        GasInflator,
        HighVoltageComponents,
        Reinforcements
    }

    [Serializable]
    public class ComponentGroup
    {
        public ComponentType type;
        public GameObject[] objects;
    }

    // store originals per renderer
    private readonly Dictionary<Renderer, Material[]> originalMats = new();

    // current selection
    private ComponentType selectedComponent = ComponentType.None;

    private void Awake()
    {
        CacheOriginalMaterials();
        HideAllOverlays();
    }

    // ---------- Public API (hook these to UI buttons / XR UI) ----------

    /// <summary>Show the car fully opaque with original materials; hide all overlays.</summary>
    public void SetMode_Normal()
    {
        currentMode = Mode.Normal;
        RestoreOriginalMaterials();
        HideAllOverlays();
    }

    /// <summary>Make car see-through; keep only the selected component visible (if any).</summary>
    public void SetMode_All()
    {
        currentMode = Mode.All;
        ApplyTransparentMaterials();
        UpdateOverlayVisibility();
    }

    /// <summary>Set which component to show when in ALL mode. Pass enum as int from UI.</summary>
    public void SelectComponent_Int(int componentEnumValue)
    {
        var val = Mathf.Clamp(componentEnumValue, 0, Enum.GetValues(typeof(ComponentType)).Length - 1);
        SelectComponent((ComponentType)val);
    }

    /// <summary>Set which component to show when in ALL mode.</summary>
    public void SelectComponent(ComponentType type)
    {
        selectedComponent = type;
        UpdateOverlayVisibility();
    }

    /// <summary>Hide all components and return to original opaque look.</summary>
    public void SetNone()
    {
        selectedComponent = ComponentType.None;
        SetMode_Normal();
    }

    // --------------------- Internals ---------------------

    private void CacheOriginalMaterials()
    {
        originalMats.Clear();
        foreach (var r in carRenderers)
        {
            if (r == null) continue;
            // store a copy of the sharedMaterials array so we can restore exactly
            var mats = r.sharedMaterials;
            var copy = new Material[mats.Length];
            Array.Copy(mats, copy, mats.Length);
            originalMats[r] = copy;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in originalMats)
        {
            var r = kvp.Key;
            if (r == null) continue;
            r.sharedMaterials = kvp.Value;
        }
    }

    private void ApplyTransparentMaterials()
    {
        if (transparentMaterial == null)
        {
            Debug.LogWarning("Transparent material is not assigned.");
            return;
        }

        foreach (var r in carRenderers)
        {
            if (r == null) continue;

            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                // choose glass vs base transparent if we have a special glass mat
                if (transparentGlassMaterial != null && m != null &&
                    m.name.ToLowerInvariant().Contains(glassKeyword.ToLowerInvariant()))
                {
                    mats[i] = transparentGlassMaterial;
                }
                else
                {
                    mats[i] = transparentMaterial;
                }
            }
            r.sharedMaterials = mats;
        }
    }

    private void HideAllOverlays()
    {
        foreach (var group in componentGroups)
        {
            if (group?.objects == null) continue;
            foreach (var go in group.objects)
                if (go) go.SetActive(false);
        }
    }

    private void UpdateOverlayVisibility()
    {
        // start hidden
        HideAllOverlays();

        if (currentMode != Mode.All) return;

        // If we want ALL parts in All mode, show everything.
        if (showAllComponentsWhenInAllMode && selectedComponent == ComponentType.None)
        {
            foreach (var group in componentGroups)
            {
                if (group?.objects == null) continue;
                foreach (var go in group.objects)
                    if (go) go.SetActive(true);
            }
            return;
        }

        // Otherwise show only the selected group (old behaviour)
        if (selectedComponent == ComponentType.None) return;

        foreach (var group in componentGroups)
        {
            if (group == null || group.objects == null) continue;
            bool show = group.type == selectedComponent;
            foreach (var go in group.objects)
                if (go) go.SetActive(show);
        }
    }
}
