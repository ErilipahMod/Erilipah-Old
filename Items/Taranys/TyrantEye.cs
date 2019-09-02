using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.IO;
using Erilipah.NPCs.ErilipahBiome;

namespace Erilipah.Items.Taranys
{
    public class TyrantEye : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tyrant Eye");
            Tooltip.SetDefault("Left click to pulse, damaging and repelling enemies\nRight click to stare, degrading the targeted enemy");
        }

        public override void SetDefaults()
        {
            item.damage = 32;
            item.knockBack = 1;
            item.crit = 10;
            item.magic = true;
            item.mana = 8;
            item.noMelee = true;

            item.maxStack = 1;
            item.useTime = 22;
            item.useAnimation = 10;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.holdStyle = ItemUseStyleID.HoldingOut;
            item.autoReuse = false;
            item.UseSound = SoundID.Item103.WithPitchVariance(-0.95f).WithVolume(0.25f);

            item.width = 30;
            item.height = 42;

            item.value = item.AutoValue();
            item.rare = ItemRarityID.Blue;

            pulse = 0;
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDustPerfect(Main.MouseWorld + Vector2.UnitX.RotatedBy(i / 30f * MathHelper.TwoPi) * 50, mod.DustType<VoidParticle>())
                        .customData = (int)1;
            }
        }

        private float pulse = 0;
        public override bool UseItem(Player player)
        {
            // Stare
            if (player.altFunctionUse == 2)
            {
                item.channel = true;

                if (player.statMana < item.mana)
                    return false;
                if (Main.myPlayer != player.whoAmI)
                    return base.UseItem(player);

                Vector2 pos;
                int closestIndex = Helper.FindClosestNPC(Main.MouseWorld, 100, n => !n.friendly);
                if (closestIndex == -1)
                    pos = Main.MouseWorld;
                else
                {
                    NPC target = Main.npc[closestIndex];
                    pos = target.Center;
                    if (target.immune[player.whoAmI] <= 0)
                    {
                        target.immune[player.whoAmI] = item.useTime;
                        target.StrikeNPC(item.damage, 0, 0);
                        target.netUpdate = true;
                    }
                }

                const int perSecond = 6;
                float amount = (float)Main.time % 140 / (140f / perSecond) % 1f;
                Vector2 dustPos = Vector2.Lerp(player.Center, pos, amount);
                Dust dust = Dust.NewDustPerfect(dustPos, mod.DustType<VoidParticle>(), Vector2.Zero, Scale: 1.5f - amount);
                dust.noGravity = true;
            }
            else // Pulse outward
            {
                item.channel = false;

                const float effectiveDist = 15;
                pulse += effectiveDist;

                if (pulse < 1000)
                    player.itemAnimation = 10;

                #region Pulse
                for (int i = 0; i < pulse * 0.2f; i++)
                {
                    // Create dusts in an even ring around the NPC
                    float rotation = MathHelper.Lerp(0, MathHelper.TwoPi, i / (pulse * 0.2f));
                    Vector2 position = player.Center + Vector2.UnitX.RotatedBy(rotation) * pulse;

                    Dust dust = Dust.NewDustPerfect(position, mod.DustType<NPCs.ErilipahBiome.VoidParticle>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;

                    dust = Dust.NewDustPerfect(position, mod.DustType<Crystalline.CrystallineDust>(), Vector2.Zero);
                    dust.noGravity = true;
                    dust.velocity = Vector2.Zero;
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    float distanceToNPC = Vector2.Distance(proj.Center, player.Center);

                    if (proj.active && distanceToNPC > pulse - effectiveDist && distanceToNPC < pulse + effectiveDist && proj.hostile)
                    {
                        proj.velocity = player.Center.To(proj.Center, proj.velocity.Length());
                    }
                }

                for (int i = 0; i < 200; i++)
                {
                    NPC n = Main.npc[i];
                    float distanceToNPC = Vector2.Distance(n.Center, player.Center);
                    if (distanceToNPC > pulse - effectiveDist && distanceToNPC < pulse + effectiveDist)
                    {
                        if (n.boss)
                            n.velocity += player.Center.To(n.Center, 6 * item.knockBack * n.knockBackResist + 2);
                        else
                            n.velocity += player.Center.To(n.Center, 8 * item.knockBack * n.knockBackResist);
                        n.netUpdate = true;
                    }
                }
                #endregion
            }
            return true;
        }

        public override void NetSend(BinaryWriter writer) => writer.Write(pulse);
        public override void NetRecieve(BinaryReader reader) => pulse = reader.ReadSingle();

        public override Vector2? HoldoutOffset() => new Vector2(-4, 0);
        public override bool AltFunctionUse(Player player) => true;
    }
}
