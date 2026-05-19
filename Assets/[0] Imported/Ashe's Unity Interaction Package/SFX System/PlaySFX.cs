using UnityEngine;

namespace Ash
{
    public class PlaySFX : MonoBehaviour
    {
        public void PlaySFX2D(AudioClip clip){ SFX.Play2D(clip); }

        public void PlaySFXRandom2D(soDATA_SFXGroup group){ SFX.Play2D(SFXGroup.GetRandomClip(group)); }
    }
}