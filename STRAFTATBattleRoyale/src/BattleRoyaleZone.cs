using UnityEngine;

namespace STRAFTATBattleRoyale
{
    public static class BattleRoyaleZone
    {
        static int    _ticksUntilZone;
        static bool   _active;
        static float  _radius;
        static Vector3 _center;
        static GameObject _cylinder;
        static PlayerHealth[] _players = System.Array.Empty<PlayerHealth>();

        public static void Reset()
        {
            _radius         = Plugin.ZoneInitialRadius.Value;
            _ticksUntilZone = Plugin.SecondsUntilZone.Value * 60;
            _active         = false;
            _center         = Vector3.zero;

            if (_cylinder) GameObject.Destroy(_cylinder);

            var tm = FishNet.InstanceFinder.TimeManager;
            tm.OnTick -= TickCountdown;
            tm.OnTick -= TickZone;
            tm.OnTick += TickCountdown;
            tm.OnTick += TickZone;
        }

        static void TickCountdown()
        {
            if (_ticksUntilZone > 0)
            {
                _ticksUntilZone--;
            }
            else if (!_active)
            {
                _active = true;
                if (_cylinder) GameObject.Destroy(_cylinder);
                Spawn();
            }
        }

        static void TickZone()
        {
            if (!_active) return;

            var tm = FishNet.InstanceFinder.TimeManager;

            if (tm.Tick % 60 == 0 && FishNet.InstanceFinder.IsServer)
            {
                foreach (var ph in _players)
                {
                    if (!ph) continue;
                    Vector3 pos  = ph.transform.position;
                    float   dist = Vector3.Distance(new Vector3(pos.x, 0, pos.z),
                                                    new Vector3(_center.x, 0, _center.z));
                    if (dist > _radius)
                        ph.RemoveHealth(Plugin.ZoneDamagePerSecond.Value / 25f);
                }
            }

            if (!_cylinder) return;

            if (_radius > Plugin.ZoneFinalRadius.Value)
                _radius -= Plugin.ZoneShrinkRate.Value / 60f;

            _cylinder.transform.localScale = new Vector3(_radius * 2, 200f, _radius * 2);
        }

        static void Spawn()
        {
            _players = GameObject.FindObjectsOfType<PlayerHealth>();

            foreach (var ph in _players)
            {
                if (ph != null && !ph.isKilled) _center += ph.transform.position;
            }
            _center /= Mathf.Max(1, _players.Length);

            _cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _cylinder.GetComponent<Collider>().enabled = false;
            _cylinder.transform.position   = new Vector3(_center.x, 0, _center.z);
            _cylinder.transform.localScale = new Vector3(_radius * 2, 200f, _radius * 2);

            string[] rgb = Plugin.ZoneColor.Value.Split(", ");
            Color color  = new Color(
                float.Parse(rgb[0]) / 255f,
                float.Parse(rgb[1]) / 255f,
                float.Parse(rgb[2]) / 255f,
                Plugin.ZoneAlpha.Value
            );

            var mat = _cylinder.GetComponent<MeshRenderer>().material;
            mat.shader = Shader.Find("UI/Default");
            mat.SetColor("_Color", color);
            mat.SetInt("_Cull", 0);

            Plugin.Log.LogInfo($"[BattleRoyale] Zona activa. Centro: {_center}  Radio: {_radius}");
        }
    }
}
