using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.EventSystems;

public class UpdateSpawnedVechileLocation : MonoBehaviour
{
    [Header("References")]
    public CarSpawner spawner;

    [Tooltip("Master switch to allow editing/moving.")]
    public Toggle editModeToggle;

    [Tooltip("When ON: right stick = camera-relative move. When OFF: right stick = world X/Z move.")]
    public Toggle directionModeToggle; // Suggested UI label: "Face Direction Move"

    [Tooltip("Parent object that contains X+/X-/Z+/Z- and L/R rotation buttons.")]
    public GameObject fallbackControlsRoot;

    [Header("Fallback Move Buttons")]
    public Button xPlusButton;
    public Button xMinusButton;
    public Button zPlusButton;
    public Button zMinusButton;

    [Header("Fallback Rotation Buttons")]
    public Button rotateLeftButton;   // L
    public Button rotateRightButton;  // R

    [Header("Movement Settings")]
    [Tooltip("Meters/sec when using the RIGHT thumbstick.")]
    public float stickMoveSpeed = 1.35f;

    [Tooltip("Meters per click (and per repeat step) for +/- buttons.")]
    public float nudgeStep = 0.05f;

    [Tooltip("Deadzone for sticks (0..1).")]
    [Range(0f, 1f)] public float stickDeadzone = 0.15f;

    [Header("Rotation Settings")]
    [Tooltip("Degrees/sec when using the LEFT stick (horizontal).")]
    public float stickRotateSpeedDeg = 90f;

    [Tooltip("Degrees per click (and per repeat) for L/R buttons.")]
    public float rotateStepDeg = 5f;

    [Header("Optional Bounds")]
    public bool clampToBounds = false;
    public Vector2 xLimits = new Vector2(-5f, 5f);
    public Vector2 zLimits = new Vector2(-5f, 5f);

    [Header("Hold-to-Repeat")]
    [Tooltip("Seconds to wait before repeating after press-and-hold.")]
    public float holdDelay = 0.25f;

    [Tooltip("Seconds between repeats while holding.")]
    public float repeatRate = 0.05f;

    // --- runtime ---
    InputDevice _rightController;
    InputDevice _leftController;
    bool _hasRightStick;
    bool _hasLeftStick;

    Transform _target; // spawn anchor or spawner transform
    float _lockedY;

    class HoldState
    {
        public bool isHeld;
        public float nextRepeatAt;
    }
    class MoveHoldState : HoldState { public Vector2 dir; }   // dx,z per step
    class RotHoldState : HoldState { public float degrees; } // + / - yaw per step

    readonly Dictionary<Button, MoveHoldState> _moveHolds = new();
    readonly Dictionary<Button, RotHoldState> _rotHolds = new();

    void Awake()
    {
        // Wire movement buttons
        SetupMoveButton(xPlusButton, new Vector2(+nudgeStep, 0));
        SetupMoveButton(xMinusButton, new Vector2(-nudgeStep, 0));
        SetupMoveButton(zPlusButton, new Vector2(0, +nudgeStep));
        SetupMoveButton(zMinusButton, new Vector2(0, -nudgeStep));

        // Wire rotation buttons
        SetupRotateButton(rotateLeftButton, -rotateStepDeg);
        SetupRotateButton(rotateRightButton, +rotateStepDeg);

        if (fallbackControlsRoot) fallbackControlsRoot.SetActive(false);
    }

    void OnEnable()
    {
        TryFindControllers();
        CacheTarget();
        SyncUIVisibility();
    }

    void Update()
    {
        CacheTarget();

        // Lock Y to initial spawn anchor height
        if (_target != null && _lockedY == default) _lockedY = _target.position.y;

        // Refresh device state
        if (!IsValid(_rightController) || !IsValid(_leftController)) TryFindControllers();

        // Gate on edit mode
        if (!IsEditMode()) { SyncUIVisibility(); return; }

        _hasRightStick = HasPrimary2DAxis(_rightController);
        _hasLeftStick = HasPrimary2DAxis(_leftController);

        // Fallback controls show only if right stick missing
        if (fallbackControlsRoot) fallbackControlsRoot.SetActive(!_hasRightStick);

        if (_hasRightStick) ApplyStickMove();
        else UpdateMoveHoldRepeats();

        if (_hasLeftStick) ApplyStickRotate();
        else UpdateRotateHoldRepeats();
    }

    // ---------- Movement Core ----------

    void MoveTarget(Vector2 dxz)
    {
        if (_target == null) return;

        var pos = _target.position + new Vector3(dxz.x, 0f, dxz.y);
        pos.y = _lockedY;

        if (clampToBounds)
        {
            pos.x = Mathf.Clamp(pos.x, xLimits.x, xLimits.y);
            pos.z = Mathf.Clamp(pos.z, zLimits.x, zLimits.y);
        }

        _target.position = pos;
    }

    void RotateTarget(float degrees)
    {
        if (_target == null) return;
        _target.Rotate(0f, degrees, 0f, Space.World); // yaw about world up
    }

