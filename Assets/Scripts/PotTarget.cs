using UnityEngine;

public enum SeedType { Flower, Weed }

public class PotTarget : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private SeedType requiredSeed = SeedType.Flower;

    [Header("FX (optional)")]
    [SerializeField] private GameObject correctVFX;
    [SerializeField] private GameObject wrongVFX;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip wrongClip;

    private SeedMiniGameScoreManager _scoreManager;

    public void Initialize(SeedMiniGameScoreManager scoreMgr)
    {
        _scoreManager = scoreMgr;
    }

    public void RegisterHit(SeedType seed)
    {
        bool correct = (seed == requiredSeed);

        // visuals / audio
        if (sfx)
            sfx.PlayOneShot(correct ? correctClip : wrongClip);
        if (correctVFX) correctVFX.SetActive(correct);
        if (wrongVFX) wrongVFX.SetActive(!correct);

        _scoreManager?.ReportHit(correct);

        // Optionally destroy on hit (or disable collider) so it can't be scored twice
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;
        // You can also add a small delay before despawn if you want to show VFX
        Destroy(gameObject, 1.0f);
    }
}
