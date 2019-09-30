using Erilipah.Items.ErilipahBiome;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    public class ErilipahItem : GlobalItem
    {
        public const float LightSnuffRate = 1 / 500f;
        public static void SnuffFx(Vector2 position)
        {
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                Gore.NewGore(position, new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -0.5f), GoreID.ChimneySmoke1 + Main.rand.Next(3), 0.85f);
            }
        }

        public override void HoldItem(Item item, Player player)
        {
            if (!Main.rand.Chance(LightSnuffRate) || item.type == mod.ItemType<Items.ErilipahBiome.ArkenTorch>())
                return;

            bool light = IsLight(item);

            if (player.InErilipah() && light)
            {
                if (item.type == mod.ItemType<CrystallineTorch>() && Main.rand.NextBool())
                    return;

                Main.PlaySound(SoundID.LiquidsWaterLava, player.Center);
                SnuffFx(player.RotatedRelativePoint(new Vector2(
                    player.itemLocation.X + 12f * player.direction + player.velocity.X,
                    player.itemLocation.Y - 14f + player.velocity.Y), true) +
                    new Vector2(player.itemWidth, player.itemHeight) / 2);
                if (Main.myPlayer == player.whoAmI)
                {
                    try
                    {
                        if (item.stack <= 1)
                            item.TurnToAir();
                        else
                            item.stack--;
                    }
                    catch { }
                }
            }
        }

        private static bool IsLight(Item item)
        {
            bool dry = false;
            bool wet = false;
            bool glow = false;
            ItemLoader.AutoLightSelect(item, ref dry, ref wet, ref glow);

            bool light = TileID.Sets.RoomNeeds.CountsAsTorch.Contains(item.createTile);
            light |= item.flame || dry || wet || glow;
            light |= item.type == ItemID.Glowstick || item.type == ItemID.BouncyGlowstick || item.type == ItemID.StickyGlowstick || item.type == ItemID.SpelunkerGlowstick;
            return light;
        }

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (!Main.rand.Chance(LightSnuffRate) || item.type == mod.ItemType<Items.ErilipahBiome.ArkenTorch>())
                return;

            int ind = item.FindClosestPlayer(5000);
            if (ind == -1)
                return;

            bool light = IsLight(item);

            Player player = Main.player[ind];
            if (player.InErilipah() && light)
            {
                if (item.type == mod.ItemType<CrystallineTorch>() && Main.rand.NextBool())
                    return;

                Main.PlaySound(SoundID.LiquidsWaterLava, item.Center);
                SnuffFx(item.Center);
                if (Main.myPlayer == player.whoAmI)
                {
                    if (item.stack == 1)
                        item.TurnToAir();
                    else
                        item.stack--;
                }
            }
        }
    }
}
