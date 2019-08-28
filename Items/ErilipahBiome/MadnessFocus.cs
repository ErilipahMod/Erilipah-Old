using Erilipah.Biomes.ErilipahBiome.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.ErilipahBiome
{
    public class MadnessFocus : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Teleports you to a location you can reach with limited range\n'Does it concentrate madness or create it?'");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.RodofDiscord);

            item.maxStack = 30;
            item.consumable = true;

            item.useTime = 25;
            item.useAnimation = 20;

            item.width = 32;
            item.height = 32;

            item.value = 3000;
            item.rare = ItemRarityID.Orange;
        }

        public override bool UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 newPos = player.position + (Main.MouseWorld - player.position) * 0.7f;
                if (Collision.CanHit(player.position, player.width, player.height, newPos, player.width, player.height))
                {
                    if (player.HasBuff(BuffID.ChaosState))
                    {
                        player.statLife -= player.statLifeMax2 / 7;
                        if (player.statLife <= 0)
                        {
                            string pronoun = player.Male ? " his" : "her";
                            player.KillMe(PlayerDeathReason.ByCustomReason(player.name + " lost " + pronoun + " mind."), 10, 0);
                            return false;
                        }
                    }

                    player.AddBuff(BuffID.ChaosState, 150);
                    player.Teleport(newPos, 1);
                    return true;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(mod.ItemType<SoulRubble>(), 2);
            recipe.AddTile(mod.TileType<Altar>());

            recipe.SetResult(this, 5);
            recipe.AddRecipe();
        }
    }
}
