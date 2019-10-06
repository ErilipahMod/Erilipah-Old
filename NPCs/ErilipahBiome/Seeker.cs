using Erilipah.Items.Crystalline;
using Erilipah.Items.ErilipahBiome;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.ErilipahBiome
{
    public class Seeker : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Seeker");
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 90;
            npc.defense = 10;
            npc.damage = 22;
            npc.knockBackResist = 0f;
            npc.SetInfecting(2f);

            npc.aiStyle = 0;
            npc.noTileCollide = true;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath22;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 36;
            npc.height = 38;

            npc.value = 0;

            // npc.MakeBuffImmune(BuffID.OnFire);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawNPC(spriteBatch, drawColor);
            this.DrawGlowmask(spriteBatch, Color.White * 0.5f);
            return false;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                Vector2 _vel() => new Vector2(Main.rand.NextFloat(2, 4) * hitDirection, -3.5f);
                for (int i = 0; i < 3; i++)
                {
                    Gore.NewGore(npc.Center, _vel(), mod.GetGoreSlot("Gores/ERBiome/SeekerGore1"));
                    Gore.NewGore(npc.Center, _vel(), mod.GetGoreSlot("Gores/ERBiome/SeekerGore2"));
                }
                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(npc.Center, _vel(), mod.GetGoreSlot("Gores/ERBiome/SeekerGore3"));
                }
                Gore.NewGore(npc.Center, _vel(), mod.GetGoreSlot("Gores/ERBiome/SeekerGore4"));
            }
        }

        private bool Direction { get => npc.ai[2] == 1; set => npc.ai[2] = value ? 1 : 0; }
        private bool HasSeenTarget { get => npc.ai[3] == 1; set => npc.ai[3] = value ? 1 : 0; }
        private Player Target => Main.player[npc.target];

        public override void AI()
        {
            npc.ai[0]++;

            // If spawned from Taranys, just float; else, circle target
            if (npc.ai[1] == 1)
            {
                npc.velocity = npc.GoTo(Target.Center, 0.03f, 1.2f);
            }
            else
            {
                npc.velocity = npc.GoTo(Target.Center + new Vector2(0, 220).RotatedBy(npc.ai[0] / 100f), 0.17f, 2.25f);
            }

            if (Main.netMode != 1 && npc.ai[0] % 160 == 0 && Collision.CanHit(npc.Center, 1, 1, Target.Center, 1, 1))
            {
                Vector2 to = npc.Center.To(Target.Center);
                Projectile.NewProjectile(npc.Center + to * 30, to * 6, mod.ProjectileType<Zoub>(), npc.damage / 2, 1f, 255);
            }

            npc.rotation = npc.Center.To(Target.Center).ToRotation() + (npc.spriteDirection == -1 ? MathHelper.Pi : 0);
        }

        public override void NPCLoot()
        {
            if (npc.ai[1] == 1)
                Loot.DropItem(npc, ItemID.Heart, 1, 1, 50);
            else
                Loot.DropItem(npc, mod.ItemType<PutridFlesh>(), 1, 1, 18);
        }
    }

    public class Zoub : ModProjectile
    {
        public override string Texture => Helper.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sonic Crystal");
        }
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.SetInfecting(0.2f);

            projectile.tileCollide = false;
            projectile.aiStyle = 0;
            projectile.timeLeft = 600;

            projectile.maxPenetrate = 1;
            projectile.hostile = !
                (projectile.friendly = false);
        }
        public override void AI()
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, mod.DustType<CrystallineDust>());
                dust.velocity = Main.rand.NextVector2Unit() * projectile.velocity.Length();
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Blackout, Main.expertMode ? 400 : 100);
        }
    }
}
