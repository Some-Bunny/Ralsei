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
using Alexandria.Assetbundle;
using Alexandria.ItemAPI;
using Alexandria.PrefabAPI;
using HarmonyLib;
using Alexandria.VisualAPI;
using Ralsei.Code;
using Ralsei.Code.VFX;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Ralsei
{
    public class ScarfWeapon : GunBehaviour
    {
        public static void Add()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Warm Scarf", "ralseisverywarmscarf");
            Game.Items.Rename("outdated_gun_mods:warm_scarf", "rlsi:warm_scarf");
            gun.gameObject.AddComponent<ScarfWeapon>();
            gun.SetShortDescription("Fluffy And Cozy");
            gun.SetLongDescription("A warm, hand-knitted scarf. Show love to your enemies, and maybe they will turn to your side. Rolling allows for a longer-range hug that pulls attention for your friends, and holding the reload button charges up a radial hug with increased love.");

            GunInt.SetupSpritePrebaked(gun, StaticSpriteCollections.ItemAndGunCollection, "warmscarf");
            gun.spriteAnimator.Library = StaticSpriteCollections.ItemAndGunAnimation;
            gun.sprite.SortingOrder = 1;

            gun.reloadAnimation = "scarf_true";
            gun.idleAnimation = "scarf_true";
            gun.shootAnimation = "scarf_true";
            gun.sprite.scale = Vector3.zero;
            //gun.sprite.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "warmscarf");

            gun.PreventOutlines = true;
            gun.CanBeDropped = false;
            gun.PreventStartingOwnerFromDropping = true;

            GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(86) as Gun, true, true);
            GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(86) as Gun, true, false);
            GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(86) as Gun, true, false);

            for (int i = 0; i < gun.Volley.projectiles.Count; i++)
            {
                var entry = gun.Volley.projectiles[i];
                entry.ammoCost = 1;
                entry.shootStyle = ProjectileModule.ShootStyle.Automatic;
                entry.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                entry.cooldownTime = i == 0 ? 0.7f : 0.25f;
                entry.numberOfShotsInClip = -1;
                entry.angleVariance = 5f;
            }
   
            gun.reloadTime = 0;
            gun.PreventNormalFireAudio = true;
            gun.OverrideNormalFireAudioEvent = "";
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(335) as Gun).gunSwitchGroup;
            gun.SetBaseMaxAmmo(360);
            gun.InfiniteAmmo = true;
            gun.quality = PickupObject.ItemQuality.SPECIAL;
            gun.CurrentStrengthTier = 0;
            gun.Volley.ModulesAreTiers = true;
            gun.muzzleFlashEffects = new VFXPool() { type = VFXPoolType.None, effects = new VFXComplex[0] };


            GameObject SlashEffect_Default_Left = PrefabBuilder.BuildObject("(Ralsei)SlashEffect_Default(Left)");
            Alexandria.ItemAPI.FakePrefab.MarkAsFakePrefab(SlashEffect_Default_Left);
            UnityEngine.Object.DontDestroyOnLoad(SlashEffect_Default_Left);
            var sprite_SlashEffect_Default_Left = SlashEffect_Default_Left.AddComponent<tk2dSprite>();
            sprite_SlashEffect_Default_Left.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_slash_default1_002");
            sprite_SlashEffect_Default_Left.IsPerpendicular = false;
            tk2dSpriteAnimator animator = SlashEffect_Default_Left.GetOrAddComponent<tk2dSpriteAnimator>();
            animator.library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.Library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.DefaultClipId = animator.GetClipIdByName("scarf_slash_0");
            animator.playAutomatically = true;
            var kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.animator = animator;

            animator.sprite.usesOverrideMaterial = true;
            var mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = animator.sprite.renderer.material.mainTexture;
            animator.sprite.renderer.material = mat;


            GameObject SlashEffect_Default_Right = PrefabBuilder.BuildObject("(Ralsei)SlashEffect_Default(Right)");
            Alexandria.ItemAPI.FakePrefab.MarkAsFakePrefab(SlashEffect_Default_Right);
            UnityEngine.Object.DontDestroyOnLoad(SlashEffect_Default_Right);
            var sprite_SlashEffect_Default_Right = SlashEffect_Default_Right.AddComponent<tk2dSprite>();
            sprite_SlashEffect_Default_Right.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_slash_default1_002");
            sprite_SlashEffect_Default_Right.IsPerpendicular = false;

            animator = SlashEffect_Default_Right.GetOrAddComponent<tk2dSpriteAnimator>();
            animator.library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.Library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.DefaultClipId = animator.GetClipIdByName("scarf_slash_1");
            animator.playAutomatically = true;
            kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.animator = animator;
            animator.sprite.usesOverrideMaterial = true;
            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = animator.sprite.renderer.material.mainTexture;
            animator.sprite.renderer.material = mat;

            GameObject SlashEffect_Whip = PrefabBuilder.BuildObject("(Ralsei)SlashEffect_Whip");
            Alexandria.ItemAPI.FakePrefab.MarkAsFakePrefab(SlashEffect_Whip);
            UnityEngine.Object.DontDestroyOnLoad(SlashEffect_Whip);
            var sprite_Whip= SlashEffect_Whip.AddComponent<tk2dSprite>();
            sprite_Whip.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_slash_default1_002");
            sprite_Whip.IsPerpendicular = false;

            animator = SlashEffect_Whip.GetOrAddComponent<tk2dSpriteAnimator>();
            animator.library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.Library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.DefaultClipId = animator.GetClipIdByName("scarf_slash_2");
            animator.playAutomatically = true;
            kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.animator = animator;
            animator.sprite.usesOverrideMaterial = true;
            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = animator.sprite.renderer.material.mainTexture;
            animator.sprite.renderer.material = mat;

            GameObject SlashEffect_Whirl = PrefabBuilder.BuildObject("(Ralsei)SlashEffect_Whirl");
            Alexandria.ItemAPI.FakePrefab.MarkAsFakePrefab(SlashEffect_Whirl);
            UnityEngine.Object.DontDestroyOnLoad(SlashEffect_Whirl);
            var sprite_Whirl = SlashEffect_Whirl.AddComponent<tk2dSprite>();
            sprite_Whirl.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_slash_default1_002");
            sprite_Whirl.IsPerpendicular = false;

            animator = SlashEffect_Whirl.GetOrAddComponent<tk2dSpriteAnimator>();
            animator.library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.Library = StaticSpriteCollections.ItemAndGunAnimation;
            animator.DefaultClipId = animator.GetClipIdByName("scarf_slash_3");
            animator.playAutomatically = true;
            kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.animator = animator;
            animator.sprite.usesOverrideMaterial = true;
            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = animator.sprite.renderer.material.mainTexture;
            animator.sprite.renderer.material = mat;

            var slashDefault = ScriptableObject.CreateInstance<RalseiSlash>();
            slashDefault.projInteractMode = CustomSlashDoer.ProjInteractMode.IGNORE;
            slashDefault.playerKnockbackForce = 1;
            slashDefault.enemyKnockbackForce = 20;
            slashDefault.VFX = SlashEffect_Default_Left.CreateQuickVFXPool();
            slashDefault.doVFX = true;

            slashDefault.doHitVFX = true;
            slashDefault.slashRange = 4.5f;
            slashDefault.slashDegrees = 72;
            slashDefault.soundEvent = "Play_ENM_gunnut_swing_01";
            slashDefault.damage = 2.7f;
            slashDefault.damagesBreakables = true;
            slashDefault.hitVFX = (PickupObjectDatabase.GetById(539) as Gun).DefaultModule.chargeProjectiles[0].Projectile.hitEffects.enemy;


            var slashDefault1 = ScriptableObject.CreateInstance<BasicSlash2>();
            slashDefault1.projInteractMode = CustomSlashDoer.ProjInteractMode.IGNORE;
            slashDefault1.playerKnockbackForce = 1;
            slashDefault1.enemyKnockbackForce = 20;
            slashDefault1.doVFX = true;
            slashDefault1.VFX = SlashEffect_Default_Right.CreateQuickVFXPool();
            slashDefault1.doHitVFX = true;
            slashDefault1.slashRange = 4.5f;
            slashDefault1.slashDegrees = 72;
            slashDefault1.soundEvent = "Play_ENM_gunnut_swing_01";
            slashDefault1.damage = 2.7f;
            slashDefault1.damagesBreakables = true;
            slashDefault1.hitVFX = (PickupObjectDatabase.GetById(539) as Gun).DefaultModule.chargeProjectiles[0].Projectile.hitEffects.enemy;

            var slashWhip = ScriptableObject.CreateInstance<StunnableSlash>();
            slashWhip.projInteractMode = CustomSlashDoer.ProjInteractMode.DESTROY;
            slashWhip.playerKnockbackForce = 1;
            slashWhip.enemyKnockbackForce = 40;
            slashWhip.VFX = SlashEffect_Whip.CreateQuickVFXPool();
            slashWhip.doVFX = true;
            slashWhip.doHitVFX = true;
            slashWhip.slashRange = 6.5f;
            slashWhip.slashDegrees = 15;
            slashWhip.soundEvent = "Play_ENM_wizardred_swing_01";
            slashWhip.damage = 11;
            slashWhip.damagesBreakables = true;
            slashWhip.hitVFX = (PickupObjectDatabase.GetById(539) as Gun).DefaultModule.chargeProjectiles[0].Projectile.hitEffects.enemy;

            var slashWhirlwind = ScriptableObject.CreateInstance<Whrilwind>();
            slashWhirlwind.projInteractMode = CustomSlashDoer.ProjInteractMode.DESTROY;
            slashWhirlwind.playerKnockbackForce = 50;
            slashWhirlwind.enemyKnockbackForce = 40;
            slashWhirlwind.VFX = SlashEffect_Whirl.CreateQuickVFXPool();
            slashWhirlwind.doVFX = true;
            slashWhirlwind.doHitVFX = true;
            slashWhirlwind.slashRange = 5f;
            slashWhirlwind.slashDegrees = 360;

            slashWhirlwind.soundEvent = "Play_obj_katana_slash_01";
            slashWhirlwind.damage = 2;
            slashWhirlwind.damagesBreakables = true;
            slashWhirlwind.hitVFX = (PickupObjectDatabase.GetById(539) as Gun).DefaultModule.chargeProjectiles[0].Projectile.hitEffects.enemy;
            


            SlashLeft = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            SlashLeft.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(SlashLeft.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(SlashLeft);
            PierceProjModifier spook = SlashLeft.gameObject.AddComponent<PierceProjModifier>();
            spook.penetratesBreakables = true;
            SlashLeft.baseData.damage = 1.8f;
            SlashLeft.baseData.speed = 12.5f;
            SlashLeft.baseData.AccelerationCurve = AnimationCurve.Linear(0, 1, 1, 0.2f);
            SlashLeft.baseData.CustomAccelerationCurveDuration = 1;
            SlashLeft.baseData.UsesCustomAccelerationCurve = true;
            SlashLeft.shouldRotate = true;
            SlashLeft.pierceMinorBreakables = true;
            var slashProjectile = SlashLeft.gameObject.AddComponent<CustomProjectileSlashingBehaviour>();
            slashProjectile.DestroyBaseAfterFirstSlash = true;
            slashProjectile.slashParameters = slashDefault;
            slashProjectile.DestroysOnlyComponentAfterFirstSlash = true;
            slashProjectile.initialDelay = 0;
            slashProjectile.SlashDamageUsesBaseProjectileDamage = false;

            SlashLeft.hitEffects = (PickupObjectDatabase.GetById(345) as Gun).DefaultModule.projectiles[0].hitEffects;

            int amount = 6;
            Alexandria.Assetbundle.ProjectileBuilders.AnimateProjectileBundle(SlashLeft, "slash_normal", StaticSpriteCollections.ProjectileCollection, StaticSpriteCollections.ProjectileAnimation, "slash_normal",
            Toolbox.ConstructListOfSameValues<IntVector2>(new IntVector2(15, 46), amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, amount),
            Toolbox.ConstructListOfSameValues(true, amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues<Vector3?>(new Vector2(0, 0), amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<Projectile>(null, amount));
            gun.Volley.projectiles[0].projectiles[0] = SlashLeft;
            gun.DefaultModule.projectiles[0] = SlashLeft;
            ImprovedAfterImage image = SlashLeft.gameObject.AddComponent<ImprovedAfterImage>();
            image.spawnShadows = true;
            image.shadowLifetime = 0.25f;
            image.shadowTimeDelay = 0.05f;
            image.dashColor = new Color(0, 0.5f, 0.1f, 0.02f);

            SlashRight = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            SlashRight.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(SlashRight.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(SlashRight);
            spook = SlashRight.gameObject.AddComponent<PierceProjModifier>();
            spook.penetratesBreakables = true;
            SlashRight.baseData.damage = 1.8f;
            SlashRight.baseData.speed = 12.5f;
            SlashRight.baseData.AccelerationCurve = AnimationCurve.Linear(0, 1, 1, 0.2f);
            SlashRight.baseData.CustomAccelerationCurveDuration = 1;
            SlashRight.baseData.UsesCustomAccelerationCurve = true;
            SlashRight.shouldRotate = true;
            SlashRight.pierceMinorBreakables = true;
            SlashRight.hitEffects = (PickupObjectDatabase.GetById(345) as Gun).DefaultModule.projectiles[0].hitEffects;

            slashProjectile = SlashRight.gameObject.AddComponent<CustomProjectileSlashingBehaviour>();
            slashProjectile.DestroyBaseAfterFirstSlash = true;
            slashProjectile.slashParameters = slashDefault1;
            slashProjectile.DestroysOnlyComponentAfterFirstSlash = true;
            slashProjectile.initialDelay = 0;
            slashProjectile.SlashDamageUsesBaseProjectileDamage = false;

            amount = 6;
            Alexandria.Assetbundle.ProjectileBuilders.AnimateProjectileBundle(SlashRight, "slash_normal", StaticSpriteCollections.ProjectileCollection, StaticSpriteCollections.ProjectileAnimation, "slash_normal",
            Toolbox.ConstructListOfSameValues<IntVector2>(new IntVector2(15, 46), amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, amount),
            Toolbox.ConstructListOfSameValues(true, amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues<Vector3?>(new Vector2(0, 0), amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<Projectile>(null, amount));
            image = SlashRight.gameObject.AddComponent<ImprovedAfterImage>();
            image.spawnShadows = true;
            image.shadowLifetime = 0.25f;
            image.shadowTimeDelay = 0.05f;
            image.dashColor = new Color(0, 0.5f, 0.1f, 0.02f);



            SlashWhip = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            SlashWhip.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(SlashWhip.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(SlashWhip);
            spook = SlashWhip.gameObject.AddComponent<PierceProjModifier>();
            spook.penetratesBreakables = true;
            SlashWhip.baseData.damage = 10f;
            SlashWhip.baseData.speed = 22f;
            SlashWhip.baseData.AccelerationCurve = AnimationCurve.Linear(0, 1, 1, 0.2f);
            SlashWhip.baseData.CustomAccelerationCurveDuration = 1;
            SlashWhip.baseData.UsesCustomAccelerationCurve = true;
            SlashWhip.shouldRotate = true;
            SlashWhip.pierceMinorBreakables = true;
            SlashWhip.hitEffects = (PickupObjectDatabase.GetById(345) as Gun).DefaultModule.projectiles[0].hitEffects;

            slashProjectile = SlashWhip.gameObject.AddComponent<CustomProjectileSlashingBehaviour>();
            slashProjectile.DestroyBaseAfterFirstSlash = true;
            slashProjectile.slashParameters = slashWhip;
            slashProjectile.DestroysOnlyComponentAfterFirstSlash = true;
            slashProjectile.initialDelay = 0;
            slashProjectile.SlashDamageUsesBaseProjectileDamage = false;

            amount = 5;
            Alexandria.Assetbundle.ProjectileBuilders.AnimateProjectileBundle(SlashWhip, "slash_whip", StaticSpriteCollections.ProjectileCollection, StaticSpriteCollections.ProjectileAnimation, "slash_whip",
            Toolbox.ConstructListOfSameValues<IntVector2>(new IntVector2(30, 11), amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, amount),
            Toolbox.ConstructListOfSameValues(true, amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues<Vector3?>(new Vector2(0, 0), amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<Projectile>(null, amount));
            gun.Volley.projectiles[1].projectiles[0] = SlashWhip;
            image = SlashWhip.gameObject.AddComponent<ImprovedAfterImage>();
            image.spawnShadows = true;
            image.shadowLifetime = 0.25f;
            image.shadowTimeDelay = 0.05f;
            image.dashColor = new Color(0, 0.5f, 0.1f, 0.02f);

            SlashSmall = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            SlashSmall.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(SlashSmall.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(SlashSmall);
            spook = SlashSmall.gameObject.AddComponent<PierceProjModifier>();
            spook.penetratesBreakables = true;
            SlashSmall.baseData.damage = 1.8f;
            SlashSmall.baseData.speed = 18f;
            SlashSmall.baseData.AccelerationCurve = AnimationCurve.Linear(0, 1, 1, 0.2f);
            SlashSmall.baseData.CustomAccelerationCurveDuration = 1;
            SlashSmall.baseData.UsesCustomAccelerationCurve = true;
            SlashSmall.shouldRotate = true;
            SlashSmall.pierceMinorBreakables = true;
            SlashSmall.hitEffects = (PickupObjectDatabase.GetById(345) as Gun).DefaultModule.projectiles[0].hitEffects;

            slashProjectile = SlashSmall.gameObject.AddComponent<CustomProjectileSlashingBehaviour>();
            slashProjectile.DestroyBaseAfterFirstSlash = true;
            slashProjectile.slashParameters = slashWhirlwind;
            slashProjectile.DestroysOnlyComponentAfterFirstSlash = true;
            slashProjectile.initialDelay = 0;
            slashProjectile.SlashDamageUsesBaseProjectileDamage = false;

            gun.Volley.projectiles[2].projectiles[0] = SlashSmall;
            image = SlashSmall.gameObject.AddComponent<ImprovedAfterImage>();
            image.spawnShadows = true;
            image.shadowLifetime = 0.25f;
            image.shadowTimeDelay = 0.05f;
            image.dashColor = new Color(0, 0.5f, 0.1f, 0.02f);


            amount = 4;
            Alexandria.Assetbundle.ProjectileBuilders.AnimateProjectileBundle(SlashSmall, "slash_small", StaticSpriteCollections.ProjectileCollection, StaticSpriteCollections.ProjectileAnimation, "slash_small",
            Toolbox.ConstructListOfSameValues<IntVector2>(new IntVector2(15, 24), amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, amount),
            Toolbox.ConstructListOfSameValues(true, amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues<Vector3?>(new Vector2(0, 0), amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<Projectile>(null, amount));



            SlashSmallSlashless = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            SlashSmallSlashless.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(SlashSmallSlashless.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(SlashSmallSlashless);
            spook = SlashSmall.gameObject.AddComponent<PierceProjModifier>();
            spook.penetratesBreakables = true;
            SlashSmallSlashless.baseData.damage = 1.8f;
            SlashSmallSlashless.baseData.speed = 18f;
            SlashSmallSlashless.baseData.AccelerationCurve = AnimationCurve.Linear(0, 1, 1, 0.2f);
            SlashSmallSlashless.baseData.CustomAccelerationCurveDuration = 1;
            SlashSmallSlashless.baseData.UsesCustomAccelerationCurve = true;
            SlashSmallSlashless.shouldRotate = true;
            SlashSmallSlashless.pierceMinorBreakables = true;
            SlashSmallSlashless.hitEffects = (PickupObjectDatabase.GetById(345) as Gun).DefaultModule.projectiles[0].hitEffects;

            image = SlashSmallSlashless.gameObject.AddComponent<ImprovedAfterImage>();
            image.spawnShadows = true;
            image.shadowLifetime = 0.25f;
            image.shadowTimeDelay = 0.05f;
            image.dashColor = new Color(0, 0.5f, 0.1f, 0.02f);


            amount = 4;
            Alexandria.Assetbundle.ProjectileBuilders.AnimateProjectileBundle(SlashSmallSlashless, "slash_small", StaticSpriteCollections.ProjectileCollection, StaticSpriteCollections.ProjectileAnimation, "slash_small",
            Toolbox.ConstructListOfSameValues<IntVector2>(new IntVector2(15, 24), amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, amount),
            Toolbox.ConstructListOfSameValues(true, amount),
            Toolbox.ConstructListOfSameValues(false, amount),
            Toolbox.ConstructListOfSameValues<Vector3?>(new Vector2(0, 0), amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<IntVector2?>(null, amount),
            Toolbox.ConstructListOfSameValues<Projectile>(null, amount));


            gun.gunClass = GunClass.SHITTY;
            gun.gunHandedness = GunHandedness.NoHanded;
            gun.carryPixelOffset = new IntVector2(0, 0);
            gun.barrelOffset.localPosition = new Vector3(0, 0, 0);
            

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            RalseiScarfID = gun.PickupObjectId;

            scarfRef = (PickupObjectDatabase.GetById(436) as BlinkPassiveItem).ScarfPrefab;

            var obj = PrefabBuilder.BuildObject("SetTargetRalsei");
            var effect = obj.AddComponent<RalseiTargetVFX>();
            DontDestroyOnLoad(obj);
            obj.layer = LayerMask.NameToLayer("FG_Critical");
            var spriteBar = obj.AddComponent<tk2dSprite>();
            spriteBar.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "priorityTarget_text_001");
            spriteBar.usesOverrideMaterial = true;
            spriteBar.HeightOffGround = -200;
            spriteBar.IsPerpendicular = false;
            spriteBar.SortingOrder = 200;
            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = spriteBar.renderer.material.mainTexture;
            spriteBar.renderer.material = mat;


            var bracket = PrefabBuilder.BuildObject("SetTargetRalsei Bracket");
            bracket.layer = LayerMask.NameToLayer("FG_Critical");

            spriteBar = bracket.AddComponent<tk2dSprite>();
            spriteBar.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "priorityTarget_bracket_001");
            spriteBar.usesOverrideMaterial = true;
            spriteBar.HeightOffGround = -200;
            spriteBar.SortingOrder = 200;
            spriteBar.IsPerpendicular = false;

            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = spriteBar.renderer.material.mainTexture;
            spriteBar.renderer.material = mat;

            var spriteanimator = bracket.AddComponent<tk2dSpriteAnimator>();
            spriteanimator.library = StaticSpriteCollections.ItemAndGunAnimation;
            spriteanimator.Library = StaticSpriteCollections.ItemAndGunAnimation;
            spriteanimator.playAutomatically = true;
            spriteanimator.DefaultClipId = StaticSpriteCollections.ItemAndGunAnimation.GetClipIdByName("targetClip");

            effect.BracketPrefab = bracket;
            EffectPrefab = effect;




        }
        public static Projectile SlashLeft;
        public static Projectile SlashRight;
        public static Projectile SlashWhip;
        public static Projectile SlashSmall;
        public static Projectile SlashSmallSlashless;
        public static RalseiTargetVFX EffectPrefab;
        public static ScarfAttachmentDoer scarfRef;

        private CustomScarfDoer scarfInstance;
        private int SlashValue = 0;

        public float WhipTimer = 0;
        public float ChargeForWhirl = 0;
        public float Reset = 0;

        public override void OnCreation(Gun gun)
        {
            gun.OnPreFireProjectileModifier += DetermineSlashType;
            gun.OnPostFired += Finish;
        }
        public override void OnPlayerPickup(PlayerController player)
        {
            base.OnPlayerPickup(player);
            scarfInstance = UnityEngine.Object.Instantiate<GameObject>(scarfRef.gameObject).AddComponent<CustomScarfDoer>();
            scarfInstance.AttachTarget = player;
            scarfInstance.ScarfMaterial = new Material(scarfRef.ScarfMaterial);
            scarfInstance.StartWidth = 0.075f;
            scarfInstance.EndWidth = 0.1f;
            scarfInstance.AnimationSpeed = 20f;
            scarfInstance.ScarfLength = 0.4f;
            scarfInstance.AngleLerpSpeed = 4;
            scarfInstance.BackwardZOffset = -0.75f;
            scarfInstance.CatchUpScale = 0.99f;
            scarfInstance.SinSpeed = 9f;
            scarfInstance.AmplitudeMod = 0.135f;
            scarfInstance.WavelengthMod = 1.1f;
            scarfInstance.ScarfMaterial.SetColor("_OverrideColor", new Color(0.42f, 0.1f, 0.42f));
            scarfInstance.Initialize(player);
            scarfInstance.AdditionalOffset = new Vector3(0, 0.0625f);

            player.OnPreDodgeRoll += Player_OnPreDodgeRoll;
        }



        public void Finish(PlayerController player, Gun gun)
        {
            if (SlashValue == 3)
            {
                for (int i = 1; i < 16; i++)
                {
                    GameObject gameObject = SpawnManager.SpawnProjectile(SlashSmallSlashless.gameObject, gun.barrelOffset.transform.position, Quaternion.Euler(0f, 0f, gun.CurrentAngle + (22.5f * i)), true);
                    Projectile component = gameObject.GetComponent<Projectile>();
                    if (component != null)
                    {
                        component.SpawnedFromOtherPlayerProjectile = true;
                        component.Shooter = this.gun.CurrentOwner.specRigidbody;
                        component.Owner = this.gun.CurrentOwner;
                    }
                }
                ChargeForWhirl = 0;
                UpdateSlashType(0);
            }
            else
            {
                UpdateSlashType(SlashValue == 0 ? 1 : 0, true);
            }
        }

        public IEnumerator ResetValues(int Value)
        {
            yield return null;
            UpdateSlashType(Value, true);
            yield break;
        }



        public void UpdateSlashType(int Value, bool isFiring = false)
        {
            SlashValue = Value;
            switch (SlashValue)
            {
                case 0:            
                    if (gun.CurrentStrengthTier == 0) { return; }
                    gun.CurrentStrengthTier = 0;
                    break;
                case 1:
                    if (gun.CurrentStrengthTier == 0) { return; }
                    gun.CurrentStrengthTier = 0;
                    break;
                case 2:
                    gun.CurrentStrengthTier = 1;
                    break;
                case 3:
                    gun.CurrentStrengthTier = 2;
                    break;
            }
        }

        private IEnumerator HandleModuleCooldown(ProjectileModule mod)
        {
            gun.m_moduleData[mod].onCooldown = true;
            float elapsed = 0f;
            float fireMultiplier = (!(gun.m_owner is PlayerController)) ? 1f : (gun.m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.RateOfFire);
            float cooldownTime = 0.5f * fireMultiplier;
            while (elapsed < cooldownTime)
            {
                elapsed += BraveTime.DeltaTime;
                yield return null;
            }         
            if (gun.m_moduleData != null && gun.m_moduleData.ContainsKey(mod))
            {
                gun.m_moduleData[mod].onCooldown = false;
                gun.m_moduleData[mod].chargeTime = 0f;
                gun.m_moduleData[mod].chargeFired = false;
            }     
            yield break;
        }

        public Projectile DetermineSlashType(Gun gun, Projectile projectile, ProjectileModule projectileModule)
        {
            gun.m_isCurrentlyFiring = false;
            //gun.StartCoroutine(ResetValues(SlashValue == 0 ? 1 : 0));
            if (SlashValue == 0) 
            {
                this.gun.Volley.projectiles[0].cooldownTime = 0.25f;
                //UpdateSlashType(SlashValue == 0 ? 1 : 0, true);
                return SlashLeft;
            }
            if (SlashValue == 1) 
            {
                //Reset = 0;
                this.gun.Volley.projectiles[0].cooldownTime = 0.8f;
                //UpdateSlashType(SlashValue == 0 ? 1 : 0, true);
                return SlashRight;
            }
            if (SlashValue == 2 | SlashValue == 3)
            {
                gun.ClearBurstState();
                gun.StartCoroutine(HandleModuleCooldown(gun.Volley.projectiles[0]));
            }
            //UpdateSlashType(SlashValue == 0 ? 1 : 0, true);

            return projectile;
        }


        private void Player_OnPreDodgeRoll(PlayerController obj)
        {
            if (SlashValue == 3) { return; }
            WhipTimer = 1.25f;
            gun.ClearCooldowns();
            UpdateSlashType(2);
        }



        public override void Update()
        {
            base.Update();
            if (gun.CurrentOwner != null)
            {
                gun.PreventNormalFireAudio = true;
                if (gun.CurrentOwner is PlayerController player)
                {
                    if (player.CurrentGun != null && player.CurrentGun == gun)
                    {
                        if (player.m_activeActions != null && player.m_activeActions.ReloadAction != null)
                        {
                            bool wasPressed = player.m_activeActions.ReloadAction.State;
                            if (wasPressed && gun.IsFiring == false)
                            {
                                ChargeForWhirl += Time.deltaTime * 1.5f;
                            }
                            else if (ChargeForWhirl < 1)
                            {
                                ChargeForWhirl = 0;
                            }
                        }
                    }
                    if (ChargeForWhirl > 1 && SlashValue != 3)
                    {
                        UpdateSlashType(3);
                        gun.DoChargeCompletePoof();
                    }
                    else if (SlashValue == 1)
                    {
                        if (gun.IsFiring == false)
                        {
                            Reset += Time.deltaTime;
                            if (Reset > 0.5f)
                            {
                                UpdateSlashType(0);
                                Reset = 0;
                            }
                        }
                    }
                    if (ChargeForWhirl > 0)
                    {
                        GlobalSparksDoer.DoRandomParticleBurst(1, player.sprite.WorldCenter, player.sprite.WorldCenter,
                            Toolbox.GetUnitOnCircle(BraveUtility.RandomAngle(), (2.5f * Mathf.Min(1, ChargeForWhirl)) + 3),
                            1f, 0.05f, 0.05f, 0.5f, Color.green * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                    }

                    if (WhipTimer > 0)
                    {
                        WhipTimer -= Time.deltaTime;
                    }
                    else if (SlashValue == 2)
                    {
                        UpdateSlashType(0);
                    }
                    if (scarfInstance)
                    {
                        scarfInstance.m_mr.enabled = !gun.m_isCurrentlyFiring;
                    }
                }
            }
        }
        public override void OnDroppedByPlayer(PlayerController player)
        {
            if (this.scarfInstance)
            {
                UnityEngine.Object.Destroy(this.scarfInstance.gameObject);
                this.scarfInstance = null;
            }
            player.OnPreDodgeRoll -= Player_OnPreDodgeRoll;
            base.OnDroppedByPlayer(player);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (gun.CurrentOwner && gun.CurrentOwner is PlayerController player)
            {
                player.OnPreDodgeRoll -= Player_OnPreDodgeRoll;
            }
            if (this.scarfInstance)
            {
                UnityEngine.Object.Destroy(this.scarfInstance.gameObject);
                this.scarfInstance = null;
            }
        }
        public static int RalseiScarfID;


        /// <summary>Completely hides a gun's ammo like blasphemy</summary>
        /// Thank you pretzel!
        
        [HarmonyPatch]
        private static class GameUIAmmoControllerUpdateUIGunPatch
        {
            /// <summary>Actually hide the gun's ammo</summary>
            [HarmonyPatch(typeof(GameUIAmmoController), nameof(GameUIAmmoController.UpdateUIGun))]
            [HarmonyILManipulator]
            private static void GameUIAmmoControllerUpdateUIGunIL(ILContext il)
            {
                ILCursor cursor = new ILCursor(il);
                if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Gun>("IsHeroSword")))
                    return;

                cursor.Emit(OpCodes.Ldloc_0);
                cursor.Emit(OpCodes.Call, typeof(GameUIAmmoControllerUpdateUIGunPatch).GetMethod(nameof(CheckHideAmmo), BindingFlags.Static | BindingFlags.NonPublic));
            }

            private static bool CheckHideAmmo(bool oldValue, Gun gun)
            {
                if (oldValue)
                    return true;
                if (gun.PickupObjectId == RalseiScarfID)
                    return true;
                return false;
            }
        }
        
        public class StunnableSlash : RalseiSlash
        {
            public override CustomSlashData ReturnClone()
            {

                StunnableSlash newData = ScriptableObject.CreateInstance<StunnableSlash>();
                newData.doVFX = this.doVFX;
                newData.VFX = this.VFX;
                newData.doHitVFX = this.doHitVFX;
                newData.hitVFX = this.hitVFX;
                newData.projInteractMode = this.projInteractMode;
                newData.playerKnockbackForce = this.playerKnockbackForce;
                newData.enemyKnockbackForce = this.enemyKnockbackForce;
                newData.statusEffects = this.statusEffects;
                newData.jammedDamageMult = this.jammedDamageMult;
                newData.bossDamageMult = this.bossDamageMult;
                newData.doOnSlash = this.doOnSlash;
                newData.doPostProcessSlash = this.doPostProcessSlash;
                newData.slashRange = this.slashRange;
                newData.slashDegrees = this.slashDegrees;
                newData.damage = this.damage;
                newData.damagesBreakables = this.damagesBreakables;
                newData.soundEvent = this.soundEvent;
                newData.OnHitTarget = this.OnHitTarget;
                newData.OnHitBullet = this.OnHitBullet;
                newData.OnHitMinorBreakable = this.OnHitMinorBreakable;
                newData.OnHitMajorBreakable = this.OnHitMajorBreakable;
                return newData;
            }
            public override bool isStunnable
            {
                get
                {
                    return true;
                }
            }
            public override bool isCharmable
            {
                get
                {
                    return false;
                }
            }
            public override bool isEnemyTag
            {
                get
                {
                    return true;
                }
            }

        }
        public class BasicSlash2 : RalseiSlash
        {
            public override CustomSlashData ReturnClone()
            {

                BasicSlash2 newData = ScriptableObject.CreateInstance<BasicSlash2>();
                newData.doVFX = this.doVFX;
                newData.VFX = this.VFX;
                newData.doHitVFX = this.doHitVFX;
                newData.hitVFX = this.hitVFX;
                newData.projInteractMode = this.projInteractMode;
                newData.playerKnockbackForce = this.playerKnockbackForce;
                newData.enemyKnockbackForce = this.enemyKnockbackForce;
                newData.statusEffects = this.statusEffects;
                newData.jammedDamageMult = this.jammedDamageMult;
                newData.bossDamageMult = this.bossDamageMult;
                newData.doOnSlash = this.doOnSlash;
                newData.doPostProcessSlash = this.doPostProcessSlash;
                newData.slashRange = this.slashRange;
                newData.slashDegrees = this.slashDegrees;
                newData.damage = this.damage;
                newData.damagesBreakables = this.damagesBreakables;
                newData.soundEvent = this.soundEvent;
                newData.OnHitTarget = this.OnHitTarget;
                newData.OnHitBullet = this.OnHitBullet;
                newData.OnHitMinorBreakable = this.OnHitMinorBreakable;
                newData.OnHitMajorBreakable = this.OnHitMajorBreakable;
                return newData;
            }
            public override bool isCharmable
            {
                get
                {
                    return true;
                }
            }
        }
        public class Whrilwind : RalseiSlash
        {
            public override CustomSlashData ReturnClone()
            {

                Whrilwind newData = ScriptableObject.CreateInstance<Whrilwind>();
                newData.doVFX = this.doVFX;
                newData.VFX = this.VFX;
                newData.doHitVFX = this.doHitVFX;
                newData.hitVFX = this.hitVFX;
                newData.projInteractMode = this.projInteractMode;
                newData.playerKnockbackForce = this.playerKnockbackForce;
                newData.enemyKnockbackForce = this.enemyKnockbackForce;
                newData.statusEffects = this.statusEffects;
                newData.jammedDamageMult = this.jammedDamageMult;
                newData.bossDamageMult = this.bossDamageMult;
                newData.doOnSlash = this.doOnSlash;
                newData.doPostProcessSlash = this.doPostProcessSlash;
                newData.slashRange = this.slashRange;
                newData.slashDegrees = this.slashDegrees;
                newData.damage = this.damage;
                newData.damagesBreakables = this.damagesBreakables;
                newData.soundEvent = this.soundEvent;
                newData.OnHitTarget = this.OnHitTarget;
                newData.OnHitBullet = this.OnHitBullet;
                newData.OnHitMinorBreakable = this.OnHitMinorBreakable;
                newData.OnHitMajorBreakable = this.OnHitMajorBreakable;

                return newData;
            }
            public override bool isStunnable
            {
                get
                {
                    return true;
                }
            }
            public override bool isCharmable 
            {
                get
                {
                    return true;
                }
            }
            public override int Stacks
            {
                get
                {
                    return 2;
                }
            }
        }
        public class RalseiSlash : CustomSlashData
        {
            public virtual bool isCharmable
            {
                get
                {
                    return true;
                }
            }
            public virtual bool isStunnable
            {
                get
                {
                    return false;
                }
            }
            public virtual int Stacks
            {
                get
                {
                    return 1;
                }
            }
            public virtual bool isEnemyTag
            {
                get
                {
                    return false;
                }
            }


            public override CustomSlashData ReturnClone()
            {

                RalseiSlash newData = ScriptableObject.CreateInstance<RalseiSlash>();
                newData.doVFX = this.doVFX;
                newData.VFX = this.VFX;
                newData.doHitVFX = this.doHitVFX;
                newData.hitVFX = this.hitVFX;
                newData.projInteractMode = this.projInteractMode;
                newData.playerKnockbackForce = this.playerKnockbackForce;
                newData.enemyKnockbackForce = this.enemyKnockbackForce;
                newData.statusEffects = this.statusEffects;
                newData.jammedDamageMult = this.jammedDamageMult;
                newData.bossDamageMult = this.bossDamageMult;
                newData.doOnSlash = this.doOnSlash;
                newData.doPostProcessSlash = this.doPostProcessSlash;
                newData.slashRange = this.slashRange;
                newData.slashDegrees = this.slashDegrees;
                newData.damage = this.damage;
                newData.damagesBreakables = this.damagesBreakables;
                newData.soundEvent = this.soundEvent;
                newData.OnHitTarget = this.OnHitTarget;
                newData.OnHitBullet = this.OnHitBullet;
                newData.OnHitMinorBreakable = this.OnHitMinorBreakable;
                newData.OnHitMajorBreakable = this.OnHitMajorBreakable;
                return newData;
            }
            public static RalseiTargetVFX targetVFX;
            public static float Cooldown;
            public override void OnHitEnemy(GameActor gameActor, bool b, float Angle, Vector2 arcOrigin, Vector2 contact)
            {
                if (gameActor is AIActor myAiActor && b == false)
                {
                    if (myAiActor && myAiActor.IsNormalEnemy && myAiActor.healthHaver)
                    {
                        if (!myAiActor.healthHaver.IsBoss && myAiActor.behaviorSpeculator)
                        {
                            if (isStunnable)
                            {
                                float Dist = Vector2.Distance(contact, arcOrigin);
                                if (Dist >= slashRange * 0.8f && Dist <= slashRange)
                                {
                                    myAiActor.behaviorSpeculator.Stun(
                                        Mathf.Min(5,
                                        Mathf.Sqrt(damage)));
                                }
                            }
                            if (isCharmable)
                            {
                                for (int i = 0; i < Stacks; i++)
                                {
                                    myAiActor.ApplyEffect(new RalseiCharmable()
                                    {
                                        duration = 10000,
                                    });
                                }
                            }
                        }
                        if (AIActorModifiers.RalseiCharmedEnemyController.allCharmed.Count > 0)
                        {
                            if (isEnemyTag && Cooldown <= 0)
                            {
                                if (targetVFX != null)
                                {
                                    Destroy(targetVFX.gameObject);
                                    targetVFX = null;
                                }
                                Cooldown = 1;
                                targetVFX = myAiActor.SmartPlayEffectOnActor(EffectPrefab.gameObject, new Vector3(0, 0), true, true, false, false).GetComponent<RalseiTargetVFX>();
                                targetVFX.TargetInst = myAiActor;

                            }
                        }
                    }
                }
            }
        }
    }
}

