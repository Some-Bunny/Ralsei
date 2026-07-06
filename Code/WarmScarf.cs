using Alexandria.ItemAPI;
using Alexandria.PrefabAPI;
using Dungeonator;
using MonoMod.RuntimeDetour;
using Ralsei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static ETGMod;

namespace MikuMikuMod
{
    public class WarmScarf : PassiveItem
    {
        public static void Init()
        {
            string itemName = "Warm Scarf";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<WarmScarf>();
            var data = StaticSpriteCollections.ItemAndGunCollection;
            ItemBuilder.AddSpriteToObjectAssetbundle(itemName, data.GetSpriteIdByName("warmscarf"), data, obj);
            string shortDesc = "Handmade";
            string longDesc = "Provides additional health occasionally.\n\nA soft, handmade scarf made by you, with love.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "rlsi");
            item.quality = PickupObject.ItemQuality.SPECIAL;
            item.CanBeDropped = false;
            scarfRef = (PickupObjectDatabase.GetById(436) as BlinkPassiveItem).ScarfPrefab;
            item.PreventStartingOwnerFromDropping = true;
        }

        private CustomScarfDoer scarfInstance;
        public static ScarfAttachmentDoer scarfRef;

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnRoomClearEvent += Player_OnRoomClearEvent;

            scarfInstance = UnityEngine.Object.Instantiate<GameObject>(scarfRef.gameObject).AddComponent<CustomScarfDoer>();

            scarfInstance.AttachTarget = player;
            scarfInstance.ScarfMaterial = new Material(scarfRef.ScarfMaterial);
            scarfInstance.StartWidth = 0.075f;
            scarfInstance.EndWidth = 0.075f;
            scarfInstance.AnimationSpeed = 20f;
            scarfInstance.ScarfLength = 0.4f;
            scarfInstance.AngleLerpSpeed = 4;
            scarfInstance.BackwardZOffset = -2f;
            scarfInstance.CatchUpScale = 0.99f;
            scarfInstance.SinSpeed = 9f;
            scarfInstance.AmplitudeMod = 0.135f;
            scarfInstance.WavelengthMod = 1.1f;

            scarfInstance.ScarfMaterial.SetColor("_OverrideColor", new Color(0.52f, 0.12f, 0.52f));

            scarfInstance.Initialize(player);


        }
        public override DebrisObject Drop(PlayerController player)
        {
            if (this.scarfInstance)
            {
                UnityEngine.Object.Destroy(this.scarfInstance.gameObject);
                this.scarfInstance = null;
            }
            player.OnRoomClearEvent -= Player_OnRoomClearEvent;
            return base.Drop(player);
        }

        public override void OnDestroy()
        {
            if (Owner)
            {
                Owner.OnRoomClearEvent -= Player_OnRoomClearEvent;
            }
            if (this.scarfInstance)
            {
                UnityEngine.Object.Destroy(this.scarfInstance.gameObject);
                this.scarfInstance = null;
            }
            base.OnDestroy();
        }

        private void Player_OnRoomClearEvent(PlayerController obj)
        {
            if (UnityEngine.Random.value < HealthDropChance)
            {
                
                HealthDropChance = 0.0166f;
                IntVector2 bestRewardLocation = obj.CurrentRoom.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
                var debris = LootEngine.SpawnItem(PickupObjectDatabase.GetById(73).gameObject, bestRewardLocation.ToVector3(), Vector2.up, 0, true, false, false);
                AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", obj.gameObject);
                for (int i = 0; i < 32; i++)
                {
                    GlobalSparksDoer.DoRandomParticleBurst(1,
                    debris.sprite.WorldCenter + new Vector2(-0.125f, -0.125f),
                    debris.sprite.WorldCenter + new Vector2(0.125f, 0.125f),
                    BraveUtility.RandomVector2(new Vector2(-1, -1), new Vector2(1,1)) * UnityEngine.Random.Range(1.5f, 2.5f),
                    2f,
                    1f,
                    0.1f,
                    1f,
                    Color.green * 3,
                    GlobalSparksDoer.SparksType.SPARKS_ADDITIVE_DEFAULT);
                }
                return;
            }
            HealthDropChance += 0.0166f;
        }

        public float HealthDropChance = 0.0333f;

        public override void Update()
        {
            base.Update();
        }
    }
}