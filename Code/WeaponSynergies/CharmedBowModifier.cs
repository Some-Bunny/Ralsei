using Alexandria.ItemAPI;
using Alexandria.VisualAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ralsei.Code
{
    public class CharmedBowModifier : BraveBehaviour
    {
        private Gun GunRef;
        private bool LastSynergyState = false;

        public string SynergyName = "Heart Maker";


        public void Start()
        {
            this.GunRef = base.GetComponent<Gun>();
        }
        public void Update()
        {
            if (GunRef && GunRef.CurrentOwner && GunRef.CurrentOwner is PlayerController player)
            {
                bool Syn = player.PlayerHasActiveSynergy(SynergyName);
                if (LastSynergyState != Syn)
                {
                    LastSynergyState = player.PlayerHasActiveSynergy(SynergyName);
                    ToggleSynergy(LastSynergyState);
                }
            }
        }
        public void ToggleSynergy(bool Value)
        {
            if (Value)
            {
                GunRef.PostProcessProjectile += PPP;
            }
            else
            {
                GunRef.PostProcessProjectile -= PPP;
            }
        }

        public void PPP(Projectile projectile)
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                projectile.statusEffectsToApply = projectile.statusEffectsToApply ?? new List<GameActorEffect> { };
                projectile.statusEffectsToApply.Add(new RalseiCharmable()
                {
                    StackAmountToApply = 3
                });
                projectile.baseData.speed *= 1.5f;
                projectile.UpdateSpeed();
                var afterimage = projectile.gameObject.AddComponent<ImprovedAfterImage>();
                afterimage.shadowLifetime = 0.3f;
                afterimage.shadowTimeDelay = 0.05f;
                afterimage.spawnShadows = true;
                afterimage.dashColor = new Color(1f, 0.5f, 0.8f, 1f);

            }
        }
    }
}
