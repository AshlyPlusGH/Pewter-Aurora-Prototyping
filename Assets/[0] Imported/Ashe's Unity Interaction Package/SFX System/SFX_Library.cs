using System;
using UnityEngine;

public class SFX_Library : MonoBehaviour
{
    public static SFX_Library instance;

    [SerializeField] private soDATA_SFXGroup exampleSFXGroup;
    public static soDATA_SFXGroup STAT_exampleSFXGroup;

    void Awake(){ Setup(); }
    void Setup()
    {
        instance = this;

        STAT_exampleSFXGroup = exampleSFXGroup;
    }
}