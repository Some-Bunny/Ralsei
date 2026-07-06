using BepInEx;
using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Ralsei.Code;
using Alexandria.CharacterAPI;
using MikuMikuMod;
using Planetside;
using HarmonyLib;
using Brave.BulletScript;

namespace Ralsei
{
    [BepInDependency(Alexandria.Alexandria.GUID)] 
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class RalseiModule : BaseUnityPlugin
    {
        public const string GUID = "somebunny.etg.ralsei";
        public const string NAME = "== Ralsei! ==";
        public const string VERSION = "1.0.4";
        public const string TEXT_COLOR = "#6aff13";

        public static AssetBundle assetBundle;
        public static PlayableCharacters Ralsei;

        public void Start()
        {
            new Harmony(GUID).PatchAll();
            var FilePathFolder = this.FolderPath();
            assetBundle = AssetBundleLoader.LoadAssetBundleFromLiterallyAnywhere(FilePathFolder, "ralseibundle");
            AudioResourceLoader.loadFromAssembly("RalseiBank.bnk", "RalseiMod");


            

            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            StaticSpriteCollections.Init();
            ParticleBase.InitParticleBase();
            ScarfWeapon.Add();
            Cake.Init();
            RalseiCharmVFX.InitializeEffect();

            Ralsei = ETGModCompatibility.ExtendEnum<PlayableCharacters>(RalseiModule.GUID, "Ralsei");


            var data = Loader.BuildCharacterBundle(
            "Ralsei/RalseiData",
            StaticSpriteCollections.RalseiCharacterCollection,
            StaticSpriteCollections.RalseiCharacterAnimation,
            StaticSpriteCollections.RalseiCharacterCollection,
            StaticSpriteCollections.RalseiCharacterAnimation,
            "somebunny.etg.ralsei",
            new Vector3(27.4f, 24.5f),
            false,
            new Vector3(27.4f, 24.5f),
            true,
            false,
            false, //armor only
            true, //Sprites used by paradox
            false, //Glows
            new GlowMatDoer(new Color32(0, 0, 0, 255), 0, 0), //Glow Mat
            new GlowMatDoer(new Color32(0, 0, 0, 255), 0, 0), //Alt Skin Glow Mat
            0, //Hegemony Cost
            false, //HasPast
            "",
            assetBundle.LoadAsset<Texture2D>("ralseibosscard_001"));
            data.animator.defaultClipId = 27;

            var doer = data.idleDoer;
            doer.idleMax = 20;
            doer.idleMin = 4;
            List<int> Offsets = new List<int>();
            data.animator.GetClipByName("chest_recover").ApplyOffsetToAnimation(new Vector2(-0.25f, 0), Offsets);
            data.animator.GetClipByName("death").ApplyOffsetToAnimation(new Vector2(-0.125f, 0), Offsets);
            data.animator.GetClipByName("death_shot").ApplyOffsetToAnimation(new Vector2(-0.25f, 0), Offsets);

            data.animator.GetClipByName("run_down").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("run_right").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("run_right_bw").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("run_up").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);

            data.animator.GetClipByName("doorway").ApplyOffsetToAnimation(new Vector2(-0.125f, 0), Offsets);

            data.animator.GetClipByName("dodge").ApplyOffsetToAnimation(new Vector2(-0.125f, 0), Offsets);
            data.animator.GetClipByName("dodge_bw").ApplyOffsetToAnimation(new Vector2(-0.125f, 0), Offsets);
            data.animator.GetClipByName("dodge_left").ApplyOffsetToAnimation(new Vector2(-0.125f, 0), Offsets);
            data.animator.GetClipByName("dodge_left_bw").ApplyOffsetToAnimation(new Vector2(-0.125f, 0), Offsets);

            data.animator.GetClipByName("ghost_sneeze_left").ApplyOffsetToAnimation(new Vector2(-0.25f, 0), Offsets);
            data.animator.GetClipByName("ghost_sneeze_right").ApplyOffsetToAnimation(new Vector2(-0.25f, 0), Offsets);

            data.animator.GetClipByName("spit_out").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("pet").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("item_get").ApplyOffsetToAnimation(new Vector2(-0.125f, 0), Offsets);


            data.animator.GetClipByName("select_startsparkles").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("select_loopsparkles").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("select_endsparkles").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);

            data.animator.GetClipByName("select_startwrite").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("select_loopwrite").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);
            data.animator.GetClipByName("select_endwrite").ApplyOffsetToAnimation(new Vector2(-0.0625f, 0), Offsets);


            doer.phases = new CharacterSelectIdlePhase[]
            {
                new CharacterSelectIdlePhase()
                {
                    inAnimation = "select_startsparkles",
                    holdAnimation = "select_loopsparkles",
                    outAnimation= "select_endsparkles",
                    holdMin = 5, holdMax = 7,

                },
                new CharacterSelectIdlePhase()
                {
                    inAnimation = "select_startwrite",
                    holdAnimation = "select_loopwrite",
                    outAnimation= "select_endwrite",
                    optionalHoldIdleAnimation = "select_loopwait",
                    optionalHoldChance = 0.7f,
                    holdMin = 12, holdMax = 16,

                },
            };


            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

        }

        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
    }
}
