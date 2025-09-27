using Alexandria.PrefabAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ralsei
{
    public class RalseiCharmVFX : MonoBehaviour
    {
        private Cake CakeRef;
        public tk2dSprite HeartPrefab;
        public tk2dSprite DepletionBarInstance;
        public void InitEffect(float amountOfHearts)
        {
            CurrentStacks = 0;
            CurrentMax = Mathf.RoundToInt(amountOfHearts);
            foreach (var entry in GameManager.Instance.AllPlayers)
            {
                if (entry.activeItems != null)
                {
                    foreach (var _ in entry.activeItems)
                    {
                        if (_ is Cake cake)
                        {
                            CakeRef = cake;
                            break;
                        }
                    }
                }
                if (CakeRef != null) { break; }
            }

            for (float i = 0; i < amountOfHearts; i++)
            {
                float t = (i + 1) / (amountOfHearts + 1);
                var hpSprite = UnityEngine.Object.Instantiate(HeartPrefab, this.transform).GetComponent<tk2dBaseSprite>();
                HeartSprites.Add(hpSprite);
                hpSprite.transform.position = Vector3.Lerp(Left.position, Right.position, t);
            }
        }

        public List<tk2dBaseSprite> HeartSprites = new List<tk2dBaseSprite>();
        public Transform Left, Right;

        public void OnAmountChanged(int NewAmount, int MaxStacks)
        {
            CurrentMax = MaxStacks;
            CurrentStacks = NewAmount;

            HeartSprites.RemoveAll(self => self == null);
            if (HeartSprites.Count == 0) { return; }
            if (NewAmount == MaxStacks)
            {
                return;
            }
            for (int i = 0; i < MaxStacks; i++)
            {
                if (NewAmount > i)
                {
                    HeartSprites[i].SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_heart_002");
                    continue;
                }
                HeartSprites[i].SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_heart_001");
            }
        }

        private int CurrentStacks = 0;
        private int CurrentMax = 1;

        private bool isLast = false;

        public void UpdateBar(float Value)
        {
            if (DepletionBarInstance)
            {
                float m = Value;
                DepletionBarInstance.color = GetColor(m);
                DepletionBarInstance.scale = new Vector3(m, 1, 1);
            }
            if (CakeRef)
            {
                if (CurrentMax == CurrentStacks)
                {
                    if (isLast == false && CakeRef.StoredHearts >= CurrentMax)
                    {
                        foreach (var entry in HeartSprites)
                        {
                            entry.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_heart_003");
                        }
                        isLast = true;
                    }
                }
                else if (isLast == true)
                {
                    OnAmountChanged(CurrentStacks, CurrentMax);
                    isLast = false;
                }
            }
            float count = HeartSprites.Count;
            for (float i = 0; i < count; i++)
            {
                float a = i / count;
                if (HeartSprites[(int)i])
                {
                    var v = HeartSprites[(int)i].transform;
                    v.localPosition = new Vector3(v.localPosition.x, 0.5f + (Easing.DoLerpT((Mathf.PingPong(Time.timeSinceLevelLoad + a, 1)) - 0.5f) * 0.333f));
                }
            }
        }

        public Color GetColor(float value)
        {
            return Color.Lerp(new Color(0.5f, 0.12f, 0.5f), Color.white, value);
        }


        public static void InitializeEffect()
        {
            var obj = PrefabBuilder.BuildObject("CharmEffectRalsei");
            var effect = obj.AddComponent<RalseiCharmVFX>();
            DontDestroyOnLoad(obj);

            obj.layer = LayerMask.NameToLayer("FG_Critical");

            var spriteBar = obj.AddComponent<tk2dSprite>();
            spriteBar.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_bar_001");
            effect.DepletionBarInstance = spriteBar;
            spriteBar.HeightOffGround = -10;

            spriteBar.usesOverrideMaterial = true;
            var mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = spriteBar.renderer.material.mainTexture;
            spriteBar.renderer.material = mat;



            var heartInstance = PrefabBuilder.BuildObject("HeartRalsei");
            spriteBar = heartInstance.AddComponent<tk2dSprite>();
            spriteBar.SetSprite(StaticSpriteCollections.ItemAndGunCollection, "ralsei_charm_heart_001");
            effect.HeartPrefab = spriteBar;
            spriteBar.HeightOffGround = -10;

            spriteBar.usesOverrideMaterial = true;
            mat = new Material(Shader.Find("tk2d/CutoutVertexColorTilted"));
            mat.mainTexture = spriteBar.renderer.material.mainTexture;
            spriteBar.renderer.material = mat;

            heartInstance.layer = LayerMask.NameToLayer("FG_Critical");


            var _left = PrefabBuilder.BuildObject("[Left]");
            var _right = PrefabBuilder.BuildObject("[Right]");
            _left.transform.SetParent(obj.transform);
            _right.transform.SetParent(obj.transform);
            _left.transform.localPosition = new Vector3(-1.125f, 0.5f);
            _right.transform.localPosition = new Vector3(1.125f, 0.5f);
            effect.Left = _left.transform;
            effect.Right = _right.transform;

            EffectPrefab = obj;
        }
        public static GameObject EffectPrefab;
    }
}
