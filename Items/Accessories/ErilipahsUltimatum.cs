#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Erilipah.Items.Accessories
{
    public class ErilipahsUltimatum : NewModItem
    {
        protected override int Damage => 0;
        protected override int[] UseSpeedArray => new int[] { 0, 0 };
        protected override float Knockback => 0;
        protected override bool FiresProjectile => false;
        protected override int[] Dimensions => new int[] { 48, 44 };

        protected override int Rarity => ItemRarityID.Red;
        protected override UseTypes UseType => UseTypes.None;
        protected override int? Value => Item.sellPrice(999, 99, 99, 99);
        protected override bool Accessory => true;

        protected override string DisplayName => "Erilipah Ultimate";
        protected override string Tooltip => "Makes you revive upon death while in your inventory";
    }
}
#endif