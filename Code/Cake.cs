using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Dungeonator;
using System.Reflection;
using Random = System.Random;
using FullSerializer;
using System.Collections;
using Gungeon;
using MonoMod.RuntimeDetour;
using MonoMod;
using Pathfinding;
using static tk2dSpriteCollectionDefinition;
using Alexandria.ItemAPI;
using Ralsei.Code;
using static ETGMod;
using Alexandria.PrefabAPI;
using HarmonyLib;
using System.Runtime.CompilerServices;
using Ralsei.Code.VFX;
using static Alexandria.DungeonAPI.SpecialComponents;
using Alexandria.SoundAPI;
using static UnityEngine.TouchScreenKeyboard;
using MonoMod.Cil;

namespace Ralsei
{


    public class EnemyHealthBarRalsei : MonoBehaviour
    {
        private AIActor MyEnemy;
        public tk2dSprite HealthbarSprite;
        public tk2dSprite Heart;

        public void InitEffect(AIActor aIActor)
        {
            MyEnemy = aIActor;
        }

        public void Update()
        {
            float e = MyEnemy.healthHaver.currentHealth / MyEnemy.healthHaver.GetMaxHealth();
            HealthbarSprite.scale = new Vector3(e, 1, 1);
            HealthbarSprite.color = GetColor(e);
            var v = Heart.transform;
            v.localPosition = new Vector3(v.localPosition.x,  (Easing.DoLerpT((Mathf.PingPong(Time.timeSinceLevelLoad, 1)) - 0.5f) * 0.25f));
        }

        public Color GetColor(float value)
        {
            return Color.Lerp(Color.red, new Color(0.5f, 1, 0.2f), value);
        }
    }




