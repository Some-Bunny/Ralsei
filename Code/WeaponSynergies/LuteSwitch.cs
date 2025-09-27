using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ralsei.Code
{
    public class LuteSwitch : MonoBehaviour
    {
        public Gun LuteRef;
        private bool LastSynergyState = false;
        private string SwitchGroupOld;

        public void Start()
        {
            this.LuteRef = base.GetComponent<Gun>();
            SwitchGroupOld = this.LuteRef.gunSwitchGroup;
        }
        public void Update()
        {
            if (LuteRef && LuteRef.CurrentOwner && LuteRef.CurrentOwner is PlayerController player)
            {
                if (LastSynergyState != player.PlayerHasActiveSynergy("Memories Of Home"))
                {
                    LastSynergyState = player.PlayerHasActiveSynergy("Memories Of Home");
                    ToggleSynergy(LastSynergyState);
                }
            }
        }
        public void ToggleSynergy(bool Value)
        {
            if (Value)
            {
                this.LuteRef.gunSwitchGroup = "rlsi:newspeciallute";
            }
            else
            {
                this.LuteRef.gunSwitchGroup = SwitchGroupOld;
            }
        }
    }
}
