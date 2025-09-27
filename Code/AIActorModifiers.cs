using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using Dungeonator;
using Pathfinding;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Ralsei.Code;
using Alexandria.ItemAPI;

namespace Ralsei
{
    public static class AIActorModifiers
    {

        public static void RalseifyEnemy(this AIActor CurrentEnemy, Cake cake)
        {
            var user = cake.LastOwner;
            CurrentEnemy.RemoveEffect(Cake.charmingRoundsEffect);

            CurrentEnemy.sprite.color = Color.green;
            CurrentEnemy.behaviorSpeculator.enabled = false;
            CurrentEnemy.IgnoreForRoomClear = true;
            CurrentEnemy.CanTargetEnemies = true;
            CurrentEnemy.CanTargetPlayers = false;

            CurrentEnemy.OverrideTarget = null;


            if (ScarfWeapon.RalseiSlash.targetVFX != null && ScarfWeapon.RalseiSlash.targetVFX.TargetInst == CurrentEnemy)
            {
                UnityEngine.Object.Destroy(ScarfWeapon.RalseiSlash.targetVFX.gameObject);
                ScarfWeapon.RalseiSlash.targetVFX = null;
            }



            CompanionController yup = CurrentEnemy.gameObject.AddComponent<CompanionController>();
            yup.companionID = CompanionController.CompanionIdentifier.NONE;
            yup.CanCrossPits = true;
            yup.CanInterceptBullets = true;
            yup.Initialize(user);

            CurrentEnemy.CompanionOwner = user;

            user.CurrentRoom.DeregisterEnemy(CurrentEnemy);

            AIAnimator aiAnimator = CurrentEnemy.aiAnimator;

            CompanionFollowPlayerBehavior comp = new CompanionFollowPlayerBehavior();
            comp.CanRollOverPits = false;
            comp.CatchUpOutAnimation = aiAnimator.MoveAnimation.Prefix;
            comp.DisableInCombat = false;
            //comp.IdleAnimations = aiAnimator.IdleAnimation.AnimNames;
            comp.CatchUpRadius = 8;
            comp.CatchUpAccelTime = 5;
            comp.CatchUpSpeed = CurrentEnemy.MovementSpeed *= 1.15f;
            comp.CatchUpMaxSpeed = Mathf.Max(CurrentEnemy.MovementSpeed * 1.3f, 6);
            comp.CatchUpAnimation = aiAnimator.MoveAnimation.Prefix;
            comp.RollAnimation = aiAnimator.MoveAnimation.Prefix;
            comp.TemporarilyDisabled = false;
           
            ///modidies certain movement behaviors and removes specific ones that interact poorly while friendly
            for (int i = CurrentEnemy.behaviorSpeculator.MovementBehaviors.Count - 1; i > -1; i--)
            {
                var att = CurrentEnemy.behaviorSpeculator.MovementBehaviors[i];
                if (att is SeekTargetBehavior tagr)
                {
                    tagr.ReturnToSpawn = false;
                    comp.PathInterval = tagr.PathInterval;
                    comp.IdealRadius = tagr.CustomRange;
                    comp.DisableInCombat = true;
                }
                if (att is FleeTargetBehavior flee)
                {
                    CurrentEnemy.behaviorSpeculator.MovementBehaviors.Remove(flee);
                }
                if (att is MoveErraticallyBehavior errat)
                {
                    CurrentEnemy.behaviorSpeculator.MovementBehaviors.Remove(errat);
                }
            }

            ///executes all mirror copies of enemies
            /// aiActor.ForceDeath does not actually kill the enemy, just runs all the code related to dying. Stupid.
            /// Just apply a billion damage and move on

            for (int i = 0; i < CurrentEnemy.behaviorSpeculator.AttackBehaviors.Count; i++)
            {
                var att = CurrentEnemy.behaviorSpeculator.AttackBehaviors[i];
                if (att is MirrorImageBehavior mirror)
                {
                    mirror.m_allImages.ForEach(self =>
                    {
                        self.aiActor.healthHaver.ApplyDamage(100000000, Vector2.zero, "Fuck off");
                    });
                }
                if (att is DisplaceBehavior displace)
                {
                    if (displace.m_image)
                    {
                        displace.m_image.healthHaver.ApplyDamage(100000000, Vector2.zero, "Fuck off"); ;
                    }
                }
                if (att is BuffEnemiesBehavior beb)
                {
                    beb.EndContinuousUpdate();
                }

                if (att is AttackBehaviorGroup atkbhgrp)
                {
                    foreach (var entry in atkbhgrp.AttackBehaviors)
                    {
                        if (entry.Behavior is MirrorImageBehavior mirror_2)
                        {
                            mirror_2.m_allImages.ForEach(self =>
                            {
                                self.aiActor.healthHaver.ApplyDamage(100000000, Vector2.zero, "Fuck off");
                            });
                        }
                        if (entry.Behavior is BuffEnemiesBehavior beb_2)
                        {
                            beb_2.EndContinuousUpdate();
                        }
                        if (entry.Behavior is DisplaceBehavior displace_2)
                        {
                            if (displace_2.m_image)
                            {
                                displace_2.m_image.healthHaver.ApplyDamage(100000000, Vector2.zero, "Fuck off");
                            }
                        }
                    }
                }
            }
            if (CurrentEnemy.behaviorSpeculator.AttackBehaviorGroup != null)
            {
                foreach (var entry in CurrentEnemy.behaviorSpeculator.AttackBehaviorGroup.AttackBehaviors)
                {
                    if (entry.Behavior is MirrorImageBehavior mirror_2)
                    {
                        mirror_2.m_allImages.ForEach(self =>
                        {
                            self.aiActor.healthHaver.ApplyDamage(100000000, Vector2.zero, "Fuck off");
                        });
                    }
                    if (entry.Behavior is BuffEnemiesBehavior beb_2)
                    {
                        beb_2.EndContinuousUpdate();
                    }
                    if (entry.Behavior is DisplaceBehavior displace_2)
                    {
                        if (displace_2.m_image)
                        {
                            displace_2.m_image.healthHaver.ApplyDamage(100000000, Vector2.zero, "Fuck off");
                        }
                    }
                }
            }








                CurrentEnemy.CompanionSettings = new ActorCompanionSettings() { WarpsToRandomPoint = false };
            CurrentEnemy.behaviorSpeculator.MovementBehaviors.Add(comp);
            CurrentEnemy.behaviorSpeculator.enabled = true;
            CurrentEnemy.behaviorSpeculator.RefreshBehaviors();

            //DontDestroyOnLoad(CurrentEnemy.gameObject);
            //CurrentEnemy.transform.SetParent(null, true);
            //CurrentEnemy.transform.parent = null;

            CurrentEnemy.gameObject.transform.parent = null;
            GameObject.DontDestroyOnLoad(CurrentEnemy.gameObject);

            comp.Start();

            for (int i = CurrentEnemy.m_activeEffects.Count - 1; i > -1; i--)
            {
                if (CurrentEnemy.m_activeEffects[i] is RalseiCharmable charmer)
                {
                    CurrentEnemy.RemoveEffect(charmer.effectIdentifier);
                }
            }

            var component = CurrentEnemy.gameObject.GetComponent<SpawnEnemyOnDeath>();
            if (component)
            {
                component.ConvertSpawnModifier();
            }
            var component_2 = CurrentEnemy.gameObject.GetComponent<CrazedController>();
            if (component_2)
            {
                UnityEngine.Object.Destroy(component_2);
            }



            AIActorModifiers.CompanionisedEnemyBulletModifiers yeehaw = yup.gameObject.AddComponent<AIActorModifiers.CompanionisedEnemyBulletModifiers>();
            yeehaw.jammedDamageMultiplier *= 2.5f;
            yeehaw.baseBulletDamage = 3f;
            yeehaw.TintBullets = true;
            yeehaw.TintColor = new Color(0.333f, 1, 0.8f);
            yeehaw.BulletsAreUnBlankable = true;

            var scarfInstance = UnityEngine.Object.Instantiate<GameObject>(ScarfWeapon.scarfRef.gameObject).AddComponent<CustomScarfDoer>();
            scarfInstance.AttachTarget = CurrentEnemy;
            scarfInstance.ScarfMaterial = new Material(ScarfWeapon.scarfRef.ScarfMaterial);
            scarfInstance.StartWidth = 0.1f;
            scarfInstance.EndWidth = 0.125f;
            scarfInstance.AnimationSpeed = 12f;
            scarfInstance.ScarfLength = 0.5f;
            scarfInstance.AngleLerpSpeed = 4;
            scarfInstance.BackwardZOffset = -0.75f;
            scarfInstance.CatchUpScale = 0.9f;
            scarfInstance.SinSpeed = 9f;
            scarfInstance.AmplitudeMod = 0.135f;
            scarfInstance.WavelengthMod = 1.1f;
            scarfInstance.ScarfMaterial.SetColor("_OverrideColor", new Color(0, 0.8f, 0.125f));
            scarfInstance.Initialize(CurrentEnemy);
            scarfInstance.AdditionalOffset = new Vector3(0, 0.0625f);
            float offset = ((float)CurrentEnemy.specRigidbody.HitboxPixelCollider.Height) / 32;
            var extantOverheadder = CurrentEnemy.SmartPlayEffectOnActor(Cake.EffectPrefab.gameObject, new Vector3(-0.625f, -offset - 0.25f), true, true, true, true).GetComponent<EnemyHealthBarRalsei>();
            extantOverheadder.InitEffect(CurrentEnemy);

            CurrentEnemy.healthHaver.maximumHealth *= 5f;
            CurrentEnemy.healthHaver.CursedMaximum = CurrentEnemy.healthHaver.maximumHealth;
            CurrentEnemy.healthHaver.FullHeal();


            user.companions.Add(CurrentEnemy);


            CurrentEnemy.StartCoroutine(DoWait(CurrentEnemy));


            var e = CurrentEnemy.specRigidbody.PixelColliders.Where(self => self.CollisionLayer == CollisionLayer.EnemyHitBox);
            if (e != null)
            {
                e.FirstOrDefault().CollisionLayer = CollisionLayer.PlayerHitBox;
            }
            e = CurrentEnemy.specRigidbody.PixelColliders.Where(self => self.CollisionLayer == CollisionLayer.EnemyCollider);
            if (e != null)
            {
                e.FirstOrDefault().CollisionLayer = CollisionLayer.PlayerCollider;
            }
            CurrentEnemy.specRigidbody.Reinitialize();

            var c = CurrentEnemy.gameObject.AddComponent<AIActorModifiers.RalseiCharmedEnemyController>();
            c.InitEnemy(cake, comp);
            cake.AllCharmedEnemies.Add(c);
        }

