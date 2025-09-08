// CarComponentEntry.cs
using System;
using UnityEngine;

[Serializable]
public class CarComponentEntry
{
    [Tooltip("Display name for UI buttons (unique per car).")]
    public string displayName;

    [Tooltip("Semantic type of this component (optional, for filtering later).")]
    public VehicleComponentType type = VehicleComponentType.Unknown;

    [Tooltip("Root GameObject of this component in the prefab (enable/disable to show/hide).")]
    public GameObject root;
}

// VehicleComponentType.cs
public enum VehicleComponentType
{
    Unknown = 0,
    Battery,
    Airbag,
    GasCylinders,
    GasInflater,
    Engine,
    Brakes,
    Wheels,
    Suspension,
    Electronics,
    Seatbelt,
    Reinforcements,
    FuelSystem,
    Cooling,
    Interior,
}
