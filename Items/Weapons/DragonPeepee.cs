using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Weapons
{
    public class DragonPeepee : NewModItem
    {
        protected override UseTypes UseType => UseTypes.SwordSwing;
        protected override int[] Dimensions => new int[] { 44, 46 };
        protected override int Rarity => 3;

        protected override int Damage => 36;
        protected override int UseSpeed => 24;
        protected override int Crit => 12;
        protected override float Knockback => 5;

        protected override string DisplayName => "Seeping Death";
        protected override string Tooltip => "Inflicts struck enemies with Wither" +
            "\nThe more they are struck, the deadlier and longer lasting the effect is" +
            "\n'Locked in chests found where the earth begins to melt and demons lurk...and for good reason'";
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            target.AddBuff(mod.BuffType<Wither>(), 300);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.LightsBane);
            r.AddIngredient(ItemID.BlackLens);
            r.AddIngredient(ItemID.HellstoneBar, 3);
            r.AddIngredient(ItemID.Diamond, 7);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(ItemID.BloodButcherer);
            r.AddIngredient(ItemID.BlackLens);
            r.AddIngredient(ItemID.HellstoneBar, 3);
            r.AddIngredient(ItemID.Diamond, 7);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
    public class Wither : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "Erilipah/Debuff";
            return true;
        }
        public override void SetDefaults()
        {
            Main.debuff[Type] = true;
        }

        private int stack = 1;
        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            stack++;
            npc.buffTime[buffIndex] += time / stack;

            return true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen = Math.Min(npc.lifeRegen, 0);
            npc.lifeRegen -= stack * 4 + 4;
            if (Main.rand.NextBool(3))
            {
                Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, 109, newColor: Color.Black)].noGravity = true;
            }

            if (npc.buffTime[buffIndex] <= 1)
            {
                if (stack > 1)
                {
                    npc.buffTime[buffIndex] += 90;
                    stack -= 1;
                }
            }
        }
    }

}
