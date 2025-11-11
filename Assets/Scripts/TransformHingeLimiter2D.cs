using UnityEngine;

/// <summary>
/// Provides a transform-based angular limit for a 2D hinge so that multiple spins do not accumulate
/// large Rigidbody2D rotations. Keeps the native HingeJoint2D for positional constraints but overrides
/// its limit handling by clamping against Transform.eulerAngles instead of Rigidbody2D.rotation.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class TransformHingeLimiter2D : MonoBehaviour
{
    [Tooltip("Optional explicit reference. If omitted the limiter uses the HingeJoint2D on this body.")]
    [SerializeField] private HingeJoint2D hinge;

    [Tooltip("Manually override the connected body used for the reference angle.")]
    [SerializeField] private Rigidbody2D connectedBodyOverride;

    [Header("Limit Settings")]
    [SerializeField] private bool useLimits = true;
    [SerializeField] private float lowerAngle = -90f;
    [SerializeField] private float upperAngle = 90f;

    [Tooltip("Instantly snap back inside the limit when violated. If disabled the limiter rewinds gradually.")]
    [SerializeField] private bool hardClamp = true;

    [Tooltip("Applies when Hard Clamp is disabled. Degrees per second used to move back inside bounds.")]
    [SerializeField] private float rewindSpeed = 360f;

    [Tooltip("Zero out angular velocity whenever we correct the rotation.")]
    [SerializeField] private bool zeroOutAngularVelocity = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (hinge == null)
        {
            hinge = GetComponent<HingeJoint2D>();
        }

        if (hinge != null && hinge.useLimits)
        {
            Debug.LogWarning($"{name}: TransformHingeLimiter2D disables built-in hinge limits. The hinge limits will now be managed by transform angles.");
            hinge.useLimits = false;
        }
    }

    private void FixedUpdate()
    {
        if (!useLimits)
        {
            return;
        }

        Transform referenceTransform = ResolveReferenceTransform();
        float referenceAngle = referenceTransform != null ? referenceTransform.eulerAngles.z : 0f;
        float selfAngle = transform.eulerAngles.z;
        float relativeAngle = Mathf.DeltaAngle(referenceAngle, selfAngle);
        float clampedAngle = Mathf.Clamp(relativeAngle, lowerAngle, upperAngle);

        if (Mathf.Approximately(relativeAngle, clampedAngle))
        {
            return;
        }

        float targetWorldAngle = referenceAngle + clampedAngle;
        ApplyCorrection(targetWorldAngle, selfAngle);

        if (zeroOutAngularVelocity)
        {
            rb.angularVelocity = 0f;
        }
    }

    private Transform ResolveReferenceTransform()
    {
        if (connectedBodyOverride != null)
        {
            return connectedBodyOverride.transform;
        }

        if (hinge != null && hinge.connectedBody != null)
        {
            return hinge.connectedBody.transform;
        }

        return null;
    }

    private void ApplyCorrection(float targetWorldAngle, float currentWorldAngle)
    {
        float newAngle = hardClamp
            ? targetWorldAngle
            : Mathf.MoveTowardsAngle(currentWorldAngle, targetWorldAngle, rewindSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(newAngle);
    }

    public void SetLimits(float lower, float upper)
    {
        lowerAngle = lower;
        upperAngle = upper;
    }

    public void EnableLimits(bool enabled)
    {
        useLimits = enabled;
    }
}
