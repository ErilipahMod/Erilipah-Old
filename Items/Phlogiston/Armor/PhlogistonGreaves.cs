﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Phlogiston.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class PhlogistonGreaves : NewModItem
    {
        protected override UseTypes UseType => UseTypes.Armor;
        protected override int[] Dimensions => new int[] { 20, 12 };
        protected override int Rarity => 3;
        protected override string Tooltip => "Spawns fire particles while moving, lighting the way" +
            "\n5% increased movement speed" +
            "\n7% increased non-melee damage";

        protected override int Defense => 9;
        public override void UpdateEquip(Player player)
        {
            if ((player.velocity.X != 0 || player.velocity.Y != 0) && Main.rand.NextBool(5))
            {
                const int yOffSet = 20;
                Dust dust = Dust.NewDustDirect(player.position + Vector2.UnitY * yOffSet, player.width, player.legFrame.Height - yOffSet, mod.DustType("DeepFlames"), Scale: 1.375f);
                dust.noGravity = true;
                if (player.InErilipah())
                    dust.noLight = true;
            }
            player.moveSpeed += 0.05f;
            player.allDamage += 0.07f;
            player.meleeDamage -= 0.07f;
        }

        protected override int CraftingTile => TileID.Anvils;
        protected override int[,] CraftingIngredients => new int[,] {
            { mod.ItemType<StablePhlogiston>(), 10 },
            { ItemID.HellstoneBar, 6 }
        };
    }
}
