using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.NPCs.ErilipahBiome
{
    public class LurkerHead : ModNPC
    {
        private const float LookAtRange = 0.2f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lurker");
            Main.npcFrameCount[npc.type] = 1;
            NPCID.Sets.TrailingMode[npc.type] = 0;
            NPCID.Sets.TrailCacheLength[npc.type] = 3;
        }

        public override void SetDefaults()
        {
            npc.lifeMax = Main.hardMode ? 400 : 250;
            npc.defense = Main.hardMode ? 200 : 100;
            npc.damage =  Main.hardMode ? 50 : 30;
            npc.knockBackResist = 0f;
            npc.SetInfecting(7);

            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.noTileCollide = true;

            npc.HitSound = SoundID.NPCHit49;
            npc.DeathSound = SoundID.NPCDeath51;

            npc.width = 36;
            npc.height = 30;

            npc.value = 752;

            BodyPartsAlive = -1;
            LookingAtMe = 255;

            // npc.buffImmune[BuffID.OnFire] = true;
        }

        private Player Target => Main.player[npc.target];
        private bool AnyoneLooking => LookingAtMe < 255;
        
        private int BodyPartsAlive { get => (int)npc.ai[0]; set => npc.ai[0] = value; }
        private int LookingAtMe { get => (int)npc.ai[1]; set => npc.ai[1] = value; }

        public override void AI()
        {
            if (BodyPartsAlive == 0)
            {
                SpawnBody();
                LookingAtMe = 255;
                BodyPartsAlive = 4;
                npc.netUpdate = true;
            }

            if (npc.target == 255)
            {
                npc.netUpdate = true;
                npc.target = npc.FindClosestPlayer();
            }

            // Record Lurker, finish rest of NPCS
            if (Main.rand.NextFloat(npc.velocity.Length()) > 1)
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, 
                    DustType<Biomes.ErilipahBiome.Hazards.FlowerDust>(), Scale: 1.2f);
                d.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * -2;
                d.noGravity = true;
            }

            if (BodyPartsAlive > 1)
                UpdateLookingState();
            else
                LookingAtMe = 255; // Rush to the player if we're just a head
            UpdateLookingEffects();
            UpdateDefense();
            UpdateMovement();
        }

        private void UpdateDefense()
        {
            switch (BodyPartsAlive)
            {
                default: break;
                case 4: npc.defense = 40; break;
                case 3: npc.defense = 30; break;
                case 2: npc.defense = 20; break;
                case 1: npc.defense = 08; break;
            }
            if (Main.expertMode)
                npc.defense = (int)(npc.defense * 1.5f);
            if (Main.hardMode)
                npc.defense *= 2;
        }

        private void UpdateLookingState()
        {
            float myMouseRot = (Main.MouseWorld - Main.LocalPlayer.Center).SafeNormalize(Vector2.Zero).ToRotation();
            float myFacingRot = (npc.Center - Main.LocalPlayer.Center).SafeNormalize(Vector2.Zero).ToRotation();
            bool iCanHitNPC = Collision.CanHitLine(npc.Center, 1, 1, Main.LocalPlayer.Center, 1, 1);
            bool myMouseFacingNPC = Helper.AngleWithinRange(myMouseRot, myFacingRot, LookAtRange);

            // IF   this client's mouse is looking at the npc 
            // AND  if nobody is already looking at it 
            if (myMouseFacingNPC && iCanHitNPC && !AnyoneLooking)
            {
                //  Then say that this client is looking at the NPC
                npc.netUpdate = true;
                npc.target = Main.myPlayer;
                LookingAtMe = Main.myPlayer;
            }
            // IF   this client's not looking at the NPC 
            // AND  if we are the one who was looking
            else if ((!myMouseFacingNPC || !iCanHitNPC) && Main.myPlayer == LookingAtMe)
            {
                npc.netUpdate = true;
                LookingAtMe = 255;
            }
        }

        private void UpdateLookingEffects()
        {
            if (AnyoneLooking)
            {
                npc.alpha += 2 * (5 - BodyPartsAlive);

                if (npc.alpha < 50)
                    npc.alpha = 50;
                if (npc.alpha > 230)
                    npc.alpha = 230;
            }
            else
            {
                npc.alpha -= 5;

                if (npc.alpha < 0)
                    npc.alpha = 0;
            }

            if (npc.alpha >= 165)
            {
                npc.dontTakeDamage = true;
            }
            else
            {
                npc.dontTakeDamage = false;
            }
        }

        private void UpdateMovement()
        {
            float speed = 1.25f * Math.Min(3.25f, 5 - BodyPartsAlive);
            speed *= (255 - npc.alpha) / 255f;
            float distance = Vector2.Distance(npc.Center, Target.Center);

            if (distance > 600)
                speed = distance / 300 + 1.5f;
            if (!Main.expertMode)
                speed *= 0.75f;

            npc.velocity = npc.DirectionTo(Target.Center) * speed;

            float toTargetRot = (Target.Center - npc.Center).ToRotation();
            npc.rotation = toTargetRot - (npc.direction == -1 ? MathHelper.Pi : 0);

            npc.direction = npc.spriteDirection = (Target.Center.X > npc.Center.X) ? 1 : -1;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => npc.alpha < 200;

        private void SpawnBody()
        {
            int[] parts = { NPCType<LurkerBackArm>(), NPCType<LurkerForeArm>(), NPCType<LurkerSpine>() };

            for (int i = 0; i < 3; i++)
            {
                NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, parts[i], 0, npc.whoAmI);
            }
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            Vector2 eye = npc.GetSpritePosition(12, 16);

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(eye, DustID.PurpleCrystalShard, new Vector2(4 * hitDirection, Main.rand.NextFloat(-3, 3))); 
            }

            if (npc.life <= 0)
            {
                Gore.NewGore(npc.Center, npc.velocity, mod.GetGoreSlot("Gores/ERBiome/LurkerHead"));
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return Main.hardMode && spawnInfo.player.InErilipah() ? 0.045f : 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawTrail(spriteBatch, 4 - BodyPartsAlive, Color.White * 0.5f);
            npc.DrawNPC(spriteBatch, Color.White * 0.75f);
            return false;
        }
    }

    public abstract class LurkerPart : ModNPC
    {
        protected NPC Head => Main.npc[(int)npc.ai[0]];
        protected Player Target => Main.player[Head.target];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lurker");
            Main.npcFrameCount[npc.type] = 1;
            NPCID.Sets.TrailingMode[npc.type] = 0;
            NPCID.Sets.TrailCacheLength[npc.type] = 3;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.noTileCollide = true;

            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.NPCDeath52;

            npc.value = 0;
        }

        public override void AI()
        {
            if (!Head.active)
            {
                npc.life = 0;
                npc.HitEffect();
            }

            npc.target = Head.target;
            npc.dontTakeDamage = Head.dontTakeDamage;
            npc.direction = npc.spriteDirection = Head.spriteDirection;
            npc.alpha = Head.alpha;

            if (npc.alpha > 35)
                npc.velocity /= 3;
            else
                npc.velocity *= 0.82f;
        }

        public override bool CheckDead()
        {
            Head.ai[0]--;
            return true;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.DrawTrail(spriteBatch, 4 - (int)Head.ai[0], Color.White * 0.35f);
            npc.DrawNPC(spriteBatch, Color.White * 0.5f);
            return false;
        }

        public override sealed bool CanHitPlayer(Player target, ref int cooldownSlot) => npc.alpha < 200;
    }

    public class LurkerSpine : LurkerPart
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            npc.lifeMax = Main.hardMode ? 200 : 150;
            npc.defense = Main.hardMode ? 20 : 12;
            npc.damage = Main.hardMode ? 25 : 15;
            npc.knockBackResist = 0.1f;
            npc.SetInfecting(5);

            npc.width = 36;
            npc.height = 62;
        }

        private Vector2 BodyPos => Head.Center + new Vector2(-6 * npc.direction, 22);

        public override void AI()
        {
            base.AI();

            npc.Center = Vector2.Lerp(npc.Center, BodyPos, 0.2f);
            npc.rotation = Head.velocity.X / 10;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                Gore.NewGore(npc.Center, (npc.Center - BodyPos) * 0.2f, mod.GetGoreSlot("Gores/ERBiome/LurkerSpine"));
            }
        }
    }

    public class LurkerBackArm : LurkerPart
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            npc.lifeMax = Main.hardMode ? 120 : 80;
            npc.defense = Main.hardMode ? 6 : 0;
            npc.damage = Main.hardMode ? 35 : 22;
            npc.knockBackResist = 0.3f;
            npc.SetInfecting(7);

            npc.width = 38;
            npc.height = 30;
        }

        private Vector2 BodyPos => Head.Center + new Vector2(-28 * npc.direction, 30);

        public override void AI()
        {
            base.AI();

            Vector2 toPlayerDir = npc.DirectionTo(Target.Center);
            npc.Center = Vector2.Lerp(npc.Center, BodyPos + toPlayerDir * 20, 0.2f);
            npc.rotation = toPlayerDir.ToRotation() - (npc.direction == -1 ? MathHelper.Pi : 0);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                Gore.NewGore(npc.Center, (npc.Center - BodyPos) * 0.2f, mod.GetGoreSlot("Gores/ERBiome/LurkerLArm"));
            }
        }
    }

    public class LurkerForeArm : LurkerPart
    {
        public override void SetDefaults()
        {
            base.SetDefaults();

            npc.lifeMax = Main.hardMode ? 120 : 80;
            npc.defense = Main.hardMode ? 6 : 0;
            npc.damage = Main.hardMode ? 35 : 22;
            npc.knockBackResist = 0.3f;
            npc.SetInfecting(7);

            npc.width = 38;
            npc.height = 20;
        }

        private float ReachFactor => 20 * MathHelper.Clamp(1 - npc.Distance(Target.Center) / 300f, 0, 1);
        private Vector2 ReachPos  => ReachFactor * npc.DirectionTo(Target.Center);
        private Vector2 BodyPos   => Head.Center + new Vector2(18 * npc.direction, 30) + ReachPos;

        public override void AI()
        {
            base.AI();

            Vector2 toPlayerDir = npc.DirectionTo(Target.Center);
            npc.Center = Vector2.Lerp(npc.Center, BodyPos + toPlayerDir * 20, 0.2f);
            npc.rotation = toPlayerDir.ToRotation() - (npc.direction == -1 ? MathHelper.Pi : 0);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                Gore.NewGore(npc.Center, (npc.Center - BodyPos) * 0.2f, mod.GetGoreSlot("Gores/ERBiome/LurkerLArm"));
            }
        }
    }
}
