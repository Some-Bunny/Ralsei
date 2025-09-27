using HarmonyLib;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using Dungeonator;

namespace Ralsei
{
    public class BehaviorPatches
    {
        /// <summary>
        /// A lot of these should realistically be transpilers but frankly doing this is quicker than fighting transpilers. 
        /// I have written *1* transpiler and I kinda get how they work but until I'm more confident with them this'll do.
        /// </summary>


        public static IEnumerator WaitUntilActive(AIActor aIActor)
        {
            while (aIActor.State != AIActor.ActorState.Normal)
            {
                yield return null;
            }
            yield return null;
            if (aIActor)
            {
                var c = Toolbox.isCakedUp();
                if (c)
                {
                    
                    aIActor.RalseifyEnemy(c);
                }
            }
            yield break;
        }


        [HarmonyPatch(typeof(SummonEnemyBehavior), nameof(SummonEnemyBehavior.ContinuousUpdate))]
        public class ChangeUpSummonEnemiesBehavior
        {
            [HarmonyPatch]
            public static bool Prefix(SummonEnemyBehavior __instance, ref ContinuousBehaviorResult __result)
            {
                if (AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(__instance.m_aiActor))
                {
                    if (__instance.m_state == SummonEnemyBehavior.State.Summoning)
                    {
                        if (__instance.m_timer <= 0f)
                        {
                            __instance.m_spawnedActor = AIActor.Spawn(__instance.m_enemyPrefab, __instance.m_spawnCell.Value, __instance.m_aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Spawn, true);
                            __instance.m_spawnedActor.StartCoroutine(WaitUntilActive(__instance.m_spawnedActor));
                            __instance.m_spawnedActor.healthHaver.SetHealthMaximum(__instance.m_spawnedActor.healthHaver.GetMaxHealth() * 0.3f);
                            __instance.m_spawnedActor.aiAnimator.PlayDefaultSpawnState();
                            __instance.m_allSpawnedActors.Add(__instance.m_spawnedActor);
                            __instance.m_spawnedActor.CanDropCurrency = false;
                            if (__instance.OverrideCorpse != null)
                            {
                                __instance.m_spawnedActor.CorpseObject = __instance.OverrideCorpse;
                            }
                            if (__instance.BlackPhantomChance > 0f && (__instance.BlackPhantomChance >= 1f || UnityEngine.Random.value < __instance.BlackPhantomChance))
                            {
                                __instance.m_spawnedActor.ForceBlackPhantom = true;
                            }
                            __instance.m_spawnCount++;
                            __instance.m_lifetimeSpawnCount++;
                            if (__instance.m_spawnCount < __instance.m_numToSpawn)
                            {
                                __instance.PrepareSpawn();
                                IntVector2? spawnCell = __instance.m_spawnCell;
                                if (spawnCell != null)
                                {
                                    if (!string.IsNullOrEmpty(__instance.TargetVfx))
                                    {
                                        if (__instance.TargetVfxLoops)
                                        {
                                            __instance.m_aiAnimator.StopVfx(__instance.TargetVfx);
                                        }
                                        AIAnimator aiAnimator = __instance.m_aiAnimator;
                                        string targetVfx = __instance.TargetVfx;
                                        Vector2? position = new Vector2?(Pathfinder.GetClearanceOffset(__instance.m_spawnCell.Value, __instance.m_enemyClearance));
                                        aiAnimator.PlayVfx(targetVfx, null, null, position);
                                    }
                                    __instance.m_timer = __instance.SummonTime;
                                    __result = ContinuousBehaviorResult.Continue;
                                    return false;
                                }
                            }
                            __instance.m_spawnClip = __instance.m_spawnedActor.spriteAnimator.CurrentClip;
                            if (__instance.m_spawnClip != null && __instance.m_spawnClip.wrapMode != tk2dSpriteAnimationClip.WrapMode.Loop)
                            {
                                __instance.m_state = SummonEnemyBehavior.State.WaitingForSummonAnim;
                                __result = ContinuousBehaviorResult.Continue;
                                return false;

                            }
                            if (!string.IsNullOrEmpty(__instance.PostSummonAnim))
                            {
                                __instance.m_state = SummonEnemyBehavior.State.WaitingForPostAnim;
                                __instance.m_aiAnimator.PlayUntilFinished(__instance.PostSummonAnim, false, null, -1f, false);
                                __result = ContinuousBehaviorResult.Continue;
                                return false;
                            }
                            __result = ContinuousBehaviorResult.Finished;
                            return false;

                        }
                    }
                    else if (__instance.m_state == SummonEnemyBehavior.State.WaitingForSummonAnim)
                    {
                        if (!__instance.m_spawnedActor || !__instance.m_spawnedActor.healthHaver || __instance.m_spawnedActor.healthHaver.IsDead || !__instance.m_spawnedActor.spriteAnimator.IsPlaying(__instance.m_spawnClip))
                        {
                            if (!string.IsNullOrEmpty(__instance.PostSummonAnim))
                            {
                                __instance.m_state = SummonEnemyBehavior.State.WaitingForPostAnim;
                                __instance.m_aiAnimator.PlayUntilFinished(__instance.PostSummonAnim, false, null, -1f, false);
                                __result = ContinuousBehaviorResult.Continue;
                                return false;
                            }
                            __result = ContinuousBehaviorResult.Finished;
                            return false;
                        }
                    }
                    else if (__instance.m_state == SummonEnemyBehavior.State.WaitingForPostAnim && !__instance.m_aiActor.spriteAnimator.IsPlaying(__instance.PostSummonAnim))
                    {
                        __result = ContinuousBehaviorResult.Finished;
                        return false;

                    }
                    __result = ContinuousBehaviorResult.Continue;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(MirrorImageController), nameof(MirrorImageController.SetHost))]
        public class ChangeUpMirrorImageController
        {
            [HarmonyPostfix]
            public static void Prefix(MirrorImageController __instance, AIActor host)
            {
                if (AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(host))
                {
                    var c = Toolbox.isCakedUp();
                    if (c)
                    {
                        __instance.aiActor.RalseifyEnemy(c);
                        __instance.StartCoroutine(KillMirror(__instance.aiActor));
                    }
                }
            }
            public static IEnumerator KillMirror(AIActor aIActor)
            {
                yield return new WaitForSeconds(20);
                if (aIActor)
                {
                    aIActor.healthHaver.ApplyDamage(100000f, Vector2.zero, "Mirror Host Death", CoreDamageTypes.None, DamageCategory.Unstoppable, false, null, false);
                }
                yield break;
            }
        }
        [HarmonyPatch(typeof(BuffEnemiesBehavior), nameof(BuffEnemiesBehavior.Update))]
        public class ChangeUpBuffEnemiesBehavior
        {
            [HarmonyPrefix]
            public static bool Prefix(BuffEnemiesBehavior __instance, ref BehaviorResult __result)
            {
                if (AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(__instance.m_aiActor))
                {

                    BehaviorResult behaviorResult = BehaviorResult.Continue;



                    //(__instance as BasicAttackBehavior).Update();
                    if (behaviorResult != BehaviorResult.Continue)
                    {
                        __result =  behaviorResult;
                        return false;
                    }
                    if (!__instance.IsReady())
                    {
                        __result = BehaviorResult.Continue;
                        return false;
                    }
                    if (__instance.m_searchTimer > 0f)
                    {
                        __result = BehaviorResult.Continue;
                        return false;
                    }
                    List<AIActor> allEnemies = new List<AIActor>();
                    allEnemies.AddRange(AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors);
                    //__instance.m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref BuffEnemiesBehavior.s_activeEnemies);
                    BuffEnemiesBehavior.s_activeEnemies = allEnemies;
                    BuffEnemiesBehavior.s_activeEnemies.Remove(__instance.m_aiActor);
                    for (int i = BuffEnemiesBehavior.s_activeEnemies.Count - 1; i >= 0; i--)
                    {
                        if (!__instance.IsGoodBuffTarget(BuffEnemiesBehavior.s_activeEnemies[i]))
                        {
                            BuffEnemiesBehavior.s_activeEnemies.RemoveAt(i);
                        }
                    }
                    if (BuffEnemiesBehavior.s_activeEnemies.Count == 0)
                    {
                        __result = BehaviorResult.Continue;
                        return false;
                    }
                    while ((float)__instance.m_buffedEnemies.Count < __instance.EnemiesToBuff && BuffEnemiesBehavior.s_activeEnemies.Count > 0)
                    {
                        int index = UnityEngine.Random.Range(0, BuffEnemiesBehavior.s_activeEnemies.Count);
                        __instance.m_buffedEnemies.Add(BuffEnemiesBehavior.s_activeEnemies[index]);
                        BuffEnemiesBehavior.s_activeEnemies.RemoveAt(index);
                    }
                    for (int j = 0; j < __instance.m_buffedEnemies.Count; j++)
                    {
                        __instance.BuffEnemy(__instance.m_buffedEnemies[j]);
                    }
                    __instance.m_searchTimer = __instance.SearchInterval;
                    if (!string.IsNullOrEmpty(__instance.BuffAnimation))
                    {
                        __instance.m_aiAnimator.PlayUntilCancelled(__instance.BuffAnimation, true, null, -1f, false);
                    }
                    if (!string.IsNullOrEmpty(__instance.BuffVfx))
                    {
                        __instance.m_aiAnimator.PlayVfx(__instance.BuffVfx, null, null, null);
                    }
                    if (__instance.m_aiActor && __instance.m_aiActor.knockbackDoer)
                    {
                        __instance.m_aiActor.knockbackDoer.SetImmobile(true, "BuffEnemiesBehavior");
                    }
                    __result = BehaviorResult.RunContinuous;

                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(DestroyBulletsBehavior), nameof(DestroyBulletsBehavior.Upkeep))]
        public class ChangeUpDestroyBulletsBehavior
        {
            [HarmonyPrefix]
            public static bool Prefix(DestroyBulletsBehavior __instance)
            {
                if (AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(__instance.m_aiActor))
                {
                    __instance.DecrementTimer(ref __instance.m_cooldownTimer, true);
                    __instance.DecrementTimer(ref __instance.m_resetCooldownOnDamageCooldown, true);
                    if (__instance.HealthThresholds.Length > 0)
                    {
                        float currentHealthPercentage = __instance.m_aiActor.healthHaver.GetCurrentHealthPercentage();
                        if (currentHealthPercentage < __instance.m_lowestRecordedHealthPercentage)
                        {
                            for (int i = 0; i < __instance.HealthThresholds.Length; i++)
                            {
                                if (__instance.HealthThresholds[i] >= currentHealthPercentage && __instance.HealthThresholds[i] < __instance.m_lowestRecordedHealthPercentage)
                                {
                                    __instance.m_healthThresholdCredits++;
                                }
                            }
                            __instance.m_lowestRecordedHealthPercentage = currentHealthPercentage;
                        }
                    }
                    if (BasicAttackBehavior.DrawDebugFiringArea)
                    {
                        if (Time.frameCount != BasicAttackBehavior.m_lastFrame)
                        {
                            BasicAttackBehavior.m_arcCount = 0;
                            BasicAttackBehavior.m_lastFrame = Time.frameCount;
                        }
                        if (__instance.m_aiActor.TargetRigidbody && __instance.targetAreaStyle != null)
                        {
                            __instance.targetAreaStyle.DrawDebugLines(__instance.GetOrigin(__instance.targetAreaStyle.targetAreaOrigin), __instance.m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox), __instance.m_aiActor);
                        }
                    }

                    __instance.DecrementTimer(ref __instance.m_timer, false);
                    if (__instance.m_behaviorSpeculator.AttackCooldown <= 0f && __instance.m_behaviorSpeculator.GlobalCooldown <= 0f && __instance.m_cooldownTimer < __instance.SkippableCooldown)
                    {
                        bool flag = false;

                        GameObject gameObject = new GameObject("silencer_bombshee");
                        SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
                        silencerInstance.ForceNoDamage = true;
                        var cake = Toolbox.isCakedUp();
                        if (cake)
                        {
                            silencerInstance.TriggerSilencer(__instance.m_aiActor.specRigidbody.UnitCenter, 50f, __instance.Radius * 10, null, 0f, 0f, 0f, 0f, 0f, 0f, 0.25f, cake != null ? cake.LastOwner : null, false, true);
                        }

                        Vector2 unitCenter = __instance.m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter;
                        ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
                        for (int i = 0; i < allProjectiles.Count; i++)
                        {
                            Projectile projectile = allProjectiles[i];
                            if (projectile.Owner is AIActor enemy)
                            {
                                if (!AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(enemy))
                                {
                                    if (projectile.specRigidbody)
                                    {
                                        if (Vector2.Distance(unitCenter, projectile.specRigidbody.UnitCenter) <= __instance.SkippableRadius)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            __instance.m_cooldownTimer = 0f;
                        }
                    }

                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(DisplaceBehavior), nameof(DisplaceBehavior.SpawnImage))]
        public class ChangeUpDisplaceBehavior
        {
            [HarmonyPrefix]
            public static bool Prefix(DisplaceBehavior __instance)
            {
                if (AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(__instance.m_aiActor))
                {
                    var c = Toolbox.isCakedUp();
                    if (c)
                    {
                        if (__instance.m_behaviorSpeculator && __instance.m_behaviorSpeculator.MovementBehaviors.Count == 0)
                        {
                            return false;
                        }
                        AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(__instance.m_aiActor.EnemyGuid);
                        __instance.m_image = AIActor.Spawn(orLoadByGuid, __instance.m_aiActor.specRigidbody.UnitBottomLeft, __instance.m_aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Spawn, true);
                        __instance.m_image.transform.position = __instance.m_aiActor.transform.position;
                        __instance.m_image.specRigidbody.Reinitialize();
                        __instance.m_image.aiAnimator.healthHaver.SetHealthMaximum((__instance.ImageHealthMultiplier * __instance.m_aiActor.healthHaver.GetMaxHealth()) * 0.0625f, null, false);
                        
                        __instance.m_image.StartCoroutine(WaitUntilActive(__instance.m_image));



                        DisplacedImageController displacedImageController = __instance.m_image.gameObject.AddComponent<DisplacedImageController>();
                        displacedImageController.Init();
                        displacedImageController.SetHost(__instance.m_aiActor);
                        if (__instance.m_behaviorSpeculator && __instance.m_behaviorSpeculator.MovementBehaviors != null && __instance.m_behaviorSpeculator.MovementBehaviors.Count > 0)
                        {
                            FleeTargetBehavior fleeTargetBehavior = __instance.m_behaviorSpeculator.MovementBehaviors[0] as FleeTargetBehavior;
                            if (fleeTargetBehavior != null)
                            {
                                fleeTargetBehavior.ForceRun = true;
                            }
                        }
                        if (!__instance.m_hasInstantSpawned)
                        {
                            __instance.m_image.behaviorSpeculator.GlobalCooldown = __instance.InitialImageAttackDelay;
                        }
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(TargetEnemiesBehavior), nameof(TargetEnemiesBehavior.Update))]
        public class ChangeUpTargetEnemiesBehavior
        {
            [HarmonyPrefix]
            public static bool Prefix(TargetEnemiesBehavior __instance, ref BehaviorResult __result)
            {
                if (AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(__instance.m_aiActor))
                {
                    BehaviorResult behaviorResult = BehaviorResult.Continue;
                    if (behaviorResult != BehaviorResult.Continue)
                    {
                        __result = behaviorResult;
                        return false;
                    }
                    if (__instance.m_losTimer > 0f)
                    {
                        __result = BehaviorResult.Continue;
                        return false;
                    }
                    __instance.m_losTimer = __instance.SearchInterval;
                    if (__instance.m_aiActor.PlayerTarget)
                    {
                        if (__instance.m_aiActor.PlayerTarget.IsFalling)
                        {
                            __instance.m_aiActor.PlayerTarget = null;
                            __instance.m_aiActor.ClearPath();
                            __result = BehaviorResult.SkipRemainingClassBehaviors;
                            return false;
                        }
                        if (__instance.m_aiActor.PlayerTarget.healthHaver && __instance.m_aiActor.PlayerTarget.healthHaver.IsDead)
                        {
                            __instance.m_aiActor.PlayerTarget = null;
                            __instance.m_aiActor.ClearPath();
                            __result = BehaviorResult.SkipRemainingClassBehaviors;
                            return false;
                        }
                    }
                    else
                    {
                        __instance.m_aiActor.PlayerTarget = null;
                    }
                    if (!__instance.ObjectPermanence)
                    {
                        __instance.m_aiActor.PlayerTarget = null;
                    }
                    if (__instance.m_aiActor.PlayerTarget != null)
                    {
                        __result = BehaviorResult.Continue;
                        return false;
                    }
                    if (!__instance.m_aiActor.CanTargetEnemies)
                    {
                        __result = BehaviorResult.Continue;
                        return false;
                    }
                    List<AIActor> activeEnemies = AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors;
                    if (activeEnemies != null && activeEnemies.Count > 0)
                    {
                        AIActor playerTarget = null;
                        float num = float.MaxValue;
                        for (int i = 0; i < activeEnemies.Count; i++)
                        {
                            AIActor aiactor = activeEnemies[i];
                            if (!(aiactor == __instance.m_aiActor))
                            {
                                float num2 = Vector2.Distance(__instance.m_aiActor.CenterPosition, aiactor.CenterPosition);
                                if (num2 < num)
                                {
                                    if (__instance.LineOfSight)
                                    {
                                        int standardPlayerVisibilityMask = CollisionMask.StandardPlayerVisibilityMask;
                                        RaycastResult raycastResult;
                                        if (!PhysicsEngine.Instance.Raycast(__instance.m_aiActor.CenterPosition, aiactor.CenterPosition - __instance.m_aiActor.CenterPosition, num2, out raycastResult, true, true, standardPlayerVisibilityMask, null, false, null, __instance.m_aiActor.specRigidbody))
                                        {
                                            RaycastResult.Pool.Free(ref raycastResult);
                                            goto IL_258;
                                        }
                                        if (raycastResult.SpeculativeRigidbody == null || raycastResult.SpeculativeRigidbody.GetComponent<PlayerController>() == null)
                                        {
                                            RaycastResult.Pool.Free(ref raycastResult);
                                            goto IL_258;
                                        }
                                        RaycastResult.Pool.Free(ref raycastResult);
                                    }
                                    playerTarget = aiactor;
                                    num = num2;
                                }
                            }
                        IL_258:;
                        }
                        __instance.m_aiActor.PlayerTarget = playerTarget;
                    }
                    if (__instance.m_aiShooter != null && __instance.m_aiActor.PlayerTarget != null)
                    {
                        __instance.m_aiShooter.AimAtPoint(__instance.m_aiActor.PlayerTarget.CenterPosition);
                    }
                    if (!__instance.m_aiActor.HasBeenEngaged)
                    {
                        __instance.m_aiActor.HasBeenEngaged = true;
                        __result = BehaviorResult.SkipAllRemainingBehaviors;
                        return false;
                    }
                    __result = BehaviorResult.SkipRemainingClassBehaviors;


                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(BulletLimbController), nameof(BulletLimbController.PostBodyMovement))]
        public class ChangeUpBulletLimbController
        {
            [HarmonyPostfix]
            public static void Postfix(BulletLimbController __instance, SpeculativeRigidbody specRigidbody, Vector2 unitDelta, IntVector2 pixelDelta)
            {
                if (AIActorModifiers.RalseiCharmedEnemyController.allCharmedAIActors.Contains(__instance.m_body))
                {
                    int i = __instance.m_projectiles.Count; 

                    for (int a = 0; a < i; a++)
                    {
                        Projectile projectile = __instance.m_projectiles[a];
                        if (projectile.collidesWithPlayer == true)
                        {
                            projectile.DieInAir(!projectile.gameObject.activeSelf, true, true, false);
                        }
                    }
                }
            }
        }
    }
}