        public static IEnumerator DoWait(AIActor aIActor)
        {
            yield return null;

            BulletLimbController limbs = aIActor.gameObject.GetComponent<BulletLimbController>();
            if (limbs != null)
            {
                limbs.DestroyProjectiles();
            }
            yield break;
        }

        public static FriendlySpawnEnemyOnDeath ConvertSpawnModifier(this SpawnEnemyOnDeath spawnEnemyOnDeath)
        {
            var _new = spawnEnemyOnDeath.gameObject.AddComponent<FriendlySpawnEnemyOnDeath>();
            _new.enemyGuidsToSpawn = spawnEnemyOnDeath.enemyGuidsToSpawn;
            _new.enemySelection = spawnEnemyOnDeath.enemySelection;
            _new.extraPixelWidth = spawnEnemyOnDeath.extraPixelWidth;
            _new.spawnAnim = spawnEnemyOnDeath.spawnAnim;
            _new.chanceToSpawn = spawnEnemyOnDeath.chanceToSpawn;
            _new.deathType = spawnEnemyOnDeath.deathType;
            _new.DoNormalReinforcement = spawnEnemyOnDeath.DoNormalReinforcement;
            _new.guaranteedSpawnGenerations = spawnEnemyOnDeath.guaranteedSpawnGenerations;
            _new.minSpawnCount = spawnEnemyOnDeath.minSpawnCount;
            _new.maxSpawnCount = spawnEnemyOnDeath.maxSpawnCount;
            _new.triggerName = spawnEnemyOnDeath.triggerName;
            _new.spawnVfx = spawnEnemyOnDeath.spawnVfx;
            _new.spawnsCanDropLoot = spawnEnemyOnDeath.spawnsCanDropLoot;
            _new.spawnRadius = spawnEnemyOnDeath.spawnRadius;
            _new.spawnPosition = spawnEnemyOnDeath.spawnPosition;
            _new.preDeathDelay = spawnEnemyOnDeath.preDeathDelay;
            _new.m_deathDir = spawnEnemyOnDeath.m_deathDir;
            _new.m_cachedCache = spawnEnemyOnDeath.m_cachedCache;
            UnityEngine.Object.Destroy(spawnEnemyOnDeath);
            return _new;
        }


