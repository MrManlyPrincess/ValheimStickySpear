﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimCombatIncentives.Extensions;
using static ValheimCombatIncentives.ValheimCombatIncentives;

namespace ValheimCombatIncentives.Patches
{
    [HarmonyPatch(typeof(Character))]
    public static class CharacterPatch
    {
        [HarmonyPatch(nameof(Character.RPC_Damage))]
        [HarmonyPrefix]
        public static void PrefixOnDamage(Character __instance, long sender, HitData hit,
            ref Tuple<bool, float> __state)
        {
            __state = new Tuple<bool, float>(__instance.m_baseAI && !__instance.m_baseAI.m_alerted &&
                                             hit.m_backstabBonus > 1f && Time.time - __instance.m_backstabTime > 300.0f,
                __instance.GetHealth());
        }

        [HarmonyPatch(nameof(Character.RPC_Damage))]
        [HarmonyPostfix]
        public static void PostfixOnDamage(Character __instance, long sender, HitData hit, Tuple<bool, float> __state)
        {
            var attacker = hit.GetAttacker() as Player;
            if (!attacker) return;

            var experienceBonus = Utils.GetExperienceBonusFromDamage(__instance,
                hit,
                DamageExperienceMultiplier.Value);


            var isSecondaryAttack = attacker.GetCurrentWeapon().HaveSecondaryAttack() &&
                                    attacker.m_currentAttack.m_attackAnimation == attacker.GetCurrentWeapon().m_shared
                                        .m_secondaryAttack?.m_attackAnimation;

            var isSurpriseAttack = __state.Item1;
            if (isSurpriseAttack)
            {
                var preAttackVictimHealth = __state.Item2;
                HandleSneakBonus(__instance, preAttackVictimHealth, hit, attacker);
            }

            experienceBonus *= isSecondaryAttack ? SecondaryAttackMultiplier.Value : 1f;
            experienceBonus *= hit.m_skill == Skills.SkillType.Knives && isSurpriseAttack
                ? KnifeSurpriseAttackMultiplier.Value
                : 1f;

            var distance = Vector3.Distance(__instance.GetTransform().position, attacker.GetTransform().position);
            var drawPercentage = -1f;
            var drawPercentageMultiplier = -1f;

            if (hit.IsFromRangedWeapon())
            {
                HandleRangedWeapon(hit.m_skill,
                    attacker,
                    distance,
                    ref drawPercentage,
                    ref experienceBonus,
                    out drawPercentageMultiplier);
            }

            attacker.RaiseSkill(hit.m_skill, experienceBonus);

            var rangedWeaponAddition = hit.IsFromRangedWeapon()
                ? $"\nDistance: {distance}\n" +
                  $"Draw Percentage: {drawPercentage} (Mult: {drawPercentageMultiplier})"
                : string.Empty;

            Jotunn.Logger.LogInfo($"\nGranted {hit.m_skill} {experienceBonus} experience!\n" +
                                  $"Damage Dealt: {hit.GetTotalDamage()}\n" +
                                  $"Is Secondary Attack: {isSecondaryAttack}\n" +
                                  $"Is Surprise Attack: {isSurpriseAttack}" +
                                  rangedWeaponAddition);

            Utils.ShowExperienceNotification(hit.m_skill, experienceBonus);
        }

        private static void HandleSneakBonus(Character __instance, float preAttackVictimHealth, HitData hit,
            Player attacker)
        {
            var sneakExperienceBonus = Utils.GetExperienceBonusFromDamage(__instance, hit,
                SneakDamageExperienceMultiplier.Value);

            var attackWillKill = hit.GetTotalDamage() >= preAttackVictimHealth;
            sneakExperienceBonus *= attackWillKill ? AssassinateExperienceMultiplier.Value : 1f;


            Jotunn.Logger.LogInfo($"\nGranted {hit.m_skill} {sneakExperienceBonus} experience!\n" +
                                  $"Damage Dealt: {hit.GetTotalDamage()}\n" +
                                  $"Is Assassination: {attackWillKill}");

            attacker.RaiseSkill(Skills.SkillType.Sneak, sneakExperienceBonus);
            Utils.ShowExperienceNotification(Skills.SkillType.Sneak, sneakExperienceBonus);
        }

        private static void HandleRangedWeapon(Skills.SkillType skill,
            Humanoid attacker,
            float distance,
            ref float drawPercentage,
            ref float experienceBonus,
            out float drawPercentageMultiplier)
        {
            drawPercentageMultiplier = -1;

            var multiplier = skill == Skills.SkillType.Spears
                ? SpearDistanceMultiplier.Value
                : BowDistanceMultiplier.Value;
            experienceBonus *= 1 + distance * multiplier;

            if (skill != Skills.SkillType.Bows) return;

            drawPercentage = attacker.m_currentAttack.m_attackDrawPercentage;
            drawPercentageMultiplier = Utils.MapToRange(drawPercentage, 0f, 1f,
                DrawMinMultiplier.Value, DrawMaxMultiplier.Value);

            experienceBonus *= drawPercentageMultiplier;
        }
    }
}
