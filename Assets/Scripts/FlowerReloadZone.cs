using BioTools;
using UnityEngine;

public class FlowerReloadZone : MonoBehaviour
{
    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        var spreader = FindFirstObjectByType<SeedSpreaderTool>();
        if (spreader) spreader.ReloadPellets(50, "Flower Seed");
    }
}
