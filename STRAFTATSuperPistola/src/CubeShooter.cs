using System.Collections;
using UnityEngine;

namespace STRAFTATSuperPistola
{
    public static class CubeShooter
    {
        static Material _mat;

        static Material GetMat()
        {
            if (_mat != null) return _mat;
            _mat = new Material(Shader.Find("Standard")) { color = Color.yellow };
            return _mat;
        }

        public static void Shoot(WeaponHandSpawner spawner)
        {
            // Origen: punta del cañón o, si no existe, la cámara
            Transform origin = spawner.muzzleFlashPoint != null
                ? spawner.muzzleFlashPoint
                : (spawner.cam != null ? spawner.cam.transform : spawner.transform);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "SP3000_Projectile";
            cube.transform.position   = origin.position + origin.forward * 0.5f;
            cube.transform.localScale = Vector3.one * 0.7f;
            cube.GetComponent<Renderer>().material = GetMat();

            // Trigger para atravesar paredes
            cube.GetComponent<Collider>().isTrigger = true;

            // Rigidbody cinemático — necesario para que OnTriggerEnter funcione
            var rb = cube.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;

            var proj = cube.AddComponent<CubeProjectile>();
            proj.Direction = origin.forward;

            Plugin.Log.LogInfo("[SuperPistola] Cubo disparado.");
        }
    }

    public class CubeProjectile : MonoBehaviour
    {
        public Vector3 Direction;

        const float Speed    = 5f;
        const float Damage   = 100f;
        const float Lifetime = 20f;

        float _age;

        void Update()
        {
            _age += Time.deltaTime;
            if (_age >= Lifetime) { Destroy(gameObject); return; }

            transform.position += Direction * Speed * Time.deltaTime;
            transform.Rotate(Vector3.up, 60f * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            var ph = other.GetComponentInParent<PlayerHealth>();
            if (ph == null || ph.isKilled) return;

            ph.RemoveHealth(Damage);
            Plugin.Log.LogInfo($"[SuperPistola] Impacto en {ph.name} — {Damage} daño.");
            Destroy(gameObject);
        }
    }
}
