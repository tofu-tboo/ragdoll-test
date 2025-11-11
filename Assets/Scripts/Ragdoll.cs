using UnityEngine;
using System.Collections.Generic;

public class Ragdoll : MonoBehaviour
{
    private bool activated = false;
    // List<HingeJoint2D> allHinges = new List<HingeJoint2D>();
    List<TransformHingeLimiter2D> allHinges = new List<TransformHingeLimiter2D>();

    private void Awake()
    {
        TransformHingeLimiter2D[] hinges = GetComponentsInChildren<TransformHingeLimiter2D>();
        allHinges.AddRange(hinges);
    }

    public void UpdateModeToggle(bool toggle)
    {
        Muscle[] muscles = GetComponentsInChildren<Muscle>();
        foreach (Muscle muscle in muscles)
        {
            muscle.musclesActive = toggle;
        }
        SetHingeLimit(!toggle);
        activated = !toggle;

    }

    public void UpdateMode()
    {
        //
        Muscle[] muscles = GetComponentsInChildren<Muscle>();
        foreach (Muscle muscle in muscles)
        {
            muscle.musclesActive = activated;
        }
        SetHingeLimit(!activated);
        activated = !activated;

    }

    private void SetHingeLimit(bool toggle)
    {
        foreach (TransformHingeLimiter2D hinge in allHinges)
        {
            // hinge.useLimits = toggle;
            // hinge.useLimits = false;
            hinge.EnableLimits(toggle);
        }
    }
    
}