    void ApplyStickMove()
    {
        if (_target == null || !_rightController.isValid) return;

        if (_rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
        {
            if (axis.magnitude < stickDeadzone) return;

            Vector2 step;
            if (IsDirectionModeOn() && TryGetCameraGroundVectors(out var fwd, out var right))
            {
                // Camera-relative: (right * x + fwd * y) on ground plane
                Vector3 moveVec = right * axis.x + fwd * axis.y;
                step = new Vector2(moveVec.x, moveVec.z) * (stickMoveSpeed * Time.deltaTime);
            }
            else
            {
                // Regular world X/Z mapping
                step = new Vector2(axis.x, axis.y) * (stickMoveSpeed * Time.deltaTime);
            }

            MoveTarget(step);
        }
    }

    void ApplyStickRotate()
    {
        if (_target == null || !_leftController.isValid) return;

        if (_leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
        {
            float x = axis.x;
            if (Mathf.Abs(x) < stickDeadzone) return;

            float deg = x * stickRotateSpeedDeg * Time.deltaTime;
            RotateTarget(deg);
        }
    }

    bool TryGetCameraGroundVectors(out Vector3 fwd, out Vector3 right)
    {
        var cam = Camera.main;
        if (!cam)
        {
            fwd = right = Vector3.zero;
            return false;
        }

        fwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        if (fwd.sqrMagnitude < 1e-6f || right.sqrMagnitude < 1e-6f) return false;
        return true;
    }

    // ---------- Fallback Buttons with Hold ----------

    void SetupMoveButton(Button btn, Vector2 dir)
    {
        if (!btn) return;

        // Click = single step
        btn.onClick.AddListener(() => { if (IsEditMode()) MoveTarget(dir); });

        var state = new MoveHoldState { dir = dir, isHeld = false, nextRepeatAt = 0f };
        _moveHolds[btn] = state;

        var tracker = btn.gameObject.GetComponent<ButtonHoldTracker>();
        if (!tracker) tracker = btn.gameObject.AddComponent<ButtonHoldTracker>();
        tracker.OnHoldStart += () =>
        {
            state.isHeld = true;
            if (IsEditMode()) MoveTarget(state.dir); // immediate step
            state.nextRepeatAt = Time.unscaledTime + holdDelay;
        };
        tracker.OnHoldEnd += () => state.isHeld = false;
    }

    void SetupRotateButton(Button btn, float degreesPerStep)
    {
        if (!btn) return;

        btn.onClick.AddListener(() => { if (IsEditMode()) RotateTarget(degreesPerStep); });

        var state = new RotHoldState { degrees = degreesPerStep, isHeld = false, nextRepeatAt = 0f };
        _rotHolds[btn] = state;

        var tracker = btn.gameObject.GetComponent<ButtonHoldTracker>();
        if (!tracker) tracker = btn.gameObject.AddComponent<ButtonHoldTracker>();
        tracker.OnHoldStart += () =>
        {
            state.isHeld = true;
            if (IsEditMode()) RotateTarget(state.degrees); // immediate step
            state.nextRepeatAt = Time.unscaledTime + holdDelay;
        };
        tracker.OnHoldEnd += () => state.isHeld = false;
    }

    void UpdateMoveHoldRepeats()
    {
        if (!IsEditMode() || _target == null) return;

        float now = Time.unscaledTime;
        foreach (var kv in _moveHolds)
        {
            var s = kv.Value;
            if (!s.isHeld) continue;

            if (now >= s.nextRepeatAt)
            {
                MoveTarget(s.dir);
                s.nextRepeatAt = now + repeatRate;
            }
        }
    }

    void UpdateRotateHoldRepeats()
    {
        if (!IsEditMode() || _target == null) return;

        float now = Time.unscaledTime;
        foreach (var kv in _rotHolds)
        {
            var s = kv.Value;
            if (!s.isHeld) continue;

            if (now >= s.nextRepeatAt)
            {
                RotateTarget(s.degrees);
                s.nextRepeatAt = now + repeatRate;
            }
        }
    }

    // ---------- Helpers ----------

    bool IsEditMode() => editModeToggle ? editModeToggle.isOn : true;
    bool IsDirectionModeOn() => directionModeToggle ? directionModeToggle.isOn : false;

    void SyncUIVisibility()
    {
        bool on = IsEditMode();
        if (fallbackControlsRoot) fallbackControlsRoot.SetActive(on && !_hasRightStick);
    }

    void CacheTarget()
    {
        if (spawner == null) { _target = null; return; }
        _target = spawner.spawnAnchor != null ? spawner.spawnAnchor : spawner.transform;
    }

    void TryFindControllers()
    {
        _rightController = GetController(true);
        _leftController = GetController(false);
    }

    static InputDevice GetController(bool right)
    {
        var list = new List<InputDevice>();
        var mask = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand |
                   (right ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left);
        InputDevices.GetDevicesWithCharacteristics(mask, list);
        return list.Count > 0 ? list[0] : default;
    }

    static bool IsValid(InputDevice d) => d.isValid;

    static bool HasPrimary2DAxis(InputDevice d)
    {
        if (!d.isValid) return false;
        Vector2 dummy;
        return d.TryGetFeatureValue(CommonUsages.primary2DAxis, out dummy);
    }
}