using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils; // XROrigin

public class ForceRoomScaleOpenXR : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait a moment for XR subsystems to initialize
        yield return null; yield return null;

        var origin = FindAnyObjectByType<XROrigin>();
        if (origin)
        {
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            origin.CameraYOffset = 0f;
        }

        var inputs = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(inputs);
        foreach (var s in inputs)
        {
            // Try to force Floor origin and recenter
            s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            s.TryRecenter();
            Debug.Log($"XR origin now: {s.GetTrackingOriginMode()}");
        }
    }
}
