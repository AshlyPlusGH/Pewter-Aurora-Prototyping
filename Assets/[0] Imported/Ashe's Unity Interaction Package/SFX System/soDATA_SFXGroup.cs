using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class soDATA_SFXGroup : ScriptableObject
{
    [SerializeField] private List<AudioClip> soDATA_Clips = new();

    public List<AudioClip> STAT_Clips => soDATA_Clips;
}

public class SFXGroup
{
    public static AudioClip GetRandomClip(soDATA_SFXGroup group){ return group.STAT_Clips[Random.Range(0,group.STAT_Clips.Count)]; }

    public static soDATA_SFXGroup GetGroup(string groupName){ return Resources.Load<soDATA_SFXGroup>("SFX_Library/Fx Groups/" + groupName); }
}