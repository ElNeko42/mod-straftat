using UnityEngine;

namespace STRAFTATCrouchSounds
{
    public class CrouchSoundTracker : MonoBehaviour
    {
        FirstPersonController _fpc;
        float _crouchTimer;
        bool  _soundsActive;
        float _clipTimer;

        void Start()
        {
            _fpc = GetComponent<FirstPersonController>();
        }

        void Update()
        {
            if (_fpc == null) return;

            if (!_fpc.isCrouching)
            {
                // Player stood up — reset everything
                if (_crouchTimer > 0f || _soundsActive)
                {
                    _crouchTimer  = 0f;
                    _soundsActive = false;
                    _clipTimer    = 0f;
                }
                return;
            }

            _crouchTimer += Time.deltaTime;
            if (_crouchTimer < 3f) return;

            // First sound after 3 s
            if (!_soundsActive)
            {
                _soundsActive = true;
                PlayRandomTaunt();
                return;
            }

            // Chain sounds: wait for current clip to finish, then play another
            _clipTimer -= Time.deltaTime;
            if (_clipTimer <= 0f)
                PlayRandomTaunt();
        }

        void PlayRandomTaunt()
        {
            var clips = _fpc.tauntClip;
            if (clips == null || clips.Length == 0) return;

            var clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            _fpc.audio.PlayOneShot(clip);
            _clipTimer = clip.length;

            Plugin.Log.LogInfo($"[CrouchSounds] Sonido: {clip.name} ({clip.length:F1}s)");
        }
    }
}
