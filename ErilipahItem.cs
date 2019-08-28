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
        public override void HoldItem(Item item, Player player)
        {
            if (!Main.rand.Chance(1 / 250f))
                return;

            bool light = TileID.Sets.RoomNeeds.CountsAsTorch.Any(t => t == item.createTile) || item.flame;
            if (player.InErilipah() && light)
            {
                if (item.type == mod.ItemType<CrystallineTorch>() && Main.rand.NextBool())
                    return;

                Main.PlaySound(SoundID.LiquidsWaterLava, player.Center);
                if (Main.LocalPlayer == player)
                    item.stack--;
            }
        }

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (!Main.rand.Chance(1 / 300f))
                return;

            bool light = TileID.Sets.RoomNeeds.CountsAsTorch.Any(t => t == item.createTile) || item.flame;

            int ind = item.FindClosestPlayer(2000);
            if (ind == -1)
                return;

            Player player = Main.player[ind];
            if (player.InErilipah() && light)
            {
                if (item.type == mod.ItemType<CrystallineTorch>() && Main.rand.NextBool())
                    return;

                Main.PlaySound(SoundID.LiquidsWaterLava, item.Center);
                if (Main.LocalPlayer == player)
                    item.stack--;
            }
        }
    }
}
