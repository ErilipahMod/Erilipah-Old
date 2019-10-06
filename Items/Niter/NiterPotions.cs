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

            //player.statLifeMax2 -= (int)(50 * proportion) - 50;
            player.allDamage *= proportion;
            player.moveSpeed *= proportion;
            player.jumpSpeedBoost += 2 * proportion;
            player.maxRunSpeed *= System.Math.Min(proportion, 2.06f);
            player.wellFed = true;
        }
    }

    public abstract class NiterPot : ModItem
    {
        public int takeLife;
        public int sickTime;

        public override bool CloneNewInstances => true;
        public override ModItem Clone()
        {
            NiterPot clone = (NiterPot)base.Clone();
            clone.takeLife = takeLife;
            clone.sickTime = sickTime;
            return clone;
        }

        public abstract void SetDefaults(out int width, out int height, out int rare, out int value, out int lifeAmt, out int buffTime, out int sickTime);

        public sealed override void SetDefaults()
        {
            item.CloneDefaults(ItemID.LesserHealingPotion);

            item.healLife = 0;
            item.healMana = 0;

            sickTime = 0;
            takeLife = 0;

            item.buffType = mod.BuffType<NiterPotionBuff>();

            SetDefaults(out item.width, out item.height, out item.rare, out item.value, out takeLife, out item.buffTime, out sickTime);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(1, new TooltipLine(mod, "Takes Life", "Takes " + takeLife + " life"));
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffID.PotionSickness);
        }

        public override void OnConsumeItem(Player player)
        {
            // lt = 50
            // pl = 40

            int lifeTaken = takeLife;
            if (lifeTaken >= player.statLife)
                lifeTaken -= lifeTaken - player.statLife + 1; // Leaves player at 1 HP if they consume while < amount

            CombatText.NewText(player.getRect(), CombatText.DamagedFriendly, lifeTaken);
            player.statLife -= lifeTaken;
            player.netLife = true;

            player.ClearBuff(BuffID.PotionSickness);
            player.potionDelayTime = 0;

            float time = sickTime * (player.potionDelay / 3600f);
            player.AddBuff(BuffID.PotionSickness, (int)time);
        }
    }

    public class LesserNiterPotion : NiterPot
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lesser Niter Potion");
            Tooltip.SetDefault("Induces a potent power surge");
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

        public override void SetDefaults(out int width, out int height, out int rare, out int value, out int lifeAmt, out int buffTime, out int sickTime)
        {
            width = 28;
            height = 34;

            rare = 1;
            value = 500;
            lifeAmt = 50;
            buffTime = 400;
            sickTime = 600;
        }
    }

    public class NiterPotion : NiterPot
    {
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

        public override void SetDefaults(out int width, out int height, out int rare, out int value, out int lifeAmt, out int buffTime, out int sickTime)
        {
            width = 30;
            height = 36;

            rare = 3;
            value = 1500;

            lifeAmt = 100;
            buffTime = 600;
            sickTime = 850;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Niter Potion");
            Tooltip.SetDefault("Induces an immense power surge");
        }
    }

    public class GreaterNiterPotion : NiterPot
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Greater Niter Potion");
            Tooltip.SetDefault("Induces an overwhelming power surge");
        }

        public override void SetDefaults(out int width, out int height, out int rare, out int value, out int lifeAmt, out int buffTime, out int sickTime)
        {
            width = 32;
            height = 38;

            rare = 5;
            value = 2500;

            lifeAmt = 150;
            buffTime = 900;
            sickTime = 1200;
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

    public class SuperNiterPotion : NiterPot
    {
        public override void SetDefaults(out int width, out int height, out int rare, out int value, out int lifeAmt, out int buffTime, out int sickTime)
        {
            width = 32;
            height = 46;

            rare = 7;
            value = 3500;

            lifeAmt = 200;
            buffTime = 1100;
            sickTime = 1450;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Antares Potion");
            Tooltip.SetDefault("Induces a lethal power surge");
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
