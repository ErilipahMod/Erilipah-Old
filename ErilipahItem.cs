using Erilipah.Items.ErilipahBiome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    public class ErilipahItem : GlobalItem
    {
        public const float LightSnuffRate = 1 / 500f;

        public override void HoldItem(Item item, Player player)
        {
            if (!Main.rand.Chance(LightSnuffRate))
                return;

            bool light = TileID.Sets.RoomNeeds.CountsAsTorch.Any(t => t == item.createTile) || item.flame;
            if (player.InErilipah() && light)
            {
                if (item.type == mod.ItemType<CrystallineTorch>() && Main.rand.NextBool())
                    return;

                Main.PlaySound(SoundID.LiquidsWaterLava, player.Center);
                for (int i = 0; i < Main.rand.Next(1, 4); i++)
                {
                    var position = player.RotatedRelativePoint(new Microsoft.Xna.Framework.Vector2(
                        player.itemLocation.X + 12f * player.direction + player.velocity.X,
                        player.itemLocation.Y - 14f + player.velocity.Y), true);

                    Gore.NewGore(
                        position + new Microsoft.Xna.Framework.Vector2(player.itemWidth, player.itemHeight) / 2,
                        new Microsoft.Xna.Framework.Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -0.5f),
                        GoreID.ChimneySmoke2,
                        0.85f);
                }
                if (Main.LocalPlayer == player)
                {
                    if (item.stack == 1)
                        item.TurnToAir();
                    else
                        item.stack--;
                }
            }
        }

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (!Main.rand.Chance(LightSnuffRate))
                return;

            bool light = TileID.Sets.RoomNeeds.CountsAsTorch.Any(t => t == item.createTile) || item.flame;

            int ind = item.FindClosestPlayer(5000);
            if (ind == -1)
                return;

            Player player = Main.player[ind];
            if (player.InErilipah() && light)
            {
                if (item.type == mod.ItemType<CrystallineTorch>() && Main.rand.NextBool())
                    return;

                Main.PlaySound(SoundID.LiquidsWaterLava, item.Center);
                for (int i = 0; i < Main.rand.Next(1, 4); i++)
                {
                    Gore.NewGore(
                        item.Center, new Microsoft.Xna.Framework.Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -0.5f), GoreID.ChimneySmoke2, 0.85f);
                }
                if (Main.LocalPlayer == player)
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
