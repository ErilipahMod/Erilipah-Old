using Erilipah.Items.ErilipahBiome;
using Erilipah.Items.Taranys;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Erilipah
{
    public class ErilipahPlayer : ModPlayer
    {
        public bool ZoneErilipah { get; private set; } = false;
        public bool ZoneLostCity { get; private set; } = false;

        public int bankedDamage = 0;
        public int extraItemReach = 0;
        public bool canMove = true;
        public bool canJump = true;
        public bool soulBankHealing = false;

        private bool pressedSoulBank = false;

        public override void ResetEffects()
        {
            extraItemReach = 0;
            canMove = true;
            canJump = true;
        }

        private void AutoHeal(int damage)
        {
            if (player.FindEquip(mod.ItemType<SoulHearth>()) == null)
                return;

            if (bankedDamage <= 0)
            {
                if (!pressedSoulBank)
                    CombatText.NewText(player.getRect(), Main.errorColor, "Empty bank", true);
                pressedSoulBank = true;
            }
            else
            {
                bool highStored = bankedDamage > 400 && player.statLife < player.statLifeMax2;
                bool highDamage = damage > player.statLifeMax2 * 0.25;
                bool halfLife = player.statLife < player.statLifeMax2 * 0.5;
                bool quarterLife = player.statLife < player.statLifeMax2 * 0.25;

                if (bankedDamage > 100 && halfLife || highDamage || highStored || quarterLife)
                {
                    soulBankHealing = true;
                }
            }
        }
        private void TorchOfSoul(NPC target, int damage)
        {
            Item i = player.FindEquip(mod.ItemType<TorchOfSoul>());
            Item i2 = player.FindEquip(mod.ItemType<SoulHearth>());

            if (i == null && i2 == null || target.immortal || target.dontTakeDamage || soulBankHealing)
                return;

            if (i2 != null)
            {
                AutoHeal(0);
                if (bankedDamage >= 500)
                {
                    bankedDamage = 500;
                    return;
                }
            }
            else if (bankedDamage >= 200)
            {
                bankedDamage = 200;
                return;
            }

            // Prevent from gaining > npc life in benefits
            int amount = Math.Min(target.life + damage, damage) / 6;
            if (i2 != null)
                amount = Math.Min(target.life + damage, damage) / 4;
            if (amount < 1)
                return;

            bankedDamage += amount;
            Rectangle loc = player.getRect();
            loc.Y -= 30;
            CombatText.NewText(loc, new Color(247, 202, 166), amount);
        }

        public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (player.statLife == 1 && player.HasBuff(mod.BuffType<Items.Niter.NiterPotionBuff>()))
            {
                Dust d = Main.dust[Dust.NewDust(player.position, player.width, player.height, mod.DustType<Items.Niter.NiterDust>(), Scale: 0.75f)];
                d.velocity = new Vector2(0, -0.25f);
            }
        }

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            TorchOfSoul(target, damage);
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            TorchOfSoul(target, damage);
        }

        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            AutoHeal((int)damage);
        }

        public override void UpdateLifeRegen()
        {
            if (soulBankHealing)
            {
                if (player.statLife >= player.statLifeMax2)
                {
                    soulBankHealing = false;
                    CombatText.NewText(player.getRect(), Main.errorColor, "Full life");
                    return;
                }
                if (bankedDamage <= 0)
                {
                    soulBankHealing = false;
                    CombatText.NewText(player.getRect(), Main.errorColor, "Empty bank", true);
                    return;
                }

                if (Main.myPlayer == player.whoAmI && (int)Main.time % 5 == 0)
                {
                    player.netLife = true;

                    if ((int)Main.time % 25 == 0)
                        CombatText.NewText(player.getRect(), CombatText.HealLife, 5, false, true);
                    player.statLife++;
                    bankedDamage--;
                }
            }
        }
        public override void UpdateBadLifeRegen()
        {
            if (player.HasBuff(mod.BuffType<Items.Niter.NiterPotionBuff>()))
            {
                player.lifeRegenTime = 0;
                if (player.lifeRegen > 0)
                    player.lifeRegen = 0;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (player.FindEquip(mod.ItemType<TorchOfSoul>()) != null && Erilipah.SoulBank.JustReleased)
            {
                if (player.statLife >= player.statLifeMax2)
                {
                    if (!pressedSoulBank)
                        CombatText.NewText(player.getRect(), Main.errorColor, "Full life");
                    pressedSoulBank = true;
                }
                else if (bankedDamage <= 0)
                {
                    if (!pressedSoulBank)
                        CombatText.NewText(player.getRect(), Main.errorColor, "Empty bank", true);
                    pressedSoulBank = true;
                }
                else
                {
                    Main.PlaySound(SoundID.Item4, player.Center);
                    soulBankHealing = !soulBankHealing;
                    pressedSoulBank = false;
                }
            }
        }
        public override void SetControls()
        {
            if (!canMove)
            {
                player.controlDown = player.controlUp = player.controlLeft =
                    player.controlRight = player.controlJump = player.controlMount = false;
                player.mount.Dismount(player);
            }
            if (!canJump)
            {
                player.controlJump = false;
                player.mount.Dismount(player);
            }
        }

        public override void UpdateBiomes()
        {
            ZoneLostCity = ErilipahWorld.lostCityTiles > 35;
            ZoneErilipah = ErilipahWorld.erilipahTiles > 35 || ZoneLostCity;
        }
        public override bool CustomBiomesMatch(Player other)
        {
            var difModPlr = other.GetModPlayer<ErilipahPlayer>();
            return ZoneErilipah == difModPlr.ZoneErilipah && ZoneLostCity == difModPlr.ZoneLostCity;
        }
        public override void CopyCustomBiomesTo(Player other)
        {
            ErilipahPlayer difModPlr = other.GetModPlayer<ErilipahPlayer>();
            difModPlr.ZoneErilipah = ZoneErilipah;
            difModPlr.ZoneLostCity = ZoneLostCity;
        }
        public override void SendCustomBiomes(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = ZoneErilipah;
            flags[1] = ZoneLostCity;
            writer.Write(flags);
        }
        public override void ReceiveCustomBiomes(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            ZoneErilipah = flags[0];
            ZoneLostCity = flags[1];
        }
        public override void UpdateBiomeVisuals()
        {
            player.ManageSpecialBiomeVisuals("Erilipah:ErilipahBiome", ZoneErilipah, player.Center);
        }
        //public override Texture2D GetMapBackgroundImage()
        //{
        //    if (ZoneExample)
        //    {
        //        return mod.GetTexture("ExampleBiomeMapBackground");
        //    }
        //    return null;
        //}
    }
}
