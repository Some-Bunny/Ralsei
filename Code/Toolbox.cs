using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Ralsei
{



    public static class SpriteOffseter
    {
        internal static void MakeOffset(this tk2dSpriteCollectionData tk2DSpriteCollectionData, tk2dSpriteDefinition def, Vector3 offset, string[] ApplyToAttachPoints = null, bool changesCollider = false)
        {
            def.position0 += offset;
            def.position1 += offset;
            def.position2 += offset;
            def.position3 += offset;
            def.boundsDataCenter += offset;
            def.untrimmedBoundsDataCenter += offset;
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
                def.colliderVertices[0] += offset;

            if (ApplyToAttachPoints != null)
            {
                var attach = def.GetAttachPoints(tk2DSpriteCollectionData, tk2DSpriteCollectionData.GetSpriteIdByName(def.name));
                if (attach != null)
                {
                    foreach (var entry in attach)
                    {
                        foreach (var Spr in ApplyToAttachPoints)
                        {
                            if (entry.name == Spr)
                            {
                                entry.position += offset;
                                break;
                            }
                        }
                    }
                }
            }

        }
    }
    public static class Toolbox
    {

        public static void DeregisterAttachedObject(this AIActor enemy, GameObject instance, bool completeDestruction = true)
        {
            if (!instance)
            {
                return;
            }
            tk2dBaseSprite[] componentsInChildren = instance.GetComponentsInChildren<tk2dBaseSprite>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
            }
            if (completeDestruction)
            {
                UnityEngine.Object.Destroy(instance);
            }
            else
            {
                instance.transform.parent = null;
            }
        }

        public static GameObject RegisterAttachedObject(this AIActor enemy, GameObject prefab, string attachPoint, float depth = 0f)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
            if (!string.IsNullOrEmpty(attachPoint))
            {
                tk2dSpriteAttachPoint orAddComponent = enemy.sprite.gameObject.GetOrAddComponent<tk2dSpriteAttachPoint>();
                gameObject.transform.parent = orAddComponent.GetAttachPointByName(attachPoint);
            }
            else
            {
                gameObject.transform.parent = enemy.sprite.transform;
            }
            gameObject.transform.localPosition = Vector3.zero;
            if (gameObject.transform.parent == null)
            {
                UnityEngine.Debug.LogError("FAILED TO FIND ATTACHPOINT " + attachPoint + " ON ENEMY");
            }
            tk2dBaseSprite tk2dBaseSprite = gameObject.GetComponent<tk2dBaseSprite>();
            if (tk2dBaseSprite == null)
            {
                tk2dBaseSprite = gameObject.GetComponentInChildren<tk2dBaseSprite>();
            }
            enemy.sprite.AttachRenderer(tk2dBaseSprite);
            tk2dBaseSprite[] componentsInChildren = gameObject.GetComponentsInChildren<tk2dBaseSprite>();
            return gameObject;
        }

        public static Cake isCakedUp()
        {
            foreach (var entry in GameManager.Instance.AllPlayers)
            {
                if (entry.activeItems != null)
                {
                    foreach (var _ in entry.activeItems)
                    {
                        if (_ is Cake cake)
                        {
                            return cake;
                        }
                    }
                }
            }
            return null;
        }
        public static object InvokeNotOverride(this MethodInfo methodInfo,
object targetObject, params object[] arguments)
        {
            var parameters = methodInfo.GetParameters();

            if (parameters.Length == 0)
            {
                if (arguments != null && arguments.Length != 0)
                    throw new Exception("Arguments cont doesn't match");
            }
            else
            {
                if (parameters.Length != arguments.Length)
                    throw new Exception("Arguments cont doesn't match");
            }

            Type returnType = null;
            if (methodInfo.ReturnType != typeof(void))
            {
                returnType = methodInfo.ReturnType;
            }

            var type = targetObject.GetType();
            var dynamicMethod = new DynamicMethod("", returnType,
                    new Type[] { type, typeof(object) }, type);

            var iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0); // this

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                iLGenerator.Emit(OpCodes.Ldarg_1); // load array argument

                // get element at index
                iLGenerator.Emit(OpCodes.Ldc_I4_S, i); // specify index
                iLGenerator.Emit(OpCodes.Ldelem_Ref); // get element

                var parameterType = parameter.ParameterType;
                if (parameterType.IsPrimitive)
                {
                    iLGenerator.Emit(OpCodes.Unbox_Any, parameterType);
                }
                else if (parameterType == typeof(object))
                {
                    // do nothing
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Castclass, parameterType);
                }
            }

            iLGenerator.Emit(OpCodes.Call, methodInfo);
            iLGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.Invoke(null, new object[] { targetObject, arguments });
        }
        public static VFXPool CreateQuickVFXPool(this GameObject gameObject, bool DeathPersistance = true, bool Attachment = true, VFXAlignment alignment = VFXAlignment.NormalAligned, bool Destroyable = false, bool usesZHeight = false, float ZHeight = 0)
        {
            VFXPool pool = new VFXPool();
            pool.type = VFXPoolType.All;
            VFXComplex complex = new VFXComplex();
            VFXObject vfObj = new VFXObject();
            vfObj.attached = Attachment;
            vfObj.persistsOnDeath = DeathPersistance;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = ZHeight;
            vfObj.alignment = alignment;
            vfObj.destructible = Destroyable;
            vfObj.effect = gameObject;
            complex.effects = new VFXObject[] { vfObj };
            pool.effects = new VFXComplex[] { complex };
            return pool;
        }
        public static string RainbowizeString(string orig)
        {
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < orig.Length; i++)
            {
                var c = orig[i];
                var hue = Mathf.InverseLerp(0, orig.Length, i);
                var col = Color.HSVToRGB(hue, 1, 1);
                var colString = ColorUtility.ToHtmlStringRGB(col);
                var charString = $"<color=#{colString}>{c}</color>";
                sb.Append(charString);
            }
            return sb.ToString();
        }

        public static List<T> ConstructListOfSameValues<T>(T value, int length)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < length; i++)
            {
                list.Add(value);
            }
            return list;
        }

        internal static BasicBeamController GenerateBeamPrefabBundleInternal(this Projectile projectile, string defaultSpriteName, tk2dSpriteCollectionData data,
            tk2dSpriteAnimation animation, string IdleAnimationName, Vector2 colliderDimensions, Vector2 colliderOffsets, string impactVFXAnimationName = null,
            Vector2? impactVFXColliderDimensions = null, Vector2? impactVFXColliderOffsets = null, string endAnimation = null, Vector2? endColliderDimensions = null,
            Vector2? endColliderOffsets = null, string muzzleAnimationName = null, Vector2? muzzleColliderDimensions = null, Vector2? muzzleColliderOffsets = null,
            bool glows = false, bool canTelegraph = false, string beamTelegraphIdleAnimationName = null, string beamStartTelegraphAnimationName = null,
            string beamEndTelegraphAnimationName = null, float telegraphTime = 1, bool canDissipate = false, string beamDissipateAnimationName = null,
            string beamStartDissipateAnimationName = null, string beamEndDissipateAnimationName = null, float dissipateTime = 1, bool constructOffsets = true)
        {
            try
            {
                if (projectile.specRigidbody)
                    projectile.specRigidbody.CollideWithOthers = false;

                tk2dTiledSprite tiledSprite = projectile.gameObject.GetOrAddComponent<tk2dTiledSprite>();

                tiledSprite.Collection = data;
                tiledSprite.SetSprite(data, data.GetSpriteIdByName(defaultSpriteName));
                tk2dSpriteDefinition def = tiledSprite.GetCurrentSpriteDef();
                def.colliderVertices = new Vector3[] { 0.0625f * colliderOffsets, 0.0625f * colliderDimensions };

                if (constructOffsets)
                    def.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleLeft); //NOTE: this seems right, but double check later

                tk2dSpriteAnimator animator = projectile.gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
                animator._startingSpriteCollection = data;
                animator.Library = animation;
                animator.playAutomatically = true;
                animator.defaultClipId = animation.GetClipIdByName(IdleAnimationName);

                UnityEngine.Object.Destroy(projectile.GetComponentInChildren<tk2dSprite>());
                projectile.sprite = tiledSprite;
                projectile.sprite.Collection = data;

                BasicBeamController beamController = projectile.gameObject.GetOrAddComponent<BasicBeamController>();
                beamController.sprite = tiledSprite;
                beamController.spriteAnimator = animator;
                beamController.m_beamSprite = tiledSprite;

                //---------------- Sets up the animation for the main part of the beam
                beamController.beamAnimation = IdleAnimationName;

                //------------- Sets up the animation for the part of the beam that touches the wall

                if (endAnimation != null && endColliderDimensions != null && endColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, endAnimation, (Vector2)endColliderDimensions, (Vector2)endColliderOffsets, constructOffsets: constructOffsets);
                    beamController.beamEndAnimation = endAnimation;
                }
                else
                {
                    SetupBeamPart(animation, data, IdleAnimationName, null, null, def.colliderVertices, constructOffsets: constructOffsets);
                    beamController.beamEndAnimation = IdleAnimationName;
                }

                //---------------Sets up the animaton for the VFX that plays over top of the end of the beam where it hits stuff
                if (impactVFXAnimationName != null && impactVFXColliderDimensions != null && impactVFXColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, impactVFXAnimationName, (Vector2)impactVFXColliderDimensions, (Vector2)impactVFXColliderOffsets, anchor: tk2dBaseSprite.Anchor.MiddleCenter, constructOffsets: constructOffsets);
                    beamController.impactAnimation = impactVFXAnimationName;
                }

                //--------------Sets up the animation for the very start of the beam
                if (muzzleAnimationName != null && muzzleColliderDimensions != null && muzzleColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, muzzleAnimationName, (Vector2)muzzleColliderDimensions, (Vector2)muzzleColliderOffsets, constructOffsets: constructOffsets);
                    beamController.beamStartAnimation = muzzleAnimationName;
                }
                else
                {
                    SetupBeamPart(animation, data, IdleAnimationName, null, null, def.colliderVertices, constructOffsets: constructOffsets);
                    beamController.beamStartAnimation = IdleAnimationName;
                }

                if (canTelegraph)
                {
                    beamController.usesTelegraph = true;
                    beamController.telegraphAnimations = new BasicBeamController.TelegraphAnims();
                    if (beamStartTelegraphAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamStartTelegraphAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.telegraphAnimations.beamStartAnimation = beamStartTelegraphAnimationName;
                    }
                    if (beamTelegraphIdleAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamTelegraphIdleAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.telegraphAnimations.beamAnimation = beamTelegraphIdleAnimationName;
                    }
                    if (beamEndTelegraphAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamEndTelegraphAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.telegraphAnimations.beamEndAnimation = beamEndTelegraphAnimationName;
                    }
                    beamController.telegraphTime = telegraphTime;
                }

                if (canDissipate)
                {
                    beamController.endType = BasicBeamController.BeamEndType.Dissipate;
                    beamController.dissipateAnimations = new BasicBeamController.TelegraphAnims();
                    if (beamStartDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamStartDissipateAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.dissipateAnimations.beamStartAnimation = beamStartDissipateAnimationName;
                    }
                    if (beamDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamDissipateAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.dissipateAnimations.beamAnimation = beamDissipateAnimationName;
                    }
                    if (beamEndDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamEndDissipateAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.dissipateAnimations.beamEndAnimation = beamEndDissipateAnimationName;
                    }
                    beamController.dissipateTime = dissipateTime;
                }


                return beamController;
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
                return null;
            }
        }
        internal static void SetupBeamPart(tk2dSpriteAnimation beamAnimation, tk2dSpriteCollectionData data, string animationName, Vector2? colliderDimensions = null,
            Vector2? colliderOffsets = null, Vector3[] overrideVertices = null, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Once,
            tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleLeft, bool constructOffsets = true)
        {
            foreach (var frame in beamAnimation.GetClipByName(animationName).frames)
            {
                tk2dSpriteDefinition frameDef = data.spriteDefinitions[frame.spriteId];
                if (constructOffsets)
                    frameDef.ConstructOffsetsFromAnchor(anchor);
                if (overrideVertices != null)
                    frameDef.colliderVertices = overrideVertices;
                else if (colliderDimensions != null && colliderOffsets != null)
                    frameDef.colliderVertices = new Vector3[] { 0.0625f * colliderOffsets.Value, 0.0625f * colliderDimensions.Value };
                else
                    ETGModConsole.Log("<size=100><color=#ff0000ff>BEAM ERROR: colliderDimensions or colliderOffsets was null with no override vertices!</color></size>", false);
            }
        }
        private static readonly HashSet<tk2dSpriteDefinition> adjustedDefs = new HashSet<tk2dSpriteDefinition>();

        internal static void ConstructOffsetsFromAnchor(this tk2dSpriteDefinition def, tk2dBaseSprite.Anchor anchor, Vector2? scale = null, bool fixesScale = false, bool changesCollider = true)
        {
            if (adjustedDefs.Contains(def))
                return; // don't set up offsets for definitions multiple times
            adjustedDefs.Add(def);

            if (!scale.HasValue)
            {
                scale = new Vector2?(def.position3);
            }
            if (fixesScale)
            {
                Vector2 fixedScale = scale.Value - def.position0.XY();
                scale = new Vector2?(fixedScale);
            }
            float xOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.UpperCenter)
            {
                xOffset = -(scale.Value.x / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                xOffset = -scale.Value.x;
            }
            float yOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.MiddleLeft)
            {
                yOffset = -(scale.Value.y / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                yOffset = -scale.Value.y;
            }
            def.MakeOffset(new Vector2(xOffset, yOffset), false);
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
            {
                float colliderXOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.UpperLeft)
                {
                    colliderXOffset = (scale.Value.x / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderXOffset = -(scale.Value.x / 2f);
                }
                float colliderYOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.LowerRight)
                {
                    colliderYOffset = (scale.Value.y / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderYOffset = -(scale.Value.y / 2f);
                }
                def.colliderVertices[0] += new Vector3(colliderXOffset, colliderYOffset, 0);
            }
        }

        internal static void MakeOffset(this tk2dSpriteDefinition def, Vector3 offset, bool changesCollider = false)
        {
            def.position0 += offset;
            def.position1 += offset;
            def.position2 += offset;
            def.position3 += offset;
            def.boundsDataCenter += offset;
            def.untrimmedBoundsDataCenter += offset;
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
                def.colliderVertices[0] += offset;
        }
        public static void ApplyOffsetToAnimation(this tk2dSpriteAnimationClip tk2DSpriteAnimationClip, Vector2 Offset, List<int> IdsModified = null)
        {
            if (IdsModified == null)
            {
                IdsModified = new List<int>();
            }
            if (tk2DSpriteAnimationClip == null) { return; }
            //tk2dSpriteAnimationClip deathClip = data.animator.GetClipByName("death_shot");
            var offsetsX2 = Offset.x;
            var offsetsY2 = Offset.y;
            for (int i = 0; i < tk2DSpriteAnimationClip.frames.Length; i++)
            {
                int id = tk2DSpriteAnimationClip.frames[i].spriteId;
                if (!IdsModified.Contains(id))
                {
                    IdsModified.Add(id);
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position0.x += offsetsX2;
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position0.y += offsetsY2;
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position1.x += offsetsX2;
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position1.y += offsetsY2;
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position2.x += offsetsX2;
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position2.y += offsetsY2;
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position3.x += offsetsX2;
                    tk2DSpriteAnimationClip.frames[i].spriteCollection.spriteDefinitions[id].position3.y += offsetsY2;
                }

            }
        }

        public static PlayerItem GetRandomActiveOfQualities(System.Random usedRandom, List<int> excludedIDs, params PickupObject.ItemQuality[] qualities)
        {
            List<PlayerItem> list = new List<PlayerItem>();
            for (int i = 0; i < PickupObjectDatabase.Instance.Objects.Count; i++)
            {
                if (PickupObjectDatabase.Instance.Objects[i] != null && PickupObjectDatabase.Instance.Objects[i] is PlayerItem)
                {
                    if (PickupObjectDatabase.Instance.Objects[i].quality != PickupObject.ItemQuality.EXCLUDED && PickupObjectDatabase.Instance.Objects[i].quality != PickupObject.ItemQuality.SPECIAL)
                    {
                        if (!(PickupObjectDatabase.Instance.Objects[i] is ContentTeaserItem))
                        {
                            if (Array.IndexOf<PickupObject.ItemQuality>(qualities, PickupObjectDatabase.Instance.Objects[i].quality) != -1)
                            {
                                if (!excludedIDs.Contains(PickupObjectDatabase.Instance.Objects[i].PickupObjectId))
                                {
                                    EncounterTrackable component = PickupObjectDatabase.Instance.Objects[i].GetComponent<EncounterTrackable>();
                                    if (component && component.PrerequisitesMet())
                                    {
                                        list.Add(PickupObjectDatabase.Instance.Objects[i] as PlayerItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int num = usedRandom.Next(list.Count);
            if (num < 0 || num >= list.Count)
            {
                return null;
            }
            return list[num];
        }

        public static Vector2 GetUnitOnCircle(float angleDegrees, float radius)
        {

            // initialize calculation variables
            float _x = 0;
            float _y = 0;
            float angleRadians = 0;
            Vector2 _returnVector;

            // convert degrees to radians
            angleRadians = angleDegrees * Mathf.PI / 180.0f;

            // get the 2D dimensional coordinates
            _x = radius * Mathf.Cos(angleRadians);
            _y = radius * Mathf.Sin(angleRadians);

            // derive the 2D vector
            _returnVector = new Vector2(_x, _y);

            // return the vector info
            return _returnVector;
        }
        public static Vector3 GetUnitOnCircleVec3(float angleDegrees, float radius)
        {

            // initialize calculation variables
            float _x = 0;
            float _y = 0;
            float angleRadians = 0;
            Vector3 _returnVector;

            // convert degrees to radians
            angleRadians = angleDegrees * Mathf.PI / 180.0f;

            // get the 2D dimensional coordinates
            _x = radius * Mathf.Cos(angleRadians);
            _y = radius * Mathf.Sin(angleRadians);

            // derive the 2D vector
            _returnVector = new Vector3(_x, _y);

            // return the vector info
            return _returnVector;
        }
        public static GameObject SmartPlayEffectOnActor(this GameActor effector, GameObject effect, Vector3 offset, bool attached = true, bool IgnorePools = true, bool alreadyMiddleCenter = false, bool useHitbox = false)
        {
            GameObject gameObject = SpawnManager.SpawnVFX(effect, IgnorePools);
            tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
            Vector3 a = (!useHitbox || !effector.specRigidbody || effector.specRigidbody.HitboxPixelCollider == null) ? effector.sprite.WorldCenter.ToVector3ZUp(0f) : effector.specRigidbody.HitboxPixelCollider.UnitCenter.ToVector3ZUp(0f);
            if (!alreadyMiddleCenter)
            {
                component.PlaceAtPositionByAnchor(a + offset, tk2dBaseSprite.Anchor.MiddleCenter);
            }
            else
            {
                component.transform.position = a + offset;
            }
            if (attached)
            {
                gameObject.transform.parent = effector.transform;
                component.HeightOffGround = 0.2f;
                effector.sprite.AttachRenderer(component);
                if (effector is PlayerController)
                {
                    SmartOverheadVFXController component2 = gameObject.GetComponent<SmartOverheadVFXController>();
                    if (component2 != null)
                    {
                        component2.Initialize(effector as PlayerController, offset);
                    }
                }
            }
            if (!alreadyMiddleCenter)
            {
                gameObject.transform.localPosition = gameObject.transform.localPosition.QuantizeFloor(0.0625f);
            }
            return gameObject;
        }
    }
}
