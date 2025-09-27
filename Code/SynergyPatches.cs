using Alexandria.ItemAPI;
using HarmonyLib;
using MonoMod.Cil;
using Ralsei.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Mono.Cecil.Cil;

namespace Ralsei
{
    public static class SynergyPatches
    {

        [HarmonyPatch(typeof(RadialCharmItem), nameof(RadialCharmItem.AffectEnemy))]
        public class AffectEnemy_StartPatch
        {
            [HarmonyPostfix]
            public static void Postfix(RadialCharmItem __instance, AIActor target)
            {
                if (__instance.LastOwner.PlayerHasActiveSynergy("Pacify"))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        target.ApplyEffect(new RalseiCharmable()
                        {
                            duration = 10000,
                        });
                    }
                }
            }
        }
        [HarmonyPatch(typeof(EstusFlaskItem), nameof(EstusFlaskItem.HandleDrinkEstus))]
        public class Glug
        {
            [HarmonyPostfix]
            public static void Postfix(EstusFlaskItem __instance, PlayerController user)
            {
                if (user.PlayerHasActiveSynergy("Group Huddle"))
                {
                    var c = Toolbox.isCakedUp();
                    if (c)
                    {
                        c.AllCharmedEnemies.ForEach(self =>
                        {
                            self.aiActor.healthHaver.FullHeal();
                            self.DoHealParticles();
                        });
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CardboardBoxItem), nameof(CardboardBoxItem.DoEffect))]
        public class CardboarxBox_PutOn
        {
            [HarmonyPostfix]
            public static void Postfix(CardboardBoxItem __instance, PlayerController user)
            {
                var c = Toolbox.isCakedUp();
                if (c)
                {
                    c.AllCharmedEnemies.ForEach(self =>
                    {
                        float a = (float)(self.specRigidbody.HitboxPixelCollider.Height) / 32;

                        var Box = self.aiActor.SmartPlayEffectOnActor(__instance.prefabToAttachToPlayer, new Vector3(-0.5f, 0.5f + a));
                        self.AttachedEffectsAdditional.Add("cardboardBox", Box);
                        var instanceBoxSprite = Box.GetComponent<tk2dSpriteAnimator>();
                        instanceBoxSprite.Play("cardboard_on");
                    });
                }
            }
        }
        [HarmonyPatch(typeof(CardboardBoxItem), nameof(CardboardBoxItem.BreakStealth))]
        public class CardboarxBox_PutOff
        {
            [HarmonyPostfix]
            public static void Postfix(CardboardBoxItem __instance, PlayerController obj)
            {
                var c = Toolbox.isCakedUp();
                if (c)
                {
                    c.AllCharmedEnemies.ForEach(self =>
                    {
                        GameObject box;
                        self.AttachedEffectsAdditional.TryGetValue("cardboardBox", out box);
                        if (box != null)
                        {
                            self.aiActor.DeregisterAttachedObject(box, false);
                            box.GetComponent<tk2dSprite>().spriteAnimator.PlayAndDestroyObject("cardboard_off", null);
                            self.AttachedEffectsAdditional.Remove("cardboardBox");
                        }
                    });
                }
            }
        }

        [HarmonyPatch(typeof(YellowChamberItem), nameof(YellowChamberItem.OnEnteredCombat))]
        public class YellowChamberItem_Enter
        {
            [HarmonyPostfix]
            public static void Postfix(YellowChamberItem __instance)
            {
                if (__instance.m_currentlyCharmedEnemy)
                {
                    RalseiCharmable.InvalidSpecificEnemies.Add(__instance.m_currentlyCharmedEnemy);
                }
            }
        }


        [HarmonyPatch]
        private static class RegenerationPassiveItemPlayerDealtDamagenPatch
        {
            [HarmonyPatch(typeof(RegenerationPassiveItem), nameof(RegenerationPassiveItem.PlayerDealtDamage))]
            [HarmonyILManipulator]
            private static void GameUIAmmoControllerUpdateUIGunIL(ILContext il)
            {
                ILCursor cursor = new ILCursor(il);

                if (!cursor.TryGotoNext(MoveType.After, 
                    instr => instr.MatchCallvirt<HealthHaver>("ApplyHealing")))
                    return;
  
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Call, typeof(RegenerationPassiveItemPlayerDealtDamagenPatch).GetMethod("UpdateHP", BindingFlags.Static | BindingFlags.NonPublic));
            }

            private static void UpdateHP(RegenerationPassiveItem regenerationPassiveItem)
            {
                if (regenerationPassiveItem.Owner && regenerationPassiveItem.Owner.PlayerHasActiveSynergy("Absorb"))
                {
                    var _ = Toolbox.isCakedUp();
                    if (_ == null) { return; }
                    _.StoredHearts += 3;
                }
            }
        }
    }
}
