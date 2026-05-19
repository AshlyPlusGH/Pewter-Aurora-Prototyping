using UnityEngine;

namespace Ash {
    public class SFXOneShot : MonoBehaviour
    {
        public AudioSource audioSource;

        public void Update()
        {
            if (audioSource.isPlaying){ return; }

            Destroy(gameObject);
        }
    }
}