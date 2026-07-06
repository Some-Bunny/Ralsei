using Alexandria.cAPI;
using Alexandria.PrefabAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using Alexandria.ItemAPI;

namespace Ralsei.Code
{
    public class RalseiCharmable : GameActorEffect
    {
        public static string[] InvalidEnemyGuids = new string[]
        {
            Alexandria.EnemyGUIDs.Advanced_Dragun_Knife_GUID,
            Alexandria.EnemyGUIDs.Draguns_Knife_GUID,
            Alexandria.EnemyGUIDs.Spent_GUID,
            Alexandria.EnemyGUIDs.Chance_Kin_GUID,
            Alexandria.EnemyGUIDs.Key_Bullet_Kin_GUID,
            Alexandria.EnemyGUIDs.Mountain_Cube_GUID,
            Alexandria.EnemyGUIDs.Lead_Cube_GUID,
            Alexandria.EnemyGUIDs.Flesh_Cube_GUID,
            Alexandria.EnemyGUIDs.Grip_Master_GUID,
            Alexandria.EnemyGUIDs.Tarnisher_GUID,
            Alexandria.EnemyGUIDs.Gunreaper_GUID,
            Alexandria.EnemyGUIDs.Fuselier_GUID,
            Alexandria.EnemyGUIDs.Blockners_Ghost_GUID,
            Alexandria.EnemyGUIDs.Shadow_Magician_GUID,
        };

        public static Dictionary<string, Vector2> AdditionalHealthCostModifiers = new Dictionary<string, Vector2>
        {
            { Alexandria.EnemyGUIDs.Ammomancer_GUID, new Vector2(1, 1) },
            { Alexandria.EnemyGUIDs.Jammomancer_GUID, new Vector2(1, 1.25f) },
            { Alexandria.EnemyGUIDs.Jamerlengo_GUID, new Vector2(1, 1.25f) },
            { Alexandria.EnemyGUIDs.Gunsinger_GUID, new Vector2(0, 1.5f) },
            { Alexandria.EnemyGUIDs.Aged_Gunsinger_GUID, new Vector2(0, 1.5f) },
            { Alexandria.EnemyGUIDs.Spogre_GUID, new Vector2(1, 1) },
            { Alexandria.EnemyGUIDs.Misfire_Beast_GUID, new Vector2(2, 1.1f) },
            { Alexandria.EnemyGUIDs.Bombshee_GUID, new Vector2(1, 1.1f) },
        };

        public static List<AIActor> InvalidSpecificEnemies = new List<AIActor>();



        public RalseiCharmable()
        {
            this.AppliesTint = false;
            this.AppliesDeathTint = false;
            this.AffectsPlayers = false;
            this.AffectsEnemies = true;
            this.stackMode = EffectStackingMode.DarkSoulsAccumulate;
            this.effectIdentifier = "ralsei:charmablestacking";
            this.currentStackAmount = 1;
            this.duration = 10000;
            this.StackAmountToApply = 1;
        }

        public int currentStackAmount = 1;
        public int MaxStacks = 3;
        public int StackAmountToApply = 1;


        private RalseiCharmVFX extantOverheadder;
        private GameActor Enemy;

        private CrazedController CrazedController;



        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            CrazedController = actor.gameObject.GetComponent<CrazedController>();
            Vector2 Mod = new Vector2(0, 1);


            if (CrazedController != null && CrazedController.m_state != CrazedController.State.Idle)
            {
                return;
            }
            if (actor.gameObject.GetComponent<MirrorImageController>() != null)
            {
                return;
            }
            if (actor.gameObject.GetComponent<DisplacedImageController>() != null)
            {
                return;
            }

            if (actor is AIActor enemy)
            {
                if (InvalidSpecificEnemies.Contains(enemy))
                {
                    return;
                }
                if (InvalidEnemyGuids.Contains(enemy.EnemyGuid))
                {
                    base.OnEffectRemoved(actor, effectData);
                    return;
                }
                if (AdditionalHealthCostModifiers.ContainsKey(enemy.EnemyGuid))
                {
                    Mod = AdditionalHealthCostModifiers[enemy.EnemyGuid];
                }
                if (enemy.HasTag("Ralsei:Uncharmable"))
                {
                    return;
                }
            }



            HeartDrop = 1;
            MaxStacks = Mathf.RoundToInt(Mathf.Sqrt((actor.healthHaver.GetMaxHealth() * Mod.y) / 2.5f) + (1 + Mod.x));
            MaxStacks = Mathf.Min(16, MaxStacks);
            currentStackAmount = Mathf.Min(StackAmountToApply, MaxStacks);

            extantOverheadder = actor.SmartPlayEffectOnActor(RalseiCharmVFX.EffectPrefab.gameObject, new Vector3(0, 1f), true, true, false, false).GetComponent<RalseiCharmVFX>();
            extantOverheadder.InitEffect(MaxStacks);
            extantOverheadder.OnAmountChanged(currentStackAmount, MaxStacks);
            base.OnEffectApplied(actor, effectData, partialAmount);
            Enemy = actor;
            actor.healthHaver.OnPreDeath += HealthHaver_OnPreDeath;


            float c = Mathf.Sqrt(actor.healthHaver.GetMaxHealth()) * 0.3f;
            DegradeMult = Mathf.Max(1.5f, DegradeMult - c);
        }

        public float DegradeMult = 5f;
        private float DegradePrevention = 0;

        private void HealthHaver_OnPreDeath(Vector2 obj)
        {         
            UnityEngine.Object.Destroy(extantOverheadder.gameObject);
            currentStackAmount = 0;
            Enemy.healthHaver.OnPreDeath -= HealthHaver_OnPreDeath;
            Enemy.RemoveEffect(this.effectIdentifier);
        }

        public float HeartDrop = 1;

        public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            Vector2 v = actor.transform.position.XY();
            if (actor.specRigidbody && actor.specRigidbody.HitboxPixelCollider != null) { v = actor.specRigidbody.HitboxPixelCollider.UnitTopCenter.Quantize(0.0625f); }
            if (extantOverheadder)
                extantOverheadder.UpdateBar(HeartDrop);

            if (HeartDrop > 0)
            {
                if (DegradePrevention > 0)
                    DegradePrevention -= Time.deltaTime;
                else
                    HeartDrop -= (currentStackAmount == MaxStacks ? Time.deltaTime / (DegradeMult * 1.5f) : Time.deltaTime / DegradeMult);
            }
            else
            {
                currentStackAmount = Mathf.Max(0, currentStackAmount - (MaxStacks - 1));
                HeartDrop = 1;
                extantOverheadder.OnAmountChanged(currentStackAmount, MaxStacks);
                if (currentStackAmount == 0)
                    OnEffectRemoved(actor, effectData);

            }
            if (CrazedController != null)
            {
                if (CrazedController.m_state != CrazedController.State.Idle)
                    OnEffectRemoved(actor, effectData);
            }

            base.EffectTick(actor, effectData);
        }
        public override bool IsFinished(GameActor actor, RuntimeGameActorEffectData effectData, float elapsedTime)
        {
            return extantOverheadder == null;
        }
        public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            if (extantOverheadder)
            {
                UnityEngine.Object.Destroy(extantOverheadder.gameObject);
            }
            currentStackAmount = 0;
            if (Enemy)
            {
                Enemy.healthHaver.OnPreDeath -= HealthHaver_OnPreDeath;
            }
            base.OnEffectRemoved(actor, effectData);
        }
        public override void OnDarkSoulsAccumulate(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1, Projectile sourceProjectile = null)
        {
            for (int i = 0; i < actor.m_activeEffects.Count; i++)
            {
                if (actor.m_activeEffects[i].effectIdentifier == effectIdentifier && actor.m_activeEffects[i] is RalseiCharmable ralseiCharmable)
                {
                    ralseiCharmable.AddStackLayer(actor);
                }
            }
            base.OnDarkSoulsAccumulate(actor, effectData, partialAmount, sourceProjectile);
        }

        
        public void AddStackLayer(GameActor actor)
        {
            bool wasNotLastStackBefore = currentStackAmount != MaxStacks;
            currentStackAmount = Mathf.Min(currentStackAmount + StackAmountToApply, MaxStacks);
            if (currentStackAmount == MaxStacks && wasNotLastStackBefore == true)
            {
                HeartDrop = 0.6f;
                if (Enemy)
                {
                    Enemy.behaviorSpeculator.Stun(1f);
                    Enemy.StartCoroutine(owo());
                }
            }
            if (currentStackAmount > MaxStacks - 2)
            {
            }
            DegradePrevention = 0.5f;
            HeartDrop = Mathf.Min((HeartDrop * 1.1f) +0.025f, 1);
            extantOverheadder?.OnAmountChanged(currentStackAmount, MaxStacks);
        }

        public IEnumerator owo()
        {
            yield return new WaitForSeconds(0.5f);
            AkSoundEngine.PostEvent("Play_NPC_Blessing_Synergy_Get_01", Enemy.gameObject);
            for (int i = 0; i < 3; i++)
            {
                if (Enemy == null) { yield break; }
                for (int a = 0; a < 16; a++)
                {
                    GlobalSparksDoer.DoRandomParticleBurst(1, Enemy.sprite.WorldCenter, Enemy.sprite.WorldCenter,
                        Toolbox.GetUnitOnCircle(BraveUtility.RandomAngle(), 5 + (1.5f * i)),
                        1f, 0.05f, 0.1f, 1f, Color.white * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                }
                yield return new WaitForSeconds(0.1666f);
            }
        }

    }
}
