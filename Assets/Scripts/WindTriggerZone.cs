using UnityEngine;
using BioTools;

[RequireComponent(typeof(Collider))]
public class WindZoneTrigger : MonoBehaviour
{
    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        var spreader = other.GetComponentInParent<SeedSpreaderTool>();
        if (spreader) spreader.SetInWindZone(true);
    }

    void OnTriggerExit(Collider other)
    {
        var spreader = other.GetComponentInParent<SeedSpreaderTool>();
        if (spreader) spreader.SetInWindZone(false);
    }
}
