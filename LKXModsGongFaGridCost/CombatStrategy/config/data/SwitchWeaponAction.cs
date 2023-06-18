using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceFrontend.CombatStrategy
{
    [Serializable]
    public class SwitchWeaponAction
    {
        public SwitchWeaponAction() { }

        public SwitchWeaponAction(sbyte weaponIndex)
        {
            this.weaponIndex = weaponIndex;
        }

        public sbyte weaponIndex = -1;
    }
}
