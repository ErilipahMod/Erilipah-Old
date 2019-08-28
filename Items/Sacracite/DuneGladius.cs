using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.Items.Sacracite
{
    public class DuneGladius : ModItem
    {
        private const int Damage = 17;
        private const int Crit = 4;
        private const int UseSpeed = 15;
        private const float Knockback = 6;
        private const float ShootSpeed = 9.5f;
        private static readonly int[] Dimensions = new int[] { 40, 46 };
        private new string DisplayName = null;
        private new string Tooltip = "After a critical strike, player is buffed for 8 seconds" +
            "\nWhile buffed, melee damage is 25% higher and player agility is greatly increased";
        private const bool Staff = false;
        private const bool Shoots = false;

        public override void SetDefaults()
        {
            // most important
            item.width = Dimensions[0];
            item.height = Dimensions[1];
            item.useStyle = Terraria.ID.ItemUseStyleID.SwingThrow;
            item.maxStack = 1;

            // most changed
            item.crit = Crit - 4;
            item.damage = Damage;
            item.knockBack = Knockback;
            item.useTime =
                item.useAnimation = UseSpeed;
            if (Shoots)
#pragma warning disable CS0162 // Unreachable code detected
                item.shootSpeed = ShootSpeed;
#pragma warning restore CS0162 // Unreachable code detected

            // occasionally changed booleans
            item.noMelee = false;
            item.melee = true;
            item.noUseGraphic = false;
            item.consumable = false;
            item.autoReuse = true;
            item.useTurn = true;

            // occasionally changed integers
            item.rare = 2;
            item.UseSound = Terraria.ID.SoundID.Item1;
            item.useAmmo = AmmoID.None;
        }
        public override void SetStaticDefaults()
        {
            if (DisplayName != null)
                base.DisplayName.SetDefault(DisplayName);

            if (Tooltip != null)
                base.Tooltip.SetDefault(Tooltip);

            Terraria.Item.staff[item.type] = Staff;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);

            r.AddIngredient(mod.ItemType<SacraciteIngot>(), 3);
            r.AddIngredient(mod.ItemType<SacraciteCore>(), 1);
            r.AddTile(Terraria.ID.TileID.Anvils);
            r.SetResult(this, 1);
            r.AddRecipe();
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (crit)
            {
                player.AddBuff(mod.BuffType("DesertsRage"), 560);
            }
        }
    }
    public class DesertsRage : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Desert's Rage");
            Description.SetDefault("Deal 25% more melee damage, accelerate 40% faster, runspeed is 20% higher, and you gain a double jump");
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.meleeDamage *= 1.25f;
            player.moveSpeed *= 1.4f;
            player.maxRunSpeed *= 1.2f;
            player.doubleJumpSandstorm = true;
        }
    }
}