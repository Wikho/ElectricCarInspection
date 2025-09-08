// CarSpawner.cs
using System;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Placement")]
    [Tooltip("Where to spawn the car. Leave null to use this GameObject's transform.")]
    public Transform spawnAnchor;

    [Tooltip("If true, destroy any previously spawned car before spawning a new one.")]
    public bool replaceExisting = true;

    [Tooltip("Parent newly spawned cars under this transform (optional).")]
    public Transform spawnedParent;

    [Header("Runtime (read-only)")]
    [SerializeField] private GameObject lastSpawned;

    public CarDefinition CurrentDefinition { get; private set; }

    public event Action<GameObject> OnSpawned;

    /// <summary>
    /// Spawns the specified car definition.
    /// Hook this to a UI Button: OnClick -> CarSpawner.Spawn(carDefinitionAsset)
    /// </summary>
    public void Spawn(CarDefinition def)
    {
        if (def == null || def.carPrefab == null)
        {
            Debug.LogWarning("[CarSpawner] Missing CarDefinition or prefab.");
            return;
        }

        if (replaceExisting && lastSpawned != null)
        {
            Destroy(lastSpawned);
            lastSpawned = null;
        }

        var anchor = spawnAnchor != null ? spawnAnchor : transform;

        // Compute pose
        Vector3 pos = anchor.position + anchor.TransformVector(def.spawnOffset);
        Quaternion rot = anchor.rotation * Quaternion.Euler(def.spawnEuler);

        lastSpawned = Instantiate(def.carPrefab, pos, rot, spawnedParent);
        lastSpawned.name = $"{def.displayName}_Instance";

        CurrentDefinition = def;
        var tag = lastSpawned.GetComponent<CarDefinitionTag>() ?? lastSpawned.AddComponent<CarDefinitionTag>();
        tag.definition = def;


        OnSpawned?.Invoke(lastSpawned);

        // Optional: reset scale if prefab is authored at 1.0
        // lastSpawned.transform.localScale = Vector3.one;
    }

    /// <summary> Removes the currently spawned car (if any). </summary>
    public void Despawn()
    {
        if (lastSpawned != null)
        {
            Destroy(lastSpawned);
            lastSpawned = null;
            
        }
        CurrentDefinition = null;
        OnSpawned?.Invoke(null); // notify UI we have no car
    }

    /// <summary> Returns the currently spawned car instance (or null). </summary>
    public GameObject CurrentInstance => lastSpawned;
}
