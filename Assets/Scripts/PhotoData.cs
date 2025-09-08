using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class PhotoData
{
    public string speciesName;
    public int rarityLevel;
    [TextArea] public string Description;
    public Image photoTaken;
    public AnimationClip animationClip;
    public string speicesTag;


}
