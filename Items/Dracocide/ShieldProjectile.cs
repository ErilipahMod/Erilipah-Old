using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Dracocide
{
    public class ShieldProjector : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Projects a Newtonian Shield that protects against all high-velocity entities\n" +
                "It has a maximum of 250 life before breaking for a while\n" +
                "It recovers 4 life per second and has your defense");
        }
        public override void SetDefaults()
        {
            item.damage = 0;
            item.knockBack = 0;
            item.crit = 0;
            item.noMelee = true;
            item.noUseGraphic = true;

            item.maxStack = 1;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.autoReuse = false;

            item.width = 36;
            item.height = 30;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.LightRed;

            item.channel = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6, 3);
        }

        public override bool UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<PlayerShield>();

            // Don't create a shield if it's broken
            if (modPlayer.brokenTimer > 0)
                return false;

            // Otherwise create and set its stats to the stored stats
            NPC shield = Main.npc[NPC.NewNPC(
                (int)player.Center.X,
                (int)player.Center.Y,
                NPCType<ShieldProjectorProj>(),
                0,
                player.whoAmI)];

            shield.life = modPlayer.life;
            shield.defense = player.statDefense;

            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemType<MalleableShard>(), 3);
            recipe.AddIngredient(ItemType<Dracocell>(), 10);

            recipe.AddTile(TileID.MythrilAnvil);

            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }

    public class ShieldProjectorProj : ModNPC
    {
        public override string Texture => "Erilipah/NPCs/Dracocide/HDracocideShield";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Newtonian Shield");
            Main.npcFrameCount[npc.type] = 2;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 250;
            npc.defense = 0;
            npc.damage = 0;
            npc.knockBackResist = 0;

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.friendly = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.NPCHit3;
            npc.DeathSound = SoundID.NPCDeath3;

            npc.friendly = true;
            npc.friendlyRegen = 0;
            npc.npcSlots = 0;

            npc.width = 56;
            npc.height = 34;

            npc.value = 0;

            npc.MakeDebuffImmune();
        }

        private Player player => Main.player[(int)npc.ai[0]];

        public override void AI()
        {
            npc.velocity = Vector2.Zero;

            if (Main.myPlayer != player.whoAmI) // only if the player that owns this NPC do we run this code
                return;

            if (!player.channel)
                npc.active = false;

            var modPlayer = player.GetModPlayer<PlayerShield>();

            npc.life = modPlayer.life;
            npc.defense = player.statDefense;

            Vector2 toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero);
            npc.rotation = toMouse.ToRotation() + MathHelper.PiOver2;
            npc.Center = player.Center + toMouse * 50 - new Vector2(0, 6);
        }

        public override void FindFrame(int frameHeight)
        {
            if (npc.immune[Main.myPlayer] > 0) // if just took damage, flash white
                npc.frame.Y = frameHeight;
            else // otherwise it's orange.
                npc.frame.Y = 0;

            if (npc.life < 150 && ++npc.frameCounter % npc.life < 5)
                npc.frame.Y = frameHeight;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (projectile.velocity.Length() < 7)
            {
                damage = 0;
                return;
            }

            if (!projectile.Reflect(1))
            {
                player.immuneTime = 45;
            }
            else
            {
                projectile.penetrate++;
            }
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            player.GetModPlayer<PlayerShield>().life -= (int)damage;
            if (npc.life <= 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, DustType<DracocideDust>());
                }
            }
        }
    }
}
