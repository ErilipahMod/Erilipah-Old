using Erilipah.Items.Dracocide;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Erilipah.NPCs.Dracocide
{
    public class ArcCaster : DracocideDrone
    {
        // all other NPCs of this type are friends!
        private List<NPC> Friends => Main.npc.Where(x => x.active && x.type == npc.type && x.whoAmI != npc.whoAmI && x.ai[1] == npc.ai[1]).ToList();

        private float dustSpawnPos;

        public override void AI()
        {
            fuckingRun = Friends.Count == 0;
            if (!Base())
                return;

            // If there IS a target and a friend, then target a position 200 pixels away from the player.
            Vector2 goTo = Target.Center + Vector2.UnitY * -300;

            // Find all NPCs of my type and order them by whoAmI.
            List<NPC> likeMe = Main.npc.Where(x => x.active && x.type == npc.type && x.ai[1] == npc.ai[1]).OrderBy(x => x.whoAmI).ToList();
            float myIndex = likeMe.IndexOf(npc);

            // Find how far on a circle this NPC should be rotated by, in proportion to its index in the list.
            float rotatedBy = MathHelper.Lerp(0, MathHelper.TwoPi, myIndex / likeMe.Count);

            // Pivot around the Target for where this NPC should go.
            goTo = goTo.RotatedBy(rotatedBy, Target.Center);

            // Now go there!
            npc.velocity = npc.GoTo(goTo, 0.3f);
            npc.velocity = Vector2.Clamp(npc.velocity, Vector2.One * -6, Vector2.One * 6);

            // Also set up the "arcs," which are just Collision checks. If the player is colliding with a line, hurt him.
            foreach (var friend in Friends)
            {
                // Is target colliding with a line?
                bool hittingLine = Collision.CheckAABBvLineCollision(
                Target.position,
                new Vector2(Target.Hitbox.Width, Target.Hitbox.Height),
                npc.Center,
                friend.Center);

                if (hittingLine && Target is Player tPlayer) // If it's a yes and the target is a player, run that player's hurt code.
                {
                    tPlayer.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), npc.damage - 10, 0);
                    tPlayer.immune = true;
                }

                else if (hittingLine && Target is NPC tNPC && tNPC.immune[255] <= 0) // Otherwise (and if yes), run the NPC's code. 
                {
                    tNPC.StrikeNPC(npc.damage, 2f, 0);
                    tNPC.immune[255] = 15;
                }

                // Zoop
                dustSpawnPos += Main.rand.NextFloat() / 20f;
                dustSpawnPos %= 1f;

                Vector2 dir = (friend.Center - npc.Center).SafeNormalize(Vector2.Zero) * 38;

                Dust dust = Dust.NewDustPerfect(
                    Vector2.Lerp(npc.Center + dir, friend.Center - dir, dustSpawnPos),
                    DustType<DracocideDust>());
                dust.noLight = true;
                dust.velocity = Vector2.Zero;
                dust.customData = -20;
                dust.fadeIn = 10;
            }

            // Set the NPC's rotation between its other friends.
            if (Friends.Count == 1) // if only one, just face it.
                npc.rotation = (Friends[0].Center - npc.Center).ToRotation() + MathHelper.PiOver2;
            else if (myIndex == 1)
                npc.rotation = MathHelper.Lerp(Friends[0].rotation, Friends[1].rotation, 0.5f) + MathHelper.Pi;
            else // otherwise, average.
                npc.rotation = Friends.Average(x => (x.Center - npc.Center).ToRotation()) + MathHelper.PiOver2;
        }

        public override void NPCLoot()
        {
            Loot.DropItem(npc, ItemType<Dracocell>(), 1, 1, 30);
            Loot.DropItem(npc, ItemType<ArcJoint>(), 1, 1, 15);
            Loot.DropItem(npc, ItemID.SilverCoin, 30, 50, 100, 2);
        }

        // Animation
        public override void FindFrame(int frameHeight) => npc.Animate(frameHeight, 4, 4);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults()
        {
            npc.dontTakeDamageFromHostiles = false;
            npc.lifeMax = 200;
            npc.defense = 30;
            npc.damage = 32;
            npc.knockBackResist = 0f;

            npc.noTileCollide = true;
            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.HitSound = SoundID.NPCHit4;
            npc.DeathSound = SoundID.NPCDeath14;
            // SoundID.NPCHit4 metal
            // SoundID.NPCDeath14 grenade explosion

            npc.width = 40;
            npc.height = 64;

            npc.value = Item.buyPrice(0, 0, 30, 0);

            npc.MakeBuffImmune(BuffID.OnFire, BuffID.ShadowFlame, BuffID.CursedInferno, BuffID.Frostburn, BuffID.Chilled);
        }
    }
}
