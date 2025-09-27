using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ralsei.Code.VFX
{
    public class RalseiTargetVFX : MonoBehaviour
    {
        public GameObject BracketPrefab;
        public GameObject[] BracketInst;
        public AIActor TargetInst;
        Vector2 Size;
        public void Start()
        {
            if (this.TargetInst)
            {

                for (int i = 0; i < 24; i++)
                {
                    GlobalSparksDoer.DoRandomParticleBurst(1, TargetInst.sprite.WorldCenter, TargetInst.sprite.WorldCenter,
                        Toolbox.GetUnitOnCircle(15 * i, 2), 1f, 0.05f, 0.1f, 1f, Color.white * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
                }

                var hitboxCollider = this.TargetInst.specRigidbody.HitboxPixelCollider;
                Size = new Vector2(-((float)(hitboxCollider.Width) / 32f), (float)(hitboxCollider.Height) / 32f);

                this.transform.position = this.TargetInst.sprite.WorldCenter - new Vector2(0.125f, Size.y + 0.375f);


                BracketInst = new GameObject[2];

                var bracket_TL = UnityEngine.Object.Instantiate(BracketPrefab, this.transform);
                bracket_TL.transform.position = this.TargetInst.sprite.WorldCenter + new Vector2(Size.x, Size.y) + new Vector2(0, 0.125f);
                BracketInst[0] = bracket_TL;

                var bracket_BR = UnityEngine.Object.Instantiate(BracketPrefab, this.transform);
                bracket_BR.transform.position = this.TargetInst.sprite.WorldCenter - new Vector2(Size.x, Size.y) + new Vector2(0, 0.125f);
                bracket_BR.transform.localRotation = Quaternion.Euler(0, 0, 180);
                BracketInst[1] = bracket_BR;
            }
        }

        public void Update()
        {
            if (TargetInst)
            {
                this.transform.position = this.TargetInst.sprite.WorldCenter - new Vector2(0, Size.y + 0.375f);
                var TL = BracketInst[0];
                var BR = BracketInst[1];


                TL.transform.position = this.TargetInst.sprite.WorldCenter + new Vector2(Size.x, Size.y) + new Vector2(0, 0.125f);
                TL.transform.position = TL.transform.position.WithZ(100);

                BR.transform.position = this.TargetInst.sprite.WorldCenter - new Vector2(Size.x, Size.y) + new Vector2(0, 0.125f);
                BR.transform.position = BR.transform.position.WithZ(100);
            }
            if (ScarfWeapon.RalseiSlash.Cooldown > 0)
            {
                ScarfWeapon.RalseiSlash.Cooldown -= Time.deltaTime;
            }
        }

        public void OnDestroy()
        {
            ScarfWeapon.RalseiSlash.Cooldown = -1;
            for (int i = 0; i < 24; i++)
            {
                GlobalSparksDoer.DoRandomParticleBurst(1, TargetInst.sprite.WorldCenter, TargetInst.sprite.WorldCenter,
                    Toolbox.GetUnitOnCircle(15 * i, 5), 1f, 0.05f, 0.1f, 0.35f, Color.white * 3, GlobalSparksDoer.SparksType.FLOATY_CHAFF);
            }
        }
    }
}