    public class Cake : PlayerItem, ILabelItem
    {
        public static void Init()
        {
            string itemName = "Slice Of Cake";
            GameObject obj = new GameObject(itemName);
            Cake activeitem = obj.AddComponent<Cake>();
            var data = StaticSpriteCollections.ItemAndGunCollection;
            ItemBuilder.AddSpriteToObjectAssetbundle(itemName, data.GetSpriteIdByName("ralseicake"), data, obj);
            string shortDesc = "For Sharing!";
            string longDesc = "Allows you to turn enemies to your side, once they have been shown enough love, using up a charge. Charges are restored by using the active on heart pickups. Passively provides a chance for additional health drops. Made purely out of milk.";
            activeitem.SetupItem(shortDesc, longDesc, "rlsi");
            activeitem.SetCooldownType(ItemBuilder.CooldownType.Timed, 1);
            activeitem.consumable = false;
            activeitem.quality = PickupObject.ItemQuality.SPECIAL;
            activeitem.CanBeDropped = false;
            ItemBuilder.AddPassiveStatModifier(activeitem, PlayerStats.StatType.AdditionalItemCapacity, 1, StatModifier.ModifyMethod.ADDITIVE);
            activeitem.PreventStartingOwnerFromDropping = true;
            //new Hook(typeof(GameUIItemController).GetMethod("UpdateItem", BindingFlags.Instance | BindingFlags.Public), typeof(Cake).GetMethod("UpdateCustomLabel"));

            var healtbhar = PrefabBuilder.BuildObject("HealthbarRalsei");
            var effect = healtbhar.AddComponent<EnemyHealthBarRalsei>();
            DontDestroyOnLoad(healtbhar);
            healtbhar.layer = LayerMask.NameToLayer("FG_Critical");
            var spriteBar = healtbhar.AddComponent<tk2dSprite>();
            spriteBar.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_bar_002");
            effect.HealthbarSprite = spriteBar;
            spriteBar.HeightOffGround = -10;
            spriteBar.SortingOrder = 20;
            spriteBar.usesOverrideMaterial = true;
            var mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = spriteBar.renderer.material.mainTexture;
            spriteBar.renderer.material = mat;

            var healtbhar_back = PrefabBuilder.BuildObject("HealthbarRalseiBack");
            healtbhar_back.layer = LayerMask.NameToLayer("FG_Critical");
            spriteBar = healtbhar_back.AddComponent<tk2dSprite>();
            spriteBar.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_bar_002");
            spriteBar.HeightOffGround = -20;
            spriteBar.SortingOrder = 8;
            spriteBar.color = new Color(0, 0.2f, 0.05f);
            spriteBar.usesOverrideMaterial = true;
            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = spriteBar.renderer.material.mainTexture;
            spriteBar.renderer.material = mat;
            healtbhar_back.transform.SetParent(healtbhar.transform);

            var healtbhar_heart = PrefabBuilder.BuildObject("HealthbarRalseiBack");
            healtbhar_heart.layer = LayerMask.NameToLayer("FG_Critical");
            spriteBar = healtbhar_heart.AddComponent<tk2dSprite>();
            spriteBar.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_heart_004");
            spriteBar.HeightOffGround = -10;
            spriteBar.SortingOrder = 8;
            spriteBar.usesOverrideMaterial = true;
            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = spriteBar.renderer.material.mainTexture;
            spriteBar.renderer.material = mat;
            healtbhar_heart.transform.SetParent(healtbhar.transform);
            healtbhar_heart.transform.localPosition -= new Vector3(0.375f, 0);
            effect.Heart = spriteBar;
            EffectPrefab = healtbhar;

            Alexandria.ItemAPI.CustomSynergies.Add("A Hearty Meal", new List<string> { "rlsi:slice_of_cake" }, new List<string> { "antibody" }, false);
            Alexandria.ItemAPI.CustomSynergies.Add("You And What Army", new List<string> { "rlsi:slice_of_cake" }, new List<string> { "battle_standard" }, false);
            Alexandria.ItemAPI.CustomSynergies.Add("Pacify", new List<string> { "rlsi:slice_of_cake" }, new List<string> { "charm_horn" }, false);
            Alexandria.ItemAPI.CustomSynergies.Add("Memories Of Home", new List<string> { "rlsi:slice_of_cake" }, new List<string> { "really_special_lute" }, false);
            Alexandria.ItemAPI.CustomSynergies.Add("Group Huddle", new List<string> { "rlsi:slice_of_cake" }, new List<string> { "old_knights_flask" }, false);
            Alexandria.ItemAPI.CustomSynergies.Add("Heart Maker", new List<string> { "rlsi:slice_of_cake" }, new List<string> { "charmed_bow" }, false);
            Alexandria.ItemAPI.CustomSynergies.Add("Absorb", new List<string> { "rlsi:slice_of_cake" }, new List<string> { "blood_brooch" }, false);

            var entry = CustomSynergies.Add("...And Tea For The Rest", new List<string>()
            {
                "rlsi:slice_of_cake",
                "teapot",
            });



            entry.bonusSynergies = new List<CustomSynergyType>() { CustomSynergyType.TEA_FOR_TWO };
            SoundManager.AddCustomSwitchData("WPN_Guns", "rlsi:newspeciallute", "Play_WPN_Gun_Shot_01", "Play_RalseiLute");
            SoundManager.AddCustomSwitchData("WPN_Guns", "rlsi:newspeciallute", "Stop_WPN_Gun_Loop_01", "Stop_RalseiLute");
            SoundManager.AddCustomSwitchData("WPN_Guns", "rlsi:newspeciallute", "Play_WPN_gun_empty_01", "Stop_RalseiLute");
            (PickupObjectDatabase.GetById(506) as Gun).gameObject.AddComponent<LuteSwitch>();
            (PickupObjectDatabase.GetById(200) as Gun).gameObject.AddComponent<CharmedBowModifier>();

        }
        public static GameObject EffectPrefab;
        public static GameActorCharmEffect charmingRoundsEffect = PickupObjectDatabase.GetById(527).GetComponent<BulletStatusEffectItem>().CharmModifierEffect;
        public static GameActorFireEffect hotLeadEffect = PickupObjectDatabase.GetById(295).GetComponent<BulletStatusEffectItem>().FireModifierEffect;

