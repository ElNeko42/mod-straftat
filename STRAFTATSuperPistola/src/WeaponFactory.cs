using FishNet;
using UnityEngine;

namespace STRAFTATSuperPistola
{
    public static class WeaponFactory
    {
        public static GameObject ClonePrefab { get; private set; }

        public static void Initialize()
        {
            if (ClonePrefab != null) return;

            if (!SpawnerManager.NameToWeaponDict.TryGetValue("Bender", out var bender))
            {
                Plugin.Log.LogWarning("[SuperPistola] 'Bender' no encontrada.");
                return;
            }

            ClonePrefab = Object.Instantiate(bender);
            ClonePrefab.name = "Super Pistola 3000";
            ClonePrefab.SetActive(false);
            Object.DontDestroyOnLoad(ClonePrefab);

            foreach (var rend in ClonePrefab.GetComponentsInChildren<Renderer>(true))
                foreach (var mat in rend.materials)
                    mat.color = Color.red;

            ClonePrefab.AddComponent<SuperPistolaMarker>();

            SpawnerManager.NameToWeaponDict["Super Pistola 3000"] = ClonePrefab;
            var list = new System.Collections.Generic.List<GameObject>(SpawnerManager.AllWeapons);
            list.Add(ClonePrefab);
            SpawnerManager.AllWeapons = list.ToArray();

            Plugin.Log.LogInfo($"[SuperPistola] Creada. Total armas: {SpawnerManager.AllWeapons.Length}");

            RegisterWithFishNet();
        }

        public static void RegisterWithFishNet()
        {
            if (ClonePrefab == null) return;

            var nm = InstanceFinder.NetworkManager;
            if (nm == null) { Plugin.Log.LogInfo("[SuperPistola] NetworkManager no listo aún."); return; }

            var nob = ClonePrefab.GetComponent<FishNet.Object.NetworkObject>();
            if (nob == null) { Plugin.Log.LogWarning("[SuperPistola] Sin NetworkObject en el clon."); return; }

            // Resetear IDs heredados del Bender (igual que KokiWeapons DeregisterFishnet)
            nob.PrefabId = 0;
            nob.SpawnableCollectionId = 0;

            var collection = nm.SpawnablePrefabs;
            collection.AddObject(nob);
            collection.InitializePrefabRange(0);

            Plugin.Log.LogInfo("[SuperPistola] Registrada en FishNet SpawnablePrefabs.");
        }

        public static void SpawnNearPlayer(FirstPersonController fpc)
        {
            if (ClonePrefab == null) { Plugin.Log.LogWarning("[SuperPistola] ClonePrefab null."); return; }

            Vector3 pos = fpc.playerCameraHolder.transform.position + fpc.dirForward.normalized;
            pos.y -= 0.5f;

            var go = Object.Instantiate(ClonePrefab, pos, Quaternion.identity);
            go.SetActive(true);
            go.name = "Super Pistola 3000";

            go.GetComponent<ItemBehaviour>()?.DispenserDrop(Vector3.zero);
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            if (InstanceFinder.IsServer)
                fpc.ServerManager.Spawn(go);

            Plugin.Log.LogInfo($"[SuperPistola] Spawneada en {pos}");
        }
    }

    public class SuperPistolaMarker : MonoBehaviour { }
}
