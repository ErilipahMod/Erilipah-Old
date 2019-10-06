using Erilipah.Items.ErilipahBiome;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Niter
{
    public class NiterPotionBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Niter");
            Description.SetDefault("Power at a cost");
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            float proportion = (500 + player.buffTime[buffIndex]) / 500f;

            player.statLifeMax2 -= (int)(50 * proportion) - 50;
            player.allDamage *= proportion;
            player.moveSpeed *= proportion;
            player.maxRunSpeed *= System.Math.Min(proportion, 2.06f);
            player.wellFed = true;
        }
    }

    public class LesserNiterPotion : ModItem
    {
        private const int amount = 50;
        public static void InsertNiterTooltip(List<TooltipLine> tooltips, Mod mod, int amount)
        {
            tooltips.Insert(1, new TooltipLine(mod, "Takes Life", "Takes " + amount + " life"));
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lesser Niter Potion");
            Tooltip.SetDefault("Induces a potent power surge");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LesserHealingPotion);

            item.healLife = 0;
            item.healMana = 0;

            item.width = 28;
            item.height = 34;

            item.value = Item.sellPrice(0, 0, 5);
            item.rare = 1;

            item.buffTime = 400;
            item.buffType = mod.BuffType<NiterPotionBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            InsertNiterTooltip(tooltips, mod, amount);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness) && player.statLife > amount;
        }

        public override void OnConsumeItem(Player player)
        {
            CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, amount);
            player.statLife -= amount;
            player.netLife = true;

            player.ClearBuff(BuffID.PotionSickness);
            player.potionDelayTime = 0;

            float time = 700 * (player.potionDelay / 3600f);
            player.AddBuff(BuffID.PotionSickness, (int)time);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BottledWater, 3);
            recipe.AddIngredient(ItemID.FallenStar, 1);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }

    public class NiterPotion : ModItem
    {
        public override string Texture => Helper.Invisible;

        private const int amount = 100;
        public static void InsertNiterTooltip(List<TooltipLine> tooltips, Mod mod, int amount)
        {
            tooltips.Insert(1, new TooltipLine(mod, "Takes Life", "Takes " + amount + " life"));
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Niter Potion");
            Tooltip.SetDefault("Induces an immense power surge");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LesserHealingPotion);

            item.healLife = 0;
            item.healMana = 0;

            item.width = 28;
            item.height = 34;

            item.value = Item.sellPrice(0, 0, 15);
            item.rare = 3;

            item.buffTime = 600;
            item.buffType = mod.BuffType<NiterPotionBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            InsertNiterTooltip(tooltips, mod, amount);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness) && player.statLife > amount;
        }

        public override void OnConsumeItem(Player player)
        {
            CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, amount);
            player.statLife -= amount;
            player.netLife = true;

            player.ClearBuff(BuffID.PotionSickness);
            player.potionDelayTime = 0;

            float time = 850 * (player.potionDelay / 3600f);
            player.AddBuff(BuffID.PotionSickness, (int)time);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.VileMushroom, 1);
            recipe.AddIngredient(mod.ItemType<LesserNiterPotion>(), 2);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();

            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ViciousMushroom, 1);
            recipe.AddIngredient(mod.ItemType<LesserNiterPotion>(), 2);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }

    public class GreaterNiterPotion : ModItem
    {
        public override string Texture => Helper.Invisible;

        private const int amount = 150;
        public static void InsertNiterTooltip(List<TooltipLine> tooltips, Mod mod, int amount)
        {
            tooltips.Insert(1, new TooltipLine(mod, "Takes Life", "Takes " + amount + " life"));
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Greater Niter Potion");
            Tooltip.SetDefault("Induces an overwhelming power surge");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LesserHealingPotion);

            item.healLife = 0;
            item.healMana = 0;

            item.width = 28;
            item.height = 34;

            item.value = Item.sellPrice(0, 0, 25);
            item.rare = 5;

            item.buffTime = 1000;
            item.buffType = mod.BuffType<NiterPotionBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            InsertNiterTooltip(tooltips, mod, amount);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness) && player.statLife > amount;
        }

        public override void OnConsumeItem(Player player)
        {
            CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, amount);
            player.statLife -= amount;
            player.netLife = true;

            player.ClearBuff(BuffID.PotionSickness);
            player.potionDelayTime = 0;

            float time = 1000 * (player.potionDelay / 3600f);
            player.AddBuff(BuffID.PotionSickness, (int)time);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BottledWater, 3);
            recipe.AddIngredient(mod.ItemType<SoulRubble>());
            recipe.AddIngredient(mod.ItemType<PutridFlesh>(), 3);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 3);
            recipe.AddRecipe();
        }
    }

    public class SuperNiterPotion : ModItem
    {
        public override string Texture => Helper.Invisible;

        private const int amount = 200;
        public static void InsertNiterTooltip(List<TooltipLine> tooltips, Mod mod, int amount)
        {
            tooltips.Insert(1, new TooltipLine(mod, "Takes Life", "Takes " + amount + " life"));
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Antares Potion");
            Tooltip.SetDefault("Induces an overwhelming power surge");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LesserHealingPotion);

            item.healLife = 0;
            item.healMana = 0;

            item.width = 28;
            item.height = 34;

            item.value = Item.sellPrice(0, 0, 35);
            item.rare = 7;

            item.buffTime = 1500;
            item.buffType = mod.BuffType<NiterPotionBuff>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            InsertNiterTooltip(tooltips, mod, amount);
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness) && player.statLife > amount;
        }

        public override void OnConsumeItem(Player player)
        {
            CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, amount);
            player.statLife -= amount;
            player.netLife = true;

            player.ClearBuff(BuffID.PotionSickness);
            player.potionDelayTime = 0;

            float time = 2000 * (player.potionDelay / 3600f);
            player.AddBuff(BuffID.PotionSickness, (int)time);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<GreaterNiterPotion>(), 4);
            recipe.AddIngredient(ItemID.FragmentNebula);
            recipe.AddIngredient(ItemID.FragmentSolar);
            recipe.AddIngredient(ItemID.FragmentStardust);
            recipe.AddIngredient(ItemID.FragmentVortex);
            recipe.AddTile(TileID.Bottles);
            recipe.SetResult(this, 4);
            recipe.AddRecipe();
        }
    }

}
