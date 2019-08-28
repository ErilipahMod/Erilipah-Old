using Erilipah.Items.Dracocide;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah.NPCs.Dracocide
{
    public class AssaultDrone : DracocideDrone
    {
        private float AttackTime { get => npc.ai[0]; set => npc.ai[0] = value; }

        private float moveFactor;
        private bool clockwise;
        private const float projSpeed = 8f;
        private NPC shield;

        public override void AI()
        {
            if (!Base())
            {
                if (shield != null) shield.active = false;
                return;
            }

            Vector2 dir = npc.Center.To(Target.Center);
            AttackTime++; // Increase this sucka

            #region Movement
            if (AttackTime < 45) // if not firing
            {
                var likeMe = Main.npc.Where(x => x.active && x.type == npc.type && x.ai[1] == npc.ai[1] && x.ai[2] == npc.ai[2]);
                clockwise = likeMe.ToList().IndexOf(npc) % 2 == 0;

                if (npc.Distance(Target.Center) > 400) // if too far, get closer
                    npc.velocity += dir;
                if (npc.Distance(Target.Center) < 150) // if too close, get farther
                    npc.velocity -= dir;

                if (clockwise) moveFactor++;
                else moveFactor--;

                float max = (float)(Math.PI / 4d);
                float radians = MathHelper.Lerp(-max, max, moveFactor % 100f / 100f);
                Vector2 goTo = new Vector2(Target.Center.X, Target.Center.Y - 375);

                npc.velocity = npc.GoTo(goTo.RotatedBy(radians, Target.Center), 0.15f, 5);
            }
            else if (AttackTime < 55)
            {
                npc.velocity *= 0.8f; // slow down before firing
            }
            #endregion
            #region Attacking
            if (AttackTime < 50)
            {
                // Rotate towards the player and get up a shield @them
                npc.rotation = dir.ToRotation();

                if (shield == null || !shield.active)
                    shield = Main.npc[NPC.NewNPC(
                        (int)(npc.Center.X + dir.X * 32),
                        (int)(npc.Center.Y + dir.Y * 32),
                        mod.NPCType<HDracocideShield>())];
                shield.Center = npc.Center + dir * 35;
                shield.rotation = dir.ToRotation() + MathHelper.PiOver2;
            }
            else if (AttackTime < 55)
            {
                shield.active = false; // Rid the shield
            }
            else if (AttackTime <= 80)
            {
                // Rapid-fire in a small, 20-degree spread towards the player
                if (AttackTime % 4 == 0)
                {
                    Vector2 fireTo = dir.RotatedBy(
                        MathHelper.ToRadians(
                            AttackTime - 65
                            ));

                    if (Main.netMode != 1)
                        Projectile.NewProjectile(npc.Center + fireTo * 34, fireTo * projSpeed, mod.ProjectileType<AssaultBolt>(), npc.damage, 2f, Main.myPlayer);

                    npc.velocity -= dir * 0.15f;
                    npc.rotation = fireTo.ToRotation();
                    Main.PlaySound(SoundID.NPCDeath14, npc.Center + fireTo * 32);
                    Dust.NewDust(npc.Center + fireTo * 32, 0, 0, mod.DustType("DeepFlames"), fireTo.X * 3, fireTo.Y * 3);

                    npc.netUpdate = true;
                }
            }
            else if (AttackTime > 85)
                AttackTime = 0; // Reset after done
            #endregion
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Assault Drone");
            Main.npcFrameCount[npc.type] = 1;
        }
        public override void SetDefaults()
        {
            npc.dontTakeDamageFromHostiles = false;
            npc.lifeMax = 300;
            npc.defense = 25;
            npc.damage = 20;
            npc.knockBackResist = 0f;

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;

            npc.width = 44;
            npc.height = 40;

            npc.value = Item.buyPrice(0, 0, 30, 0);

            npc.MakeBuffImmune(BuffID.OnFire, BuffID.ShadowFlame, BuffID.CursedInferno, BuffID.Frostburn, BuffID.Chilled);
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<Dracocell>(), 1, 1, 55);
            Loot.DropItem(npc, mod.ItemType<MalleableShard>(), 1, 1, 20);
            Loot.DropItem(npc, ItemID.SilverCoin, 30, 50, 100, 2);
        }
    }

    public class HDracocideShield : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Newtonian Shield");
            Main.npcFrameCount[npc.type] = 2;
        }
        public override void SetDefaults()
        {
            npc.lifeMax = 1;
            npc.defense = 0;
            npc.damage = 0;
            npc.knockBackResist = 0;
            npc.immortal = true;

            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit3;
            npc.DeathSound = SoundID.NPCDeath3;

            npc.width = 56;
            npc.height = 34;

            npc.value = 0;

            npc.MakeDebuffImmune();
        }

        public override void FindFrame(int frameHeight)
        {
            if (npc.immune[Main.myPlayer] > 0) // if just took damage, flash white
                npc.frame.Y = frameHeight;
            else // otherwise it's orange.
                npc.frame.Y = 0;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            projectile.penetrate++;
            damage = 0;
            knockback = 0;
            crit = false;
            hitDirection = 0;

            if (projectile.velocity.Length() < 8)
                return;

            projectile.damage /= 3;
            projectile.Reflect(1);
            projectile.velocity = projectile.velocity.RotatedByRandom(1);
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage = 0;
            npc.immune[Main.myPlayer] = 5;

            return false;
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, mod.ItemType<MalleableShard>(), 1, 1, 25);
            Loot.DropItem(npc, mod.ItemType<Dracocell>(), 1, 1, 25);
            Loot.DropItem(npc, ItemID.SilverCoin, 30, 50, 100, 2);
        }
    }

    public class AssaultBolt : NewModProjectile
    {
        protected override int[] Dimensions => new int[] { 12, 22 };
        protected override int DustType => DustID.AmberBolt;

        protected override int Pierce => 1;
        protected override int Bounce => 0;
        protected override float Gravity => 0;

        protected override bool NoDustLight => true;
        protected override float TrailScale => 0.8f;
        protected override DamageTypes DamageType => DamageTypes.Hostile;
        protected override DustTrailTypes DustTrailType => DustTrailTypes.PerfectNoGravity;
        protected override float? Rotation => projectile.velocity.ToRotation();
        protected override bool[] DamageTeam => new bool[] { true, true };

        public override void SetDefaults()
        {
            base.SetDefaults();
            projectile.owner = Main.myPlayer;
            projectile.hostile = true;
            projectile.friendly = true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return
                !(target.modNPC != null && target.modNPC is DracocideDrone) &&
                target.type != mod.NPCType<Observer>();
        }
    }
}
