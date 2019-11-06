using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Erilipah.NPCs
{
    public abstract class NewModNPC : ModNPC
    {
        protected int[] dimensions = new int[2];
        protected virtual int[] Dimensions => new int[0];
        protected virtual string Title => null;
        protected abstract int NPCFrameCount { get; }
        protected virtual int MaxLife { get; }
        protected virtual int Damage { get; }
        protected virtual int Defense { get; }
        protected virtual float KnockbackResist => 1;

        protected virtual int FrameDelay => 8;
        protected bool Client => Main.netMode == 1;
        protected virtual int[,] InflictBuffs => new int[0, 0];
        protected virtual int[] ImmuneToDebuff => new int[0];

        protected virtual Vector2 GoreVelocity => npc.velocity;
        protected virtual string GorePath => null;
        protected virtual int[] Gores => new int[0];

        protected virtual LegacySoundStyle HitSound => SoundID.NPCHit1;
        protected virtual LegacySoundStyle DeathSound => SoundID.NPCDeath1;
        protected virtual int AIType => 0;
        protected virtual NPCTypes NPCType => NPCTypes.Custom;
        protected enum NPCTypes
        {
            Custom, Fighter, Floater, Sentry
        }

        protected virtual bool LavaImmune => false;
        protected virtual bool NoTileCollide => false;
        protected virtual bool NoGravity => true;

        protected virtual float ScaleExpertHP => 0;
        protected virtual float ScaleExpertDmg => 2;
        protected virtual float ScaleExpertDef => 2;

        protected bool MotionBlurActive = false;
        protected virtual int MaxMotionBlurLength => 0;
        protected int MotionBlurLength = 0;

        protected virtual Player Target
        {
            get { return Main.player[npc.target]; }
            set { npc.target = value.whoAmI; }
        }
        protected Vector2 TCen => Target.Center;
        protected float TDist => Vector2.Distance(Target.Center, npc.Center);
        protected float TotalSpeed => Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y);

        public override void SetDefaults()
        {
            if (Main.npcTexture[npc.type] != null)
                dimensions = new int[] { Main.npcTexture[npc.type].Width, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] };
            if (Dimensions.Length > 0)
                dimensions = Dimensions;
            npc.value = MaxLife / 3 * (Defense / 15 + 1);

            npc.lifeMax = MaxLife;
            npc.damage = Damage;
            npc.defense = Defense;
            npc.width = dimensions[0];
            npc.height = dimensions[1];
            if (NPCType == NPCTypes.Fighter)
            {
                npc.lavaImmune = LavaImmune;
                npc.noTileCollide = false;
                npc.noGravity = false;
                npc.aiStyle = 3;
            }
            if (NPCType == NPCTypes.Floater)
            {
                npc.lavaImmune = LavaImmune;
                npc.noTileCollide = NoTileCollide;
                npc.noGravity = true;
            }
            if (NPCType == NPCTypes.Custom)
            {
                npc.lavaImmune = LavaImmune;
                npc.noTileCollide = NoTileCollide;
                npc.noGravity = NoGravity;
            }
            aiType = AIType;
            npc.knockBackResist = KnockbackResist;
            npc.friendly = false;

            npc.HitSound = HitSound;
            npc.DeathSound = DeathSound;
        }
        public override void SetStaticDefaults()
        {
            if (Title != null)
            {
                DisplayName.SetDefault(Title);
            }

            Main.npcFrameCount[npc.type] = NPCFrameCount;

            if (MaxMotionBlurLength > 0)
            {
                NPCID.Sets.TrailCacheLength[npc.type] = Math.Max(MotionBlurLength, MaxMotionBlurLength);
                NPCID.Sets.TrailingMode[npc.type] = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (MotionBlurActive && npc.velocity.Length() > 0)
            {
                Texture2D texture = Main.npcTexture[npc.type];

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.npcFrameCount[npc.type] * 0.5f);
                if (MotionBlurLength > MaxMotionBlurLength)
                {
                    MotionBlurLength = MaxMotionBlurLength;
                }
                for (int i = 0; i < Math.Min(MotionBlurLength, npc.oldPos.Length); i++)
                {
                    Vector2 drawPos = npc.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0, npc.gfxOffY);
                    Color color = npc.GetAlpha(drawColor) * ((MotionBlurLength - i) / (float)MotionBlurLength);
                    SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        texture: texture, position: drawPos, sourceRectangle: npc.frame, color: color, rotation: npc.rotation,
                        origin: drawOrigin, scale: npc.scale, effects: effects, layerDepth: 0
                        );
                }
            }
            return true;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (InflictBuffs != null)
            {
                for (int a = 0; a < InflictBuffs.GetLength(0); a++)
                {
                    target.AddBuff(InflictBuffs[a, 0], InflictBuffs[a, 1]);
                }
            }
        }
        public sealed override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            if (ScaleExpertHP <= 0)
                npc.lifeMax = (int)(npc.lifeMax * bossLifeScale) / 2;
            else
                npc.lifeMax = (int)(npc.lifeMax * ScaleExpertHP) / 2;
            npc.damage = (int)(npc.damage * ScaleExpertDmg) / 2;
            npc.defense = (int)(npc.defense * ScaleExpertDef) / 2;
        }

        protected enum AnimationTypes
        {
            AutoCycle, ReturnFrameNum
        }
        protected virtual int Animate(int frameHeight)
        {
            return -1;
        }
        public override void FindFrame(int frameHeight)
        {
            int a = Animate(frameHeight);
            if (a == (int)AnimationTypes.AutoCycle)
            {
                if (++npc.frameCounter % FrameDelay == 0)
                {
                    int frame = npc.frame.Y / frameHeight + 1;
                    int Frame = frame % NPCFrameCount;
                    npc.frame.Y = Frame * frameHeight;
                }
            }
            if (a > (int)AnimationTypes.ReturnFrameNum)
            {
                npc.frame.Y = a * frameHeight;
            }
        }
        protected virtual void OnKill(int hitDirection, double damage)
        {
            SpawnGore();
            MotionBlurActive = false;
        }
        protected virtual void OnHit(int hitDirection, double damage, bool killed)
        {

        }
        protected void Kill(int hitDirection = 0, double dmg = 0)
        {
            npc.life = 0;
            HitEffect(hitDirection, dmg);
            Main.PlaySound(npc.DeathSound, npc.Center);
            if (PreNPCLoot() & !SpecialNPCLoot())
                NPCLoot();
        }
        protected virtual void SpawnGore()
        {
            if (GorePath != null)
            {
                for (int i = 0; i < Gores.Length; i++)
                {
                    for (int j = 0; j < Gores[i]; j++)
                    {
                        int goreNum = i + 1;
                        Gore.NewGore(npc.Center, GoreVelocity, mod.GetGoreSlot(GorePath + goreNum.ToString()));
                    }
                }
            }
        }
        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
                OnKill(hitDirection, damage);
            }
            OnHit(hitDirection, damage, npc.life <= 0);
        }
    }
}