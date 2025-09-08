// CarDefinition.cs
using UnityEngine;

[CreateAssetMenu(fileName = "CarDefinition", menuName = "Cars/Car Definition", order = 0)]
public class CarDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Tesla Model 3";
    public string manufacturer = "Tesla Inc.";
    public string model = "Model S";
    public int year = 2021;

    [Header("Prefab to spawn")]
    public GameObject carPrefab;

    [Header("Info Panel")]
    [Tooltip("Image shown in the CarInfoPanel.")]
    public Sprite infoPanelImage;

    [Header("Optional")]
    public Sprite thumbnail;   // for UI grids later
    public Vector3 spawnOffset = Vector3.zero; // small tweak if needed
    public Vector3 spawnEuler = Vector3.zero; // default facing
}
