using UnityEngine;

namespace Ash {
    public class SFX : MonoBehaviour
    {
        [SerializeField] private bool debug;
            private void Log(string contents){ if (debug){ Debug.Log(contents + " at: " + name); }}

        [Space(10)]

        [SerializeField] private GameObject SFXOneShotPrefab;
        [SerializeField] private AudioSource audioSource;

        private static SFX instance;

        void Awake()
        {
            if (instance == null){ instance = this; }
            else { Destroy(this); }
        }

        public static void Play(AudioClip clip, Vector3 position = new Vector3())
        {
            SFXOneShot oneShot = Instantiate(instance.SFXOneShotPrefab, position, Quaternion.identity).GetComponent<SFXOneShot>();
            oneShot.audioSource.clip = clip;
            oneShot.audioSource.Play();
        }
        public static void Play2D(AudioClip clip)
        {
            instance.audioSource.clip = clip;
            instance.audioSource.Play();
        }

        public static void Play2DRandomFromGroup(soDATA_SFXGroup group)
        {
            Play2D(SFXGroup.GetRandomClip(group));
        }
    }
}