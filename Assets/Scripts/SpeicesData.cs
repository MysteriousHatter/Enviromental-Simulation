using UnityEngine;

[CreateAssetMenu(menuName = "Eco/Species Data")]
public class SpeicesData : ScriptableObject
{
    public string Name;
    [Range(1, 5)] public int Rarity;
    [TextArea] public string Description;
    public AnimationClip[] animationClips;
}
