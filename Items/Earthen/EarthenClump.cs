using Terraria.ModLoader;

namespace Erilipah.Items.Earthen
{
    public class EarthenClump : ModItem
    {
        public override void SetDefaults()
        {
            item.width = (item.height = 34) - 10;
            item.rare = 0;
            item.maxStack = 999;
        }
    }
}
