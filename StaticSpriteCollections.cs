using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ralsei
{
    public class StaticSpriteCollections
    {
        public static tk2dSpriteCollectionData RalseiCharacterCollection;
        public static tk2dSpriteAnimation RalseiCharacterAnimation;

        public static tk2dSpriteCollectionData ItemAndGunCollection;
        public static tk2dSpriteAnimation ItemAndGunAnimation;

        public static tk2dSpriteCollectionData ProjectileCollection;
        public static tk2dSpriteAnimation ProjectileAnimation;

        public static void Init()
        {
            RalseiCharacterCollection = RalseiModule.assetBundle.LoadAsset<GameObject>("RalseiCollection").GetComponent<tk2dSpriteCollectionData>();
            RalseiCharacterAnimation = RalseiModule.assetBundle.LoadAsset<GameObject>("RalseiAnimation").GetComponent<tk2dSpriteAnimation>();

            ItemAndGunCollection = RalseiModule.assetBundle.LoadAsset<GameObject>("ItemCollection").GetComponent<tk2dSpriteCollectionData>();
            ItemAndGunAnimation = RalseiModule.assetBundle.LoadAsset<GameObject>("ItemAnimation").GetComponent<tk2dSpriteAnimation>();

            ProjectileCollection = RalseiModule.assetBundle.LoadAsset<GameObject>("ProjectileCollection").GetComponent<tk2dSpriteCollectionData>();
            ProjectileAnimation = RalseiModule.assetBundle.LoadAsset<GameObject>("ProjectileAnimation").GetComponent<tk2dSpriteAnimation>();
        }
    }
}
