// AutoPlaceSpawnAnchorAtStart.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AutoPlaceSpawnAnchorAtStart : MonoBehaviour
{
    [Header("References")]
    public CarSpawner spawner;

    [Header("Placement")]
    [Tooltip("Meters in front of the user to place the car.")]
    public float distance = 1.5f;

    [Tooltip("Meters to the right of the user.")]
    public float lateralOffset = 0.0f;

    [Tooltip("Align the car's forward to match your current head yaw.")]
    public bool alignToCameraYaw = true;

    [Tooltip("Override Y position (e.g., floor height = 0).")]
    public float fixedY = 0f;

    [Header("Recenter")]
    [Tooltip("How long to wait (real time) after requesting recenter before placing.")]
    public float recenterSettleSeconds = 0.15f;

    Camera _cam;

    IEnumerator Start()
    {
        // --- ensure XR is up for a couple frames ---
        yield return null;
        yield return null;

        // --- request Floor origin + recenter BEFORE we read the camera pose ---
        TrySetFloorOriginAndRecenter();
        if (recenterSettleSeconds > 0f)
            yield return new WaitForSecondsRealtime(recenterSettleSeconds);

        // one more frame to let Camera.main update after recenter
        yield return null;

        _cam = Camera.main;
        if (!_cam)
        {
            Debug.LogWarning("[AutoPlaceSpawnAnchorAtStart] No Camera.main found.");
            yield break;
        }

        if (spawner == null)
        {
            Debug.LogWarning("[AutoPlaceSpawnAnchorAtStart] No CarSpawner assigned.");
            yield break;
        }

        Transform anchor = spawner.spawnAnchor != null ? spawner.spawnAnchor : spawner.transform;

        Vector3 camPos = _cam.transform.position;
        Vector3 fwd = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(_cam.transform.right, Vector3.up).normalized;

        // Calculate position in front of player
        Vector3 pos = camPos + fwd * distance + right * lateralOffset;
        pos.y = fixedY;

        anchor.position = pos;

        if (alignToCameraYaw)
        {
            Vector3 yawFwd = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
            if (yawFwd.sqrMagnitude > 1e-6f)
                anchor.rotation = Quaternion.LookRotation(yawFwd, Vector3.up);
        }
    }

    void TrySetFloorOriginAndRecenter()
    {
        // Ask all XR input subsystems to use Floor tracking origin and recenter.
        #if UNITY_2020_3_OR_NEWER
        var inputs = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(inputs);
        foreach (var s in inputs)
        {
            // Floor origin (if supported), then recenter
            s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            s.TryRecenter();
        }
        #endif
    }
}
