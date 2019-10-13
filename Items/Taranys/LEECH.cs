using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Erilipah.Items.Taranys
{
    class LEECH : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("L.E.E.C.H.");
            Tooltip.SetDefault(
                "Barrages your enemies with volleys of plague and death\n" +
                "'You get a cookie if you can guess what it stands for'"
                );

            //Item.staff[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.width = 44;
            item.height = 28;

            item.damage = 33;
            item.knockBack = 2f;
            item.crit = 0;

            item.melee = true;
            item.noMelee = false;
            //item.mana = 0;
            //item.shoot = 0;
            //item.shootSpeed = 0f;

            item.useTime =
            item.useAnimation = 20;

            item.useStyle = ItemUseStyleID.SwingThrow;
            item.autoReuse = true;
            item.useTurn = true;

            item.UseSound = SoundID.Item1;

            item.maxStack = 1;
            item.value = 1000;
            item.rare = ItemRarityID.Blue;
        }

        //public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        //{
        //    return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        //}

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DirtBlock, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
}