        public RalseiTargetVFX targetVFXInst = null;


        public int StoredHearts = 7;
        public float StoredArmor = 0;
        public List<AIActorModifiers.RalseiCharmedEnemyController> AllCharmedEnemies = new List<AIActorModifiers.RalseiCharmedEnemyController>();



        public string GetLabel() 
        {
            return StoredHearts.ToString();
        }


        public override void Pickup(PlayerController player)
        {
            //player.OnNewFloorLoaded += ONFL;
            player.OnRoomClearEvent += Player_OnRoomClearEvent;
            base.Pickup(player);
        }
        private bool _Check = false;
        public void ONFL(PlayerController player)
        {
            _Check = !_Check;
            if (_Check)
            {
                StoredHearts += 3;
                Debug.Log($"AAAAAA {StoredHearts}");
            }
            AllCharmedEnemies.RemoveAll(self => self == null);
            foreach (var entry in AllCharmedEnemies)
            {
                if (entry == null) { continue; }
                entry.aiActor.parentRoom = GameManager.Instance.Dungeon.data.Entrance;
            }
        }


        private void Player_OnRoomClearEvent(PlayerController obj)
        {
            if (UnityEngine.Random.value < HealthDropChance)
            {
                HealthDropChance = 0.02f;
                IntVector2 bestRewardLocation = obj.CurrentRoom.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
                var debris = LootEngine.SpawnItem(PickupObjectDatabase.GetById(73).gameObject, bestRewardLocation.ToVector3(), Vector2.up, 0, true, false, false);
                AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", obj.gameObject);
                for (int i = 0; i < 32; i++)
                {
                    GlobalSparksDoer.DoRandomParticleBurst(1,
                    debris.sprite.WorldCenter + new Vector2(-0.125f, -0.125f),
                    debris.sprite.WorldCenter + new Vector2(0.125f, 0.125f),
                    BraveUtility.RandomVector2(new Vector2(-1, -1), new Vector2(1, 1)) * UnityEngine.Random.Range(1.5f, 2.5f),
                    2f,
                    1f,
                    0.1f,
                    0.625f,
                    Color.green * 3,
                    GlobalSparksDoer.SparksType.SPARKS_ADDITIVE_DEFAULT);
                }
                return;
            }
            HealthDropChance += 0.01f;
        }

        public float HealthDropChance = 0.02f;
        public override void OnPreDrop(PlayerController user)
        {
            user.OnNewFloorLoaded -= ONFL;
            user.OnRoomClearEvent -= Player_OnRoomClearEvent;
            base.OnPreDrop(user);
        }

        public override void OnDestroy()
        {
            if (base.LastOwner != null)
            {
                LastOwner.OnNewFloorLoaded -= ONFL;
                LastOwner.OnRoomClearEvent -= Player_OnRoomClearEvent;
            }
            foreach (var entry in AllCharmedEnemies)
            {
                Destroy(entry.aiActor.gameObject);
            }
            AllCharmedEnemies.Clear();
            base.OnDestroy();
        }

        private bool Inited = false;

