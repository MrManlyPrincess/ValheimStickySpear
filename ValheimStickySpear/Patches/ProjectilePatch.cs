using HarmonyLib;
using UnityEngine;

namespace ValheimStickySpear.Patches
{
    [HarmonyPatch(typeof(Projectile))]
    public class ProjectilePatch
    {
        [HarmonyPatch(nameof(Projectile.SpawnOnHit))]
        [HarmonyPrefix]
        public static void PrefixSpawnOnHit(Projectile __instance, GameObject go, Collider collider,
            ref ItemDrop.ItemData __state)
        {
            __state = __instance.m_spawnItem?.Clone();
            __instance.m_spawnItem = null;
        }

        [HarmonyPatch(nameof(Projectile.SpawnOnHit))]
        [HarmonyPostfix]
        public static void PostfixSpawnOnHit(Projectile __instance, GameObject go, Collider collider,
            ref ItemDrop.ItemData __state)
        {
            if (__state == null) return;

            var transform = __instance.transform;

            var spawnPosition = transform.position + transform.TransformDirection(__instance.m_spawnOffset);
            var spawnRotation = transform.rotation;

            __state.m_crafterName += "||THROWN||";
            var itemDrop = ItemDrop.DropItem(__state, 0, spawnPosition, spawnRotation);
            itemDrop.OnPlayerDrop();
        }
    }
}
