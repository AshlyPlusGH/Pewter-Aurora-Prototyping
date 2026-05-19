using System;
using UnityEngine;

namespace Ash
{
    public class Soundtrack : MonoBehaviour
    {
        [SerializeField] private bool debug;
        private void Log(string contents){ if (debug){ Debug.Log(contents); }}

        [Space(10)]

        [SerializeField] private AudioSource source;

        public GenericDictionary<ENUM_SountrackTag,AudioClip> tracks = new();

        public void PlayTrackByTag(ENUM_SountrackTag tag){ source.clip = tracks[tag]; source.Play(); }
        public void PlayTrackByPosition(int pos)
        {
                Log("Playing Track " + pos);
            pos += 1;
            var values = (ENUM_SountrackTag[])Enum.GetValues(typeof(ENUM_SountrackTag));
            
            PlayTrackByTag(values[pos]);
        }
        public void PlayTrack(AudioClip audioClip){ source.clip = audioClip; source.Play(); }
    }

    public enum ENUM_SountrackTag
    {
        None,
        Track_0,
        Track_1,
        Track_2,
        Track_3,
        Track_4,
        Track_5,
        Track_6,
        Etc
    }
}