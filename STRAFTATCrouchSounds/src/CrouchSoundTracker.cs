using UnityEngine;
using FishNet;

namespace STRAFTATNuevasMecanicas
{
    public class CrouchSoundTracker : MonoBehaviour
    {
        FirstPersonController        _fpc;
        FishNet.Object.NetworkObject _nob;
        float _crouchTimer;
        bool  _soundsActive;
        float _clipTimer;

        void Start()
        {
            _fpc = GetComponent<FirstPersonController>();
            _nob = GetComponent<FishNet.Object.NetworkObject>();
            Plugin.Log.LogInfo($"[CrouchSounds] Listo â€” IsServer:{InstanceFinder.IsServer}  IsClient:{InstanceFinder.IsClient}  IsOwner:{_nob?.IsOwner}");
        }

        void Update()
        {
            if (_fpc == null) return;

            if (!_fpc.isCrouching)
            {
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

            if (!_soundsActive)
            {
                _soundsActive = true;
                PlayRandomTaunt();
                return;
            }

            _clipTimer -= Time.deltaTime;
            if (_clipTimer <= 0f)
                PlayRandomTaunt();
        }

        void PlayRandomTaunt()
        {
            var clips = _fpc.tauntClip;
            if (clips == null || clips.Length == 0) return;

            int maxIdx = Mathf.Min(clips.Length, 9); // solo sonidos de teclas 1-9
            int idx    = Random.Range(0, maxIdx);
            var clip   = clips[idx];
            if (clip == null) return;

            _clipTimer = clip.length;

            bool isServer = InstanceFinder.IsServer;
            bool isClient = InstanceFinder.IsClient;
            bool isOwner  = _nob != null && _nob.IsOwner;

            if (isServer)
            {
                // Host o servidor: PlaySoundObservers manda a todos los clientes y reproduce localmente
                _fpc.PlaySoundObservers(idx);
            }
            else if (isClient && isOwner)
            {
                // Cliente propietario: manda al servidor, el servidor hace PlaySoundObservers para todos
                _fpc.PlaySoundServer(idx);
            }
            else
            {
                // Sin red activa (training offline) o sin ownership: reproducciÃ³n local
                _fpc.audio.PlayOneShot(clip);
            }

            Plugin.Log.LogInfo($"[CrouchSounds] Sonido {idx + 1}/9 '{clip.name}' | isServer:{isServer} isClient:{isClient} isOwner:{isOwner}");
        }
    }
}
