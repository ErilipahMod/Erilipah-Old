using Terraria.ModLoader;

namespace Erilipah.Items.LunarBee
{
    [AutoloadEquip(EquipType.Head)]
    public class LunarBeeMask : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 20;
            item.rare = 2;
            item.vanity = true;
        }

        public override bool DrawHead()
        {
            return false;
        }
    }
}