        private float E = 0;
        public override void Update()
        {
            base.Update();
            if (LastOwner)
            {
                
                if (Inited == false)
                {
                    Inited = true;
                    LastOwner.OnRoomClearEvent += Player_OnRoomClearEvent;
                    LastOwner.OnNewFloorLoaded += ONFL; //This runs twice, but only if I put it here, and the actions in Pickup() dont seem to get registered because ???
                }
                if (CurrentEnemy != null)
                {
                    if (CurrentEnemy.healthHaver.IsDead) { CurrentEnemy = null; return; }
                    E += 360 * Time.deltaTime;
                    var m_1 = CurrentEnemy.sprite.WorldBottomCenter + Toolbox.GetUnitOnCircle(E, 1.25f);
                    var m_2 = CurrentEnemy.sprite.WorldBottomCenter + Toolbox.GetUnitOnCircle(E + 180, 1.25f);
                    GlobalSparksDoer.DoRandomParticleBurst(1, m_2, m_2, Vector3.up, 1f, 0.05f, 0.1f, 0.5f, Color.green * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                    GlobalSparksDoer.DoRandomParticleBurst(1, m_1, m_1, Vector3.up, 1f, 0.05f, 0.1f, 0.5f, Color.green * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                }
            }
        }
        public AIActor CurrentEnemy = null;
        public override bool CanBeUsed(PlayerController user)
        {
            var room = user.CurrentRoom;
            if (room != null)
            {
                if (StoredHearts > 0)
                {
                    List<AIActor> _ = new List<AIActor>();
                    room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref _);
                    if (_ != null)
                    {
                        _.RemoveAll(self => Vector2.Distance(self.sprite.WorldCenter, user.sprite.WorldCenter) > 5f);
                        if (_.Count() > 0)
                        {
                            foreach (var enemy in _)
                            {
                                foreach (var entry in enemy.m_activeEffects)
                                {
                                    if (entry is RalseiCharmable charmable && charmable.MaxStacks == charmable.currentStackAmount)
                                    {
                                        if (enemy.healthHaver.IsDead == false && charmable.MaxStacks <= StoredHearts)
                                        {
                                            CurrentEnemy = enemy;
                                        }
                                        return true;
                                    }
                                }
                            }
                            CurrentEnemy = null;
                        }
                    }
                }
                
            }
            var interactable = user.GetLastInteractable();
            if (interactable != null)
            {
                if (interactable is HealthPickup pickup)
                {
                    return true;
                }
                if (interactable is ShopItemController controller && controller.item is HealthPickup health)
                {
                    if (controller.ShouldSteal(user))
                    {
                        return true; 
                    }
                    switch (controller.CurrencyType)
                    {
                        case ShopItemController.ShopCurrencyType.COINS:
                            return user.carriedConsumables.Currency >= controller.ModifiedPrice;
                        case ShopItemController.ShopCurrencyType.BLANKS:
                            return user.carriedConsumables.Currency >= controller.ModifiedPrice;
                        case ShopItemController.ShopCurrencyType.KEYS:
                            return user.carriedConsumables.KeyBullets >= controller.ModifiedPrice;
                        case ShopItemController.ShopCurrencyType.META_CURRENCY:
                            return Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY)) >= controller.ModifiedPrice;
                    }
                    return true;
                }
            }       
            return false;
        }

        public override void DoEffect(PlayerController user)
        {
            if (CurrentEnemy != null)
            {
                foreach (var entry in CurrentEnemy.m_activeEffects)
                {
                    if (entry is RalseiCharmable charmable)
                    {
                        StoredHearts -= charmable.MaxStacks;
                    }
                }
                AkSoundEngine.PostEvent("Play_NPC_faerie_heal_01", user.gameObject);
                for (int i = 0; i < 16; i++)
                {
                    ParticleBase.EmitParticles("HeartParticle", 1, new ParticleSystem.EmitParams()
                    {
                        position = new Vector3(UnityEngine.Random.Range(CurrentEnemy.sprite.WorldBottomLeft.x, CurrentEnemy.sprite.WorldBottomRight.x),
                        UnityEngine.Random.Range(CurrentEnemy.sprite.WorldBottomRight.y, CurrentEnemy.sprite.WorldTopRight.y)),
                        velocity = BraveUtility.RandomVector2(new Vector2(0, 0.75f), new Vector2(0, 2.5f)),
                        startLifetime = 1.5f,
                        startSize = 0.375f
                    });
                }
                GameObject blankObj = GameObject.Instantiate((GameObject)ResourceCache.Acquire("Global VFX/BlankVFX_Ghost"), CurrentEnemy.sprite.WorldCenter, Quaternion.identity);
                Destroy(blankObj, 2f);
                AIActorModifiers.RalseifyEnemy(CurrentEnemy, this, false);
                CurrentEnemy = null;
                return;
            }


            var interactable = user.GetLastInteractable();
            if (interactable != null)
            {
                bool synergyActivated = false;
                if (interactable is HealthPickup pickup)
                {
                    AkSoundEngine.PostEvent("Play_OBJ_bottle_cork_01", user.gameObject);
                    StoredHearts += DetermineHealing(pickup, ref synergyActivated);
                    DoPickupVisual(pickup, synergyActivated);
                    Destroy(pickup.gameObject);
                }
                if (interactable is ShopItemController controller && controller.item is HealthPickup health)
                {
                    AkSoundEngine.PostEvent("Play_OBJ_bottle_cork_01", user.gameObject);
                    StoredHearts += DetermineHealing(health, ref synergyActivated);
                    DoPickupVisual(health, synergyActivated);
                    FakeInteract(controller, user);
                }
                return;
            }
        }

        public void DoPickupVisual(PickupObject pickup, bool synergyActivated = false)
        {
            tk2dSprite targetSprite = tk2dSprite.AddComponent(new GameObject("sucked sprite")
            {
                transform =
                    {
                        position = pickup.transform.position
                    }
            }, pickup.sprite.collection, pickup.sprite.spriteId);
            GameManager.Instance.Dungeon.StartCoroutine(this.HandleSuck(targetSprite));
            for (int i = 0; i < 24; i++)
            {
                GlobalSparksDoer.DoRandomParticleBurst(1, targetSprite.sprite.WorldCenter, targetSprite.sprite.WorldCenter,
                    Toolbox.GetUnitOnCircle(15 * i, 2), 1f, 0.05f, 0.125f, 0.5f, Color.red * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
            }
            if (synergyActivated)
            {
                for (int i = 0; i < 24; i++)
                {
                    GlobalSparksDoer.DoRandomParticleBurst(1, targetSprite.sprite.WorldCenter, targetSprite.sprite.WorldCenter,
                        Toolbox.GetUnitOnCircle((15 * i) + 7.5f, 3), 1f, 0.05f, 0.1f, 1.25f, Color.red * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                }
            }
        }


        public int DetermineHealing(HealthPickup healthPickup, ref bool SynergyActivated)
        {
            int amountToHeal = 0;
            float MultAmount = 6;
            float MultAmountArmor = 12;
            if (this.LastOwner.PlayerHasActiveSynergy("A Hearty Meal") && UnityEngine.Random.value >= 0.5f)
            {
                SynergyActivated = true;
                MultAmount *= 2;
            }
            amountToHeal += (int)(healthPickup.healAmount * MultAmount);
            amountToHeal += (int)(healthPickup.armorAmount * MultAmountArmor);
            return Mathf.RoundToInt(amountToHeal);
        }


        private IEnumerator HandleSuck(tk2dSprite targetSprite)
        {
            float elapsed = 0f;
            float duration = 0.5f;
            PlayerController owner = this.LastOwner;
            if (targetSprite)
            {
                Vector3 startPosition = targetSprite.transform.position;
                while (elapsed < duration && owner)
                {
                    elapsed += BraveTime.DeltaTime;
                    if (targetSprite)
                    {
                        targetSprite.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), elapsed / duration);
                        targetSprite.transform.position = Vector3.Lerp(startPosition, owner.CenterPosition.ToVector3ZisY(0f), elapsed / duration);
                    }
                    yield return null;
                }
            }
            UnityEngine.Object.Destroy(targetSprite.gameObject);
            yield break;
        }

