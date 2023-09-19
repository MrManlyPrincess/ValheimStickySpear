using HarmonyLib;
using Jotunn;

namespace ValheimStickySpear.Patches
{
    [HarmonyPatch(typeof(ItemDrop))]
    public class ItemDropPatch
    {
        [HarmonyPatch(nameof(ItemDrop.OnPlayerDrop))]
        [HarmonyPostfix]
        public static void OnPlayerDrop(ItemDrop __instance)
        {
            var isSpear = __instance.m_itemData.IsWeapon() &&
                          __instance.m_itemData.m_shared.m_skillType == Skills.SkillType.Spears;

            var wasThrown = __instance.m_itemData.m_crafterName.Contains("||THROWN||");

            if (!isSpear || !wasThrown) return;

            __instance.m_itemData.m_crafterName = __instance.m_itemData.m_crafterName
                .Replace("||THROWN||", "");

            Logger.LogInfo("It was thrown!");
            DisableAutoPickUpForOthers(__instance);
        }

        private static void DisableAutoPickUpForOthers(ItemDrop itemDrop)
        {
            itemDrop.m_nview.ClaimOwnership();
            itemDrop.m_autoPickup = false;

            ZDOMan.instance.ForceSendZDO(ZDOMan.instance.GetMyID(),
                itemDrop.m_nview.GetZDO().m_uid);

        }
    }
}
