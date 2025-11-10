using UnityEngine;
using System.Collections.Generic;

public class Ragdoll : MonoBehaviour
{
    private bool activated = false;
    List<HingeJoint2D> allHinges = new List<HingeJoint2D>();

    private void Awake()
    {
        HingeJoint2D[] colliders = GetComponentsInChildren<HingeJoint2D>();
        allHinges.AddRange(colliders);
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
        foreach (HingeJoint2D hinge in allHinges)
        {
            hinge.useLimits = toggle;
        }
    }
    
}
