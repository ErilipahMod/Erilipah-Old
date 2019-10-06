using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    internal class PureFlower : ModItem
    {
        public override void SetDefaults()
        {
            item.maxStack = 1;
            item.width = 32;
            item.height = 30;
            item.rare = 0;
        }

        public override bool OnPickup(Player player)
        {
            Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 29, 1, Main.rand.NextFloat(0.2f, 0.5f));
            player.I().Infect((Main.expertMode ? -5f : -3f) * item.stack);
            item.TurnToAir();
            return false;
        }
    }
}
