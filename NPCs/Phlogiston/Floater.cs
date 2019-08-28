using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Phlogiston
{
    public class PhlogistonFloaterOrb : ModNPC
    {
        private int AttackMode
        {
            get { return (int)npc.ai[0]; }
            set
            {
                npc.ai[0] = value;
            }
        }
        private bool HasRing
        {
            get
            {
                int type = mod.NPCType<PhlogistonFloaterRing>();
                return npc.FindClosestNPC(100) != -1;
            }
        }

        private bool summonedRing = false;
        private bool thruAnimation = false;
        private float rotationSpeed = 0;
        private int timer = 0;
        private int hitCount = 0;

        public override void SetDefaults()
        {
            npc.lifeMax = 60;
            npc.width =
            npc.height = 22;
            npc.damage = 60;
            npc.knockBackResist = 0;

            npc.friendly = false;
            npc.dontTakeDamage = true;
            npc.lavaImmune = true;
            npc.noTileCollide = true;
            npc.noGravity = true;
            npc.buffImmune[BuffID.OnFire] = true;

            npc.HitSound = SoundID.NPCHit1; // "Organic"
            npc.DeathSound = SoundID.NPCDeath14; // "Mechanical Explosion"
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 6;
            DisplayName.SetDefault("Phlogiston Construct");
        }
        public override void FindFrame(int frameHeight)
        {
            int frame = 0;
            if (!HasRing && !thruAnimation)
            {
                const int tpf = 6;
                npc.frameCounter++;

                if (npc.frameCounter < tpf)
                    frame = 1;
                else if (npc.frameCounter < tpf * 2)
                    frame = 2;
                else if (npc.frameCounter < tpf * 3)
                    frame = 3;
                else if (npc.frameCounter < tpf * 4)
                    frame = 4;
                else
                    thruAnimation = true;
            }
            if (thruAnimation)
            {
                frame = 5;
            }
            npc.frame.Y = frame * frameHeight;
        }

        // WITH RING
        // Phase 1: Simple fly toward player
        // Phase 2: Rotate and fire projectiles

        // WITHOUT RING
        // Charge at player
        private void NewAttack()
        {
            if (HasRing)
            {
                AttackMode++;
                AttackMode %= 2;
            }
            else
                AttackMode = 2;
            npc.netUpdate = true;
        }
        public override void AI()
        {
            int direction = npc.velocity.X > 0 ? 1 : -1;

            if (Collision.CanHit(Main.player[npc.target].Center, 1, 1, npc.Center, 1, 1))
                npc.noTileCollide = false;
            else
                npc.noTileCollide = true;

            if (!summonedRing)
            {
                summonedRing = true;
                NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, mod.NPCType<PhlogistonFloaterRing>(), ai0: 1, ai1: npc.whoAmI);
            }
            if (!HasRing)
            {
                npc.dontTakeDamage = false;
            }

            if (npc.InTiles() && HasRing)
            {
                AttackMode = 0;
            }
            if (!HasRing)
            {
                AttackMode = 2;
            }

            if (AttackMode == 0)
            {
                Vector2 toPlayer = Main.player[npc.target].Center;
                npc.velocity = npc.GoTo(toPlayer, 0.25f, 6);

                npc.rotation += Helper.RadiansPerTick(0.5f) * direction;
                if (++timer > 280)
                {
                    timer = 0;
                    NewAttack();
                }
            }

            if (AttackMode == 1)
            {
                npc.velocity *= 0.95f;

                rotationSpeed += 0.005f * direction;
                rotationSpeed = MathHelper.Clamp(rotationSpeed, 0.31f, 0.31f);
                npc.rotation += rotationSpeed;

                if (++timer % 30 == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Helper.FireInCircle(npc.Center, 8, mod.ProjectileType<PhlogistonFloaterProj>(),
                            15, 9, 3, ai0: i);
                    }
                }

                if (timer >= 120)
                {
                    timer = 0;
                    NewAttack();
                }
            }

            if (AttackMode == 2 && thruAnimation)
            {
                Vector2 toPlayer = Main.player[npc.target].Center;
                npc.velocity = npc.GoTo(toPlayer, 0.45f, 6);

                npc.rotation += Helper.RadiansPerTick(1) * direction;
            }

            npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -6, Vector2.One * 6);
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => !HasRing;
        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(BuffID.OnFire, 120);

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                const string path = "Gores/Phlogiston/";
                for (int i = 0; i < 3; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore6"));
                }
                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore7"));
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore8"));
                }
                for (int i = 0; i < 19; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType("DeepFlames"));
                }
            }
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            hitCount++;
            if (hitCount >= 2)
            {
                damage *= 2;
                loot = false;
                npc.StrikeNPC(1000, 0, 0, false, true);
            }
        }

        private bool loot = true;
        public override void NPCLoot()
        {
            if (loot)
                Loot.DropItem(npc, mod.ItemType<Items.Phlogiston.StablePhlogiston>(), 4, 6, 100, 1.5f);
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0;
        }
    }
    public class PhlogistonFloaterRing : ModNPC
    {
        private NPC Owner => Main.npc[(int)npc.ai[1]];
        public override void SetDefaults()
        {
            npc.lifeMax = 850;
            npc.width =
            npc.height = 74;
            npc.damage = 35;
            npc.knockBackResist = 0;

            npc.friendly = false;
            npc.dontTakeDamage = false;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.buffImmune[BuffID.OnFire] = true;

            npc.HitSound = SoundID.NPCHit4; // "Metal"
            npc.DeathSound = SoundID.NPCDeath14; // "Mechanical Explosion"
        }
        public override void SetStaticDefaults() => DisplayName.SetDefault("Phlogiston Construct");
        public override void AI()
        {
            if (Owner.active)
            {
                npc.rotation = Owner.rotation * 3;
                npc.Center = Owner.Center - Vector2.UnitY * 10;
            }
            else
            {
                npc.velocity.Y--;
            }
        }
        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Main.netMode != 1 && Owner.ai[0] == 1 && Main.rand.NextBool(2) && Main.expertMode)
            {
                npc.netUpdate = true;
                damage /= 3;
                projectile.Reflect(1);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit) => target.AddBuff(BuffID.OnFire, 120);
        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                const string path = "Gores/Phlogiston/";
                Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore1"));
                Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore2"));
                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore3"));
                }
                for (int i = 0; i < 6; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore4"));
                }
                for (int i = 0; i < 3; i++)
                {
                    Gore.NewGore(npc.Center, Main.rand.NextVector2Circular(3, 3), mod.GetGoreSlot(path + "FloaterGore5"));
                }
            }
        }
    }

    public class PhlogistonFloaterProj : NewModProjectile
    {
        public override Color? GetAlpha(Color lightColor)
        {
            switch ((int)projectile.ai[0])
            {
                case 1:
                    return new Color(
                        255 / 255f,
                        203 / 255f,
                        79 / 225f);

                default:
                    return new Color(
                        245 / 255f,
                        213 / 255f,
                        79 / 225f);
            }
        }
        protected override int[] Dimensions => new int[] { 10, 16 };
        protected override int DustType => mod.DustType("DeepFlames");

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override DamageTypes DamageType => DamageTypes.Melee;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.NoTrail;
        protected override float? Rotation => projectile.velocity.ToRotation() - Degrees90;
        protected override bool[] DamageTeam => new bool[2] { false, true };
    }
}