        public void FakeInteract(ShopItemController shopItemController, PlayerController player)
        {

            shopItemController.LastInteractingPlayer = player;
            if (shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.COINS || shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.BLANKS || shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.KEYS)
            {
                bool flag = false;
                bool flag2 = true;
                if (shopItemController.ShouldSteal(player))
                {
                    flag = shopItemController.m_baseParentShop.AttemptToSteal();
                    flag2 = false;
                    if (!flag)
                    {
                        player.DidUnstealthyAction();
                        shopItemController.m_baseParentShop.NotifyStealFailed();
                        return;
                    }
                }
                if (flag2)
                {
                    bool flag3 = false;
                    if (shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.COINS || shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.BLANKS)
                    {
                        flag3 = (player.carriedConsumables.Currency >= shopItemController.ModifiedPrice);
                    }
                    else if (shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.KEYS)
                    {
                        flag3 = (player.carriedConsumables.KeyBullets >= shopItemController.ModifiedPrice);
                    }
                    if (shopItemController.IsResourcefulRatKey)
                    {
                        if (!flag3)
                        {
                            int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY));
                            if (num >= 1000)
                            {
                                AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
                                if (shopItemController.m_parentShop != null)
                                {
                                    shopItemController.m_parentShop.NotifyFailedPurchase(shopItemController);
                                }
                                if (shopItemController.m_baseParentShop != null)
                                {
                                    shopItemController.m_baseParentShop.NotifyFailedPurchase(shopItemController);
                                }
                                return;
                            }
                            if (player.carriedConsumables.Currency > 0)
                            {
                                GameStatsManager.Instance.RegisterStatChange(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY, (float)player.carriedConsumables.Currency);
                                player.carriedConsumables.Currency = 0;
                                shopItemController.OnExitRange(player);
                                shopItemController.OnEnteredRange(player);
                            }
                            else
                            {
                                AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
                                if (shopItemController.m_parentShop != null)
                                {
                                    shopItemController.m_parentShop.NotifyFailedPurchase(shopItemController);
                                }
                                if (shopItemController.m_baseParentShop != null)
                                {
                                    shopItemController.m_baseParentShop.NotifyFailedPurchase(shopItemController);
                                }
                            }
                            return;
                        }
                        else
                        {
                            player.carriedConsumables.Currency -= shopItemController.ModifiedPrice;
                            GameStatsManager.Instance.RegisterStatChange(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY, (float)shopItemController.ModifiedPrice);
                            flag2 = false;
                        }
                    }
                    else if (!flag3)
                    {
                        AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
                        if (shopItemController.m_parentShop != null)
                        {
                            shopItemController.m_parentShop.NotifyFailedPurchase(shopItemController);
                        }
                        if (shopItemController.m_baseParentShop != null)
                        {
                            shopItemController.m_baseParentShop.NotifyFailedPurchase(shopItemController);
                        }
                        return;
                    }
                }
                if (!shopItemController.pickedUp)
                {
                    shopItemController.pickedUp = !shopItemController.item.PersistsOnPurchase;
                    //LootEngine.GivePrefabToPlayer(shopItemController.item.gameObject, player);
                    if (flag2)
                    {
                        if (shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.COINS || shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.BLANKS)
                        {
                            player.carriedConsumables.Currency -= shopItemController.ModifiedPrice;
                        }
                        else if (shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.KEYS)
                        {
                            player.carriedConsumables.KeyBullets -= shopItemController.ModifiedPrice;
                        }
                    }
                    if (shopItemController.m_parentShop != null)
                    {
                        shopItemController.m_parentShop.PurchaseItem(shopItemController, !flag, true);
                    }
                    if (shopItemController.m_baseParentShop != null)
                    {
                        shopItemController.m_baseParentShop.PurchaseItem(shopItemController, !flag, true);
                    }
                    if (flag)
                    {
                        StatModifier statModifier = new StatModifier();
                        statModifier.statToBoost = PlayerStats.StatType.Curse;
                        statModifier.amount = 1f;
                        statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
                        player.ownerlessStatModifiers.Add(statModifier);
                        player.stats.RecalculateStats(player, false, false);
                        player.HandleItemStolen(shopItemController);
                        shopItemController.m_baseParentShop.NotifyStealSucceeded();
                        player.IsThief = true;
                        GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_ITEMS_STOLEN, 1f);
                        if (shopItemController.SetsFlagOnSteal)
                        {
                            GameStatsManager.Instance.SetFlag(shopItemController.FlagToSetOnSteal, true);
                        }
                    }
                    else
                    {
                        if (shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.BLANKS)
                        {
                            player.Blanks++;
                        }
                        player.HandleItemPurchased(shopItemController);
                    }
                    if (!shopItemController.item.PersistsOnPurchase)
                    {
                        GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
                    }
                    AkSoundEngine.PostEvent("Play_OBJ_item_purchase_01", base.gameObject);
                }
            }
            else if (shopItemController.CurrencyType == ShopItemController.ShopCurrencyType.META_CURRENCY)
            {
                int num2 = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY));
                if (num2 < shopItemController.ModifiedPrice)
                {
                    AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
                    if (shopItemController.m_parentShop != null)
                    {
                        shopItemController.m_parentShop.NotifyFailedPurchase(shopItemController);
                    }
                    if (shopItemController.m_baseParentShop != null)
                    {
                        shopItemController.m_baseParentShop.NotifyFailedPurchase(shopItemController);
                    }
                    return;
                }
                if (!shopItemController.pickedUp)
                {
                    shopItemController.pickedUp = !shopItemController.item.PersistsOnPurchase;
                    GameStatsManager.Instance.ClearStatValueGlobal(TrackedStats.META_CURRENCY);
                    GameStatsManager.Instance.SetStat(TrackedStats.META_CURRENCY, (float)(num2 - shopItemController.ModifiedPrice));
                    GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY_SPENT_AT_META_SHOP, (float)shopItemController.ModifiedPrice);
                    //LootEngine.GivePrefabToPlayer(shopItemController.item.gameObject, player);
                    if (shopItemController.m_parentShop != null)
                    {
                        shopItemController.m_parentShop.PurchaseItem(shopItemController, true, true);
                    }
                    if (shopItemController.m_baseParentShop != null)
                    {
                        shopItemController.m_baseParentShop.PurchaseItem(shopItemController, true, true);
                    }
                    player.HandleItemPurchased(shopItemController);
                    if (!shopItemController.item.PersistsOnPurchase)
                    {
                        GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
                    }
                    AkSoundEngine.PostEvent("Play_OBJ_item_purchase_01", base.gameObject);
                }
            }
        }


        public override void MidGameSerialize(List<object> data)
        {
            base.MidGameSerialize(data);
            data.Add(StoredHearts);

            AllCharmedEnemies.RemoveAll(x => x == null);

            data.Add(AllCharmedEnemies.Count);
            foreach (var entry in AllCharmedEnemies)
            {
                if (entry.aiActor != null)
                {
                    data.Add(entry.aiActor.EnemyGuid);
                    data.Add(entry.aiActor.healthHaver.currentHealth);
                    data.Add(entry.aiActor.healthHaver.maximumHealth);
                }
            }
        }

        public override void MidGameDeserialize(List<object> data)
        {
            base.MidGameDeserialize(data);
            int i = 2;
            StoredHearts = (int)data[0];
            StoredHearts += 3;
            int AmountOfEnemies = (int)data[1];
            for (int i_ = 0; i_ < AmountOfEnemies; i_++) 
            {
                string EnemyGuid = (string)data[i++];
                float CurrentEnemyHealth = (float)data[i++];
                float CurrentEnemymaxHP = (float)data[i++];
                this.StartCoroutine(DoSpawn(EnemyGuid, CurrentEnemyHealth, CurrentEnemymaxHP));
            }
        }

        public IEnumerator DoSpawn(string GUID, float HP, float Max)
        {
            yield return null;
            while (LastOwner == null)
            {
                yield return null;
            }
            while (LastOwner.CurrentRoom == null)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.25f);
            var enemy = AIActor.Spawn(EnemyDatabase.GetOrLoadByGuid(GUID), this.LastOwner.transform.position, this.LastOwner.CurrentRoom, false, AIActor.AwakenAnimationType.Default, true);
            yield return null;
            enemy.reinforceType = AIActor.ReinforceType.Instant;
            enemy.RalseifyEnemy(this, false);
            enemy.healthHaver.SetHealthMaximum(Max);
            enemy.healthHaver.currentHealth = HP;
        }
    }
}