        public class FriendlySpawnEnemyOnDeath : OnDeathBehavior
        {
            private bool ShowRandomPrams()
            {
                return this.enemySelection == SpawnEnemyOnDeath.EnemySelection.Random;
            }

            private bool ShowInsideColliderParams()
            {
                return this.spawnPosition == SpawnEnemyOnDeath.SpawnPosition.InsideCollider;
            }

            private bool ShowInsideRadiusParams()
            {
                return this.spawnPosition == SpawnEnemyOnDeath.SpawnPosition.InsideRadius;
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
            }

            public override void OnTrigger(Vector2 damageDirection)
            {
                if (this.m_hasTriggered)
                {
                    return;
                }
                this.m_hasTriggered = true;
                if (this.guaranteedSpawnGenerations <= 0f && this.chanceToSpawn < 1f && UnityEngine.Random.value > this.chanceToSpawn)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(this.spawnVfx))
                {
                    base.aiAnimator.PlayVfx(this.spawnVfx, null, null, null);
                }
                string[] array = null;
                if (this.enemySelection == SpawnEnemyOnDeath.EnemySelection.All)
                {
                    array = this.enemyGuidsToSpawn;
                }
                else if (this.enemySelection == SpawnEnemyOnDeath.EnemySelection.Random)
                {
                    array = new string[UnityEngine.Random.Range(this.minSpawnCount, this.maxSpawnCount)];
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = BraveUtility.RandomElement<string>(this.enemyGuidsToSpawn);
                    }
                }
                this.SpawnEnemies(array);
            }

            public void ManuallyTrigger(Vector2 damageDirection)
            {
                this.OnTrigger(damageDirection);
            }

