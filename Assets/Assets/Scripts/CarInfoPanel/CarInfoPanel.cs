// CarInfoPanel.cs
using UnityEngine;
using UnityEngine.UI;

public class CarInfoPanel : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Used to find the currently spawned car/definition when ShowCurrent() is called.")]
    public CarSpawner spawner;

    [Header("UI References")]
    [Tooltip("The GameObject that will be SetActive(true) when showing info.")]
    public GameObject infoRoot;

    [Tooltip("The Image UI component where the car infoPanelImage will be shown.")]
    public Image imageDisplay;

    /// <summary>
    /// Shows the car info panel with the given car definition's image.
    /// </summary>
    public void Show(CarDefinition carDef)
    {
        if (carDef == null)
        {
            Debug.LogWarning("[CarInfoPanel] No CarDefinition passed in.");
            return;
        }

        if (infoRoot != null)
            infoRoot.SetActive(true);

        if (imageDisplay != null)
        {
            if (carDef.infoPanelImage != null)
            {
                imageDisplay.sprite = carDef.infoPanelImage;
                imageDisplay.enabled = true;
            }
            else
            {
                imageDisplay.sprite = null;
                imageDisplay.enabled = false;
                Debug.LogWarning($"[CarInfoPanel] CarDefinition '{carDef.displayName}' has no infoPanelImage assigned.");
            }
        }
    }

    /// <summary>
    /// Auto-detects the currently spawned car and shows its info image.
    /// Wire your Info button to this.
    /// </summary>
    public void ShowCurrent()
    {
        if (spawner == null)
        {
            Debug.LogWarning("[CarInfoPanel] No CarSpawner assigned.");
            return;
        }

        // Prefer the definition the spawner knows about
        var def = spawner.CurrentDefinition;

        // Fallback: read from tag on the instance (in case someone spawned externally)
        if (def == null)
        {
            var inst = spawner.CurrentInstance;
            if (inst != null)
            {
                var tag = inst.GetComponentInChildren<CarDefinitionTag>(true);
                if (tag != null) def = tag.definition;
            }
        }

        if (def == null)
        {
            Debug.LogWarning("[CarInfoPanel] No active car definition found.");
            return;
        }

        Show(def);
    }

    /// <summary>
    /// Hides the info panel manually (optional).
    /// </summary>
    public void Hide()
    {
        if (infoRoot != null)
            infoRoot.SetActive(false);
    }
}