            private void SpawnEnemies(string[] selectedEnemyGuids)
            {
                if (this.spawnPosition == SpawnEnemyOnDeath.SpawnPosition.InsideCollider)
                {
                    IntVector2 pos = base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
                    if (base.aiActor.IsFalling)
                    {
                        return;
                    }
                    if (GameManager.Instance.Dungeon.CellIsPit(base.specRigidbody.UnitCenter.ToVector3ZUp(0f)))
                    {
                        return;
                    }
                    RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(pos);
                    List<SpeculativeRigidbody> list = new List<SpeculativeRigidbody>();
                    list.Add(base.specRigidbody);
                    Vector2 unitBottomLeft = base.specRigidbody.UnitBottomLeft;
                    for (int i = 0; i < selectedEnemyGuids.Length; i++)
                    {
                        AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(selectedEnemyGuids[i]);
                        AIActor aiactor = AIActor.Spawn(orLoadByGuid, base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), roomFromPosition, false, AIActor.AwakenAnimationType.Default, true);
                        if (base.aiActor.IsBlackPhantom)
                        {
                            aiactor.ForceBlackPhantom = true;
                        }
                        if (aiactor)
                        {
                            aiactor.specRigidbody.Initialize();
                            Vector2 a = unitBottomLeft - (aiactor.specRigidbody.UnitBottomLeft - aiactor.transform.position.XY());
                            Vector2 vector = a + new Vector2(Mathf.Max(0f, base.specRigidbody.UnitDimensions.x - aiactor.specRigidbody.UnitDimensions.x), 0f);
                            aiactor.transform.position = Vector2.Lerp(a, vector, (selectedEnemyGuids.Length != 1) ? ((float)i / ((float)selectedEnemyGuids.Length - 1f)) : 0f);
                            aiactor.specRigidbody.Reinitialize();
                            a -= new Vector2(PhysicsEngine.PixelToUnit(this.extraPixelWidth), 0f);
                            vector += new Vector2(PhysicsEngine.PixelToUnit(this.extraPixelWidth), 0f);
                            Vector2 a2 = Vector2.Lerp(a, vector, (selectedEnemyGuids.Length != 1) ? ((float)i / ((float)selectedEnemyGuids.Length - 1f)) : 0.5f);
                            IntVector2 intVector = PhysicsEngine.UnitToPixel(a2 - aiactor.transform.position.XY());
                            CollisionData collisionData = null;
                            if (PhysicsEngine.Instance.RigidbodyCastWithIgnores(aiactor.specRigidbody, intVector, out collisionData, true, true, null, false, list.ToArray()))
                            {
                                intVector = collisionData.NewPixelsToMove;
                            }
                            CollisionData.Pool.Free(ref collisionData);
                            aiactor.transform.position += PhysicsEngine.PixelToUnit(intVector).ToVector3ZisY();
                            aiactor.specRigidbody.Reinitialize();
                            if (i == 0)
                            {
                                aiactor.aiAnimator.FacingDirection = 180f;
                            }
                            else if (i == selectedEnemyGuids.Length - 1)
                            {
                                aiactor.aiAnimator.FacingDirection = 0f;
                            }
                            this.HandleSpawn(aiactor);
                            list.Add(aiactor.specRigidbody);
                        }
                    }
                    for (int j = 0; j < list.Count; j++)
                    {
                        for (int k = 0; k < list.Count; k++)
                        {
                            if (j != k)
                            {
                                list[j].RegisterGhostCollisionException(list[k]);
                            }
                        }
                    }
                }
                else if (this.spawnPosition == SpawnEnemyOnDeath.SpawnPosition.ScreenEdge)
                {
                    for (int l = 0; l < selectedEnemyGuids.Length; l++)
                    {
                        AIActor orLoadByGuid2 = EnemyDatabase.GetOrLoadByGuid(selectedEnemyGuids[l]);
                        AIActor spawnedActor = AIActor.Spawn(orLoadByGuid2, base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), base.aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Default, true);
                        if (spawnedActor)
                        {
                            Vector2 cameraBottomLeft = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
                            Vector2 cameraTopRight = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
                            IntVector2 bottomLeft = cameraBottomLeft.ToIntVector2(VectorConversions.Ceil);
                            IntVector2 topRight = cameraTopRight.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
                            CellValidator cellValidator = delegate (IntVector2 c)
                            {
                                for (int num2 = 0; num2 < spawnedActor.Clearance.x; num2++)
                                {
                                    for (int num3 = 0; num3 < spawnedActor.Clearance.y; num3++)
                                    {
                                        if (GameManager.Instance.Dungeon.data.isTopWall(c.x + num2, c.y + num3))
                                        {
                                            return false;
                                        }
                                        if (GameManager.Instance.Dungeon.data[c.x + num2, c.y + num3].isExitCell)
                                        {
                                            return false;
                                        }
                                    }
                                }
                                return c.x >= bottomLeft.x && c.y >= bottomLeft.y && c.x + spawnedActor.Clearance.x - 1 <= topRight.x && c.y + spawnedActor.Clearance.y - 1 <= topRight.y;
                            };
                            Func<IntVector2, float> cellWeightFinder = delegate (IntVector2 c)
                            {
                                float a3 = float.MaxValue;
                                a3 = Mathf.Min(a3, (float)c.x - cameraBottomLeft.x);
                                a3 = Mathf.Min(a3, (float)c.y - cameraBottomLeft.y);
                                a3 = Mathf.Min(a3, cameraTopRight.x - (float)c.x + (float)spawnedActor.Clearance.x);
                                return Mathf.Min(a3, cameraTopRight.y - (float)c.y + (float)spawnedActor.Clearance.y);
                            };
                            Vector2 b = spawnedActor.specRigidbody.UnitCenter - spawnedActor.transform.position.XY();
                            IntVector2? randomWeightedAvailableCell = spawnedActor.ParentRoom.GetRandomWeightedAvailableCell(new IntVector2?(spawnedActor.Clearance), new CellTypes?(spawnedActor.PathableTiles), false, cellValidator, cellWeightFinder, 0.25f);
                            if (randomWeightedAvailableCell == null)
                            {
                                Debug.LogError("Screen Edge Spawn FAILED!", spawnedActor);
                                UnityEngine.Object.Destroy(spawnedActor);
                            }
                            else
                            {
                                spawnedActor.transform.position = Pathfinder.GetClearanceOffset(randomWeightedAvailableCell.Value, spawnedActor.Clearance) - b;
                                spawnedActor.specRigidbody.Reinitialize();
                                this.HandleSpawn(spawnedActor);
                            }
                        }
                    }
                }
                else if (this.spawnPosition == SpawnEnemyOnDeath.SpawnPosition.InsideRadius)
                {
                    Vector2 unitCenter = base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
                    List<SpeculativeRigidbody> list2 = new List<SpeculativeRigidbody>();
                    list2.Add(base.specRigidbody);
                    for (int m = 0; m < selectedEnemyGuids.Length; m++)
                    {
                        Vector2 vector2 = unitCenter + UnityEngine.Random.insideUnitCircle * this.spawnRadius;
                        if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST && SceneManager.GetActiveScene().name == "fs_robot")
                        {
                            RoomHandler entrance = GameManager.Instance.Dungeon.data.Entrance;
                            Vector2 lhs = entrance.area.basePosition.ToVector2() + new Vector2(7f, 7f);
                            Vector2 lhs2 = entrance.area.basePosition.ToVector2() + new Vector2(38f, 36f);
                            vector2 = Vector2.Min(lhs2, Vector2.Max(lhs, vector2));
                        }
                        AIActor orLoadByGuid3 = EnemyDatabase.GetOrLoadByGuid(selectedEnemyGuids[m]);
                        AIActor aiactor2 = AIActor.Spawn(orLoadByGuid3, unitCenter.ToIntVector2(VectorConversions.Floor), base.aiActor.ParentRoom, true, AIActor.AwakenAnimationType.Default, true);
                        if (aiactor2)
                        {
                            aiactor2.specRigidbody.Initialize();
                            Vector2 unit = vector2 - aiactor2.specRigidbody.GetUnitCenter(ColliderType.HitBox);
                            aiactor2.specRigidbody.ImpartedPixelsToMove = PhysicsEngine.UnitToPixel(unit);
                            this.HandleSpawn(aiactor2);
                            list2.Add(aiactor2.specRigidbody);
                        }
                    }
                    for (int n = 0; n < list2.Count; n++)
                    {
                        for (int num = 0; num < list2.Count; num++)
                        {
                            if (n != num)
                            {
                                list2[n].RegisterGhostCollisionException(list2[num]);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Unknown spawn type: " + this.spawnPosition);
                }
            }

            private void HandleSpawn(AIActor spawnedActor)
            {

                if (!string.IsNullOrEmpty(this.spawnAnim))
                {
                    spawnedActor.aiAnimator.PlayUntilFinished(this.spawnAnim, false, null, -1f, false);
                }
                spawnedActor.StartCoroutine(WaitTillSpawnedFully(spawnedActor));
                SpawnEnemyOnDeath component = spawnedActor.GetComponent<SpawnEnemyOnDeath>();
                if (component)
                {
                    component.guaranteedSpawnGenerations = Mathf.Max(0f, this.guaranteedSpawnGenerations - 1f);
                }
                if (!this.spawnsCanDropLoot)
                {
                    spawnedActor.CanDropCurrency = false;
                    spawnedActor.CanDropItems = false;
                }
                if (this.DoNormalReinforcement)
                {
                    spawnedActor.HandleReinforcementFallIntoRoom(0.1f);
                }
            }

            public IEnumerator WaitTillSpawnedFully(AIActor aIActor)
            {
                if (!string.IsNullOrEmpty(this.spawnAnim))
                {
                    while (aIActor.aiAnimator.IsPlaying(this.spawnAnim)) {yield return null; }
                }
                yield return null;
                bool ralseified = false;
                foreach (var entry in GameManager.Instance.AllPlayers)
                {
                    if (entry.activeItems != null)
                    {
                        foreach (var _ in entry.activeItems)
                        {
                            if (_ is Cake cake)
                            {
                                ralseified = true;
                                RalseifyEnemy(aIActor, cake);
                                break;
                            }
                        }
                    }
                    if (ralseified == true) { break; }
                }
                yield break;
            }


            public float chanceToSpawn = 1f;

            public string spawnVfx;

            [Header("Enemies to Spawn")]
            public SpawnEnemyOnDeath.EnemySelection enemySelection = SpawnEnemyOnDeath.EnemySelection.All;

            [EnemyIdentifier]
            public string[] enemyGuidsToSpawn;

            [ShowInInspectorIf("ShowRandomPrams", true)]
            public int minSpawnCount = 1;

            [ShowInInspectorIf("ShowRandomPrams", true)]
            public int maxSpawnCount = 1;

            [FormerlySerializedAs("spawnType")]
            [Header("Placement")]
            public SpawnEnemyOnDeath.SpawnPosition spawnPosition;

            [ShowInInspectorIf("ShowInsideColliderParams", true)]
            public int extraPixelWidth;

            [ShowInInspectorIf("ShowInsideRadiusParams", true)]
            public float spawnRadius = 1f;

            [Header("Spawn Parameters")]
            public float guaranteedSpawnGenerations;

            public string spawnAnim = "awaken";

            public bool spawnsCanDropLoot = true;

            public bool DoNormalReinforcement;

            private bool m_hasTriggered;

        }

        public class RalseiCharmedEnemyController : BraveBehaviour
        {
            public static List<AIActor> allCharmedAIActors = new List<AIActor>();
            public static List<RalseiCharmedEnemyController> allCharmed = new List<RalseiCharmedEnemyController> ();
            public Cake CakeRef;
            private CompanionFollowPlayerBehavior companionController;
            public Dictionary<string, GameObject> AttachedEffectsAdditional = new Dictionary<string, GameObject>();

            public void InitEnemy(Cake cake, CompanionFollowPlayerBehavior _companionController)
            {
                allCharmed.Add(this);
                allCharmedAIActors.Add(this.aiActor);
                companionController = _companionController;
                CakeRef = cake;
                if (cake)
                {
                    cake.LastOwner.OnRoomClearEvent += LastOwner_OnRoomClearEvent;
                    cake.LastOwner.OnNewFloorLoaded += PlayerNewFloor;
                    cake.LastOwner.OnEnteredCombat += EnteredCombat;
                }


                this.aiActor.healthHaver.OnPreDeath += HealthHaver_OnPreDeath;
                this.aiActor.healthHaver.OnDamaged += HealthHaver_OnDamaged;
                if (this.aiActor.CollisionDamage > 0)
                {
                    this.aiActor.specRigidbody.OnPreRigidbodyCollision += (myBody, myPixelCollider, otherBody, otherPixelCollider) =>
                    {
                        if (!isCurrentlyStealthed)
                        {
                            var _myBody = (myBody as SpeculativeRigidbody);
                            var _myPixelCollider = (myBody as SpeculativeRigidbody);
                            var _otherBody = (myBody as SpeculativeRigidbody);
                            var _otherPixelCollider = (myBody as SpeculativeRigidbody);

                            if (_otherBody.aiActor != null && _otherBody.aiActor.GetComponent<RalseiCharmedEnemyController>() == null)
                            {
                                if (CollisionDamageCooldown <= 0)
                                {
                                    CollisionDamageCooldown = ReturnContactCooldown();
                                    float contactDamage = ((this.aiActor.healthHaver.maximumHealth * 0.15f) * this.aiActor.CollisionDamage) + 0.5f;
                                    if (_otherBody.aiActor.CollisionSetsPlayerOnFire)
                                    {
                                        contactDamage *= 0.5f;
                                        _otherBody.aiActor.ApplyEffect(Cake.hotLeadEffect);
                                    }
                                    contactDamage *= CollisionDamageMult;
                                    _otherBody.aiActor.healthHaver.ApplyDamage(contactDamage, Vector2.zero, "Collision");
                                }
                            }
                        }
                    };
                }
            }
            private float CollisionDamageCooldown = 0;
            private bool isCurrentlyStealthed = false;
            private float currentC;
            private void LastOwner_OnDidUnstealthyAction(PlayerController obj)
            {
                this.aiActor.SetIsStealthed(false, ";3");
                this.aiActor.healthHaver.AllDamageMultiplier /= 0.2f;
                this.aiActor.behaviorSpeculator.CooldownScale = currentC;
                CakeRef.LastOwner.OnDidUnstealthyAction -= LastOwner_OnDidUnstealthyAction;
            }

            private void StartedStealth(PlayerController obj)
            {
                currentC = this.aiActor.behaviorSpeculator.CooldownScale;
                this.aiActor.behaviorSpeculator.CooldownScale = 0;
                this.aiActor.SetIsStealthed(true, ";3");
                this.aiActor.healthHaver.AllDamageMultiplier *= 0.2f;
                CakeRef.LastOwner.OnDidUnstealthyAction += LastOwner_OnDidUnstealthyAction;

            }

            private void HealthHaver_OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
            {
                this.StartCoroutine(DoInvulnerabilityTime(Mathf.Max(Mathf.Min(0.5f, resultValue / 100), 0.125f)));
            }

            public IEnumerator DoInvulnerabilityTime(float Duration)
            {
                this.aiActor.healthHaver.IsVulnerable = false;
                yield return new WaitForSeconds(Duration);
                this.aiActor.healthHaver.IsVulnerable = true;
                yield break;
            }
            public void EnteredCombat()
            {
                inCombat = true;
            }

            public void PlayerNewFloor(PlayerController playerController)
            {
                aiActor.healthHaver.SetHealthMaximum(aiActor.healthHaver.maximumHealth * 1.15f);
                aiActor.healthHaver.ApplyHealing(Mathf.Min(125, aiActor.healthHaver.maximumHealth * 0.3f));
            }

            private void HealthHaver_OnPreDeath(Vector2 obj)
            {
                CakeRef.LastOwner.OnNewFloorLoaded -= PlayerNewFloor;
                CakeRef.LastOwner.OnRoomClearEvent -= LastOwner_OnRoomClearEvent;
                CakeRef.LastOwner.OnDidUnstealthyAction -= LastOwner_OnDidUnstealthyAction;
                CakeRef.LastOwner.OnEnteredCombat -= EnteredCombat;


                CakeRef.LastOwner.companions.Remove(this.aiActor);
                CakeRef.AllCharmedEnemies.Remove(this);
            }

            private void LastOwner_OnRoomClearEvent(PlayerController obj)
            {
                inCombat = false;
                aiActor.healthHaver.SetHealthMaximum(aiActor.healthHaver.maximumHealth * 0.98f);
                aiActor.healthHaver.ApplyHealing(Mathf.Min(50, aiActor.healthHaver.maximumHealth * 0.125f));
                DoHealParticles();
            }

            public void DoHealParticles()
            {
                for (int i = 0; i < 12; i++)
                {
                    ParticleBase.EmitParticles("HealParticle", 1, new ParticleSystem.EmitParams()
                    {
                        position = aiActor.sprite.WorldCenter + BraveUtility.RandomVector2(new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f)),
                        velocity = BraveUtility.RandomVector2(new Vector2(0, 1), new Vector2(0, 3)),
                        startLifetime = 0.5f,
                        startSize = 0.5f
                    });
                }
            }

            public float ReturnContactCooldown()
            {
                float Start = 0.66f;
                if (WhatArmyActive)
                    Start *= 0.75f;
                if (LuteActive)
                    Start *= 0.75f;
                return Start;
            }

            private bool isFloorLoad = false;

            public void Update()
            {
                if (ScarfWeapon.RalseiSlash.targetVFX != null && ScarfWeapon.RalseiSlash.targetVFX.TargetInst == this.aiActor)
                {
                    UnityEngine.Object.Destroy(ScarfWeapon.RalseiSlash.targetVFX);
                    ScarfWeapon.RalseiSlash.targetVFX = null;
                }

                var isfloorLoading = GameManager.Instance.IsLoadingLevel;
                if (isFloorLoad != isfloorLoading)
                {
                    isFloorLoad = isfloorLoading;
                    if (isFloorLoad == true)
                    {
                        behaviorSpeculator.enabled = false;
                        aiActor.enabled = false;
                        aiAnimator.enabled = false;
                        behaviorSpeculator.Interrupt();
                        return;
                    }
                    else
                    {
                        behaviorSpeculator.enabled = true;
                        aiActor.enabled = true;
                        aiAnimator.enabled = true;
                    }
                }
                if (CollisionDamageCooldown > 0) { CollisionDamageCooldown -= BraveTime.DeltaTime; }


                if (CakeRef.LastOwner)
                {
                    var stealth = CakeRef.LastOwner.IsStealthed;
                    if (stealth != isCurrentlyStealthed)
                    {

                        isCurrentlyStealthed = stealth;

                        if (isCurrentlyStealthed == true)
                        {
                            StartedStealth(CakeRef.LastOwner);
                        }
                        else
                        {
                            LastOwner_OnDidUnstealthyAction(CakeRef.LastOwner);
                        }
                    }
                    if (CakeRef.LastOwner.CurrentRoom != null)
                    {
                        this.aiActor.parentRoom = CakeRef.LastOwner.CurrentRoom;
                    }
                }
                





                if (inCombat)
                {
                    if (ScarfWeapon.RalseiSlash.targetVFX != null)
                    {
                        this.aiActor.OverrideTarget = ScarfWeapon.RalseiSlash.targetVFX.TargetInst.specRigidbody;
                    }
                    Tick += BraveTime.DeltaTime;
                    if (Tick > 0.5f)
                    {
                        Tick = 0;
                        companionController.TemporarilyDisabled = (CakeRef.LastOwner.CurrentRoom == aiActor.transform.position.GetAbsoluteRoom());
                    }
                }
                else
                {
                    companionController.TemporarilyDisabled = false;
                }

                if (CakeRef.LastOwner != null)
                {
                    var whatarmy = CakeRef.LastOwner.PlayerHasActiveSynergy("You And What Army");

                    if (CakeRef.LastOwner.CurrentGun)
                    {
                        bool active = CakeRef.LastOwner.CurrentGun.LuteCompanionBuffActive;
                        if (LuteActive != active)
                        {
                            LuteActive = active;
                            if (LuteActive == false)
                            {
                                base.aiActor.MovementSpeed /= 2;
                                CollisionDamageMult /= 2.5f;
                            }
                            else
                            {
                                base.aiActor.MovementSpeed *= 2;
                                CollisionDamageMult *= 2.5f;
                            }
                        }
                    }
                    if (WhatArmyActive != whatarmy)
                    {
                        WhatArmyActive = whatarmy;
                        if (WhatArmyActive == false)
                        {
                            base.aiActor.MovementSpeed /= 1.5f;
                            base.aiActor.behaviorSpeculator.CooldownScale /= 1.25f;
                            CollisionDamageMult /= 2.5f;
                        }
                        else
                        {
                            base.aiActor.MovementSpeed *= 1.5f;
                            base.aiActor.behaviorSpeculator.CooldownScale *= 1.25f;
                            CollisionDamageMult *= 2.5f;
                        }
                    }
                }
            }

            private bool WhatArmyActive = false;
            private bool LuteActive = false;

            private float CollisionDamageMult = 1;

            private float Tick;
            private bool inCombat;

            public override void OnDestroy()
            {
                allCharmedAIActors.Remove(this.aiActor);
                allCharmed.Remove(this);
                if (CakeRef)
                {
                    CakeRef.LastOwner.OnNewFloorLoaded -= PlayerNewFloor;
                    CakeRef.LastOwner.OnRoomClearEvent -= LastOwner_OnRoomClearEvent;
                    CakeRef.LastOwner.OnDidUnstealthyAction -= LastOwner_OnDidUnstealthyAction;
                    CakeRef.LastOwner.OnEnteredCombat -= EnteredCombat;

                    CakeRef.LastOwner.companions.Remove(this.aiActor);
                    CakeRef.AllCharmedEnemies.Remove(this);
                }

                base.OnDestroy();
            }
        }

        public class CompanionisedEnemyBulletModifiers : BraveBehaviour //----------------------------------------------------------------------------------------------
        {
            public CompanionisedEnemyBulletModifiers()
            {
                this.scaleDamage = false;
                this.scaleSize = false;
                this.scaleSpeed = false;
                this.doPostProcess = false;
                this.baseBulletDamage = 10f;
                this.TintBullets = false;
                this.TintColor = Color.grey;
                this.jammedDamageMultiplier = 2f;
            }
            List<AIBeamShooter> aIBeamShooters = new List<AIBeamShooter>();
            public void Start()
            {
                aIBeamShooters = base.aiActor.GetComponents<AIBeamShooter>().ToList();

                AIBulletBank bulletBank2 = base.aiActor.bulletBank;
                if (bulletBank2)
                {
                    foreach (AIBulletBank.Entry bullet in bulletBank2.Bullets)
                    {
                        SpawnManager.PoolManager.Remove(bullet.BulletObject.transform);
                        bullet.BulletObject.GetComponent<Projectile>().BulletScriptSettings.preventPooling = true;
                    }
                }

                if (base.aiActor.aiShooter != null)
                {
                    AIShooter aiShooter = base.aiActor.aiShooter;
                    aiShooter.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(aiShooter.PostProcessProjectile, new Action<Projectile>(this.PostProcessSpawnedEnemyProjectiles));
                    
                }

                if (base.aiActor.bulletBank != null)
                {
                    AIBulletBank bulletBank = base.aiActor.bulletBank;
                    bulletBank.OnProjectileCreated = (Action<Projectile>)Delegate.Combine(bulletBank.OnProjectileCreated, new Action<Projectile>(this.PostProcessSpawnedEnemyProjectiles));
                }
            }

            public void Update()
            {
                foreach (var entry in aIBeamShooters)
                {
                    if (entry.LaserBeam != null && entry.LaserBeam.HitsPlayers)
                    {
                        entry.LaserBeam.HitsPlayers = false;
                        entry.LaserBeam.AdjustPlayerBeamTint(TintColor,  10);
                    }
                }
            }

            private void PostProcessSpawnedEnemyProjectiles(Projectile proj)
            {
                if (TintBullets) { proj.AdjustPlayerProjectileTint(this.TintColor, 1); }
                if (base.aiActor != null)
                {
                    if (base.aiActor.aiActor != null)
                    {
                        SpawnManager.PoolManager.Remove(proj.transform);
                        proj.baseData.damage = baseBulletDamage;
                        proj.ImmuneToBlanks = BulletsAreUnBlankable;
                        proj.ImmuneToSustainedBlanks = BulletsAreUnBlankable;

                        if (enemyOwner != null)
                        {
                            //ETGModConsole.Log("Companionise: enemyOwner is not null");
                            if (scaleDamage) proj.baseData.damage *= enemyOwner.stats.GetStatValue(PlayerStats.StatType.Damage);
                            if (scaleSize)
                            {
                                proj.RuntimeUpdateScale(enemyOwner.stats.GetStatValue(PlayerStats.StatType.PlayerBulletScale));
                            }
                            if (scaleSpeed)
                            {
                                proj.baseData.speed *= enemyOwner.stats.GetStatValue(PlayerStats.StatType.ProjectileSpeed);
                                proj.UpdateSpeed();
                            }
                            //ETGModConsole.Log("Damage: " + proj.baseData.damage);
                            if (doPostProcess) enemyOwner.DoPostProcessProjectile(proj);
                        }
                        if (base.aiActor.IsBlackPhantom) { proj.baseData.damage = baseBulletDamage * jammedDamageMultiplier; }
                    }
                }
                else { ETGModConsole.Log("Shooter is NULL"); }
            }

            public PlayerController enemyOwner;
            public bool scaleDamage;
            public bool scaleSize;
            public bool scaleSpeed;
            public bool doPostProcess;
            public float baseBulletDamage;
            public float jammedDamageMultiplier;
            public bool TintBullets;
            public Color TintColor;
            public bool BulletsAreUnBlankable = false;
        }
    }
}
