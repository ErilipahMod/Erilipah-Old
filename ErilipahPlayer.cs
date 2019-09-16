using Erilipah.Items.Taranys;
using System;
using System.IO;
using Microsoft.Xna.Framework;
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
        public bool healingSoulTorch = false;

        private bool pressedSoulBank = false;

        public override void ResetEffects()
        {
            extraItemReach = 0;
            canMove = true;
            canJump = true;
        }

        void TorchOfSoul(NPC target, int damage)
        {
            if (bankedDamage >= 200)
            {
                bankedDamage = 200;
                return;
            }
            if (healingSoulTorch) return;

            Item i = player.FindEquip(mod.ItemType<TorchOfSoul>());
            if (i != null && !target.immortal && !target.dontTakeDamage)
            {
                int amount = damage / 5;
                if (amount < 1)
                    return;

                bankedDamage += amount;
                if (bankedDamage > 500)
                {
                    bankedDamage = 500;
                    amount = 500 - amount;
                }

                Rectangle loc = player.getRect();
                loc.Y -= 30;
                CombatText.NewText(loc, new Color(247, 202, 166), amount);
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

        public override void UpdateLifeRegen()
        {
            if (healingSoulTorch)
            {
                if (player.statLife >= player.statLifeMax2)
                {
                    player.GetModPlayer<ErilipahPlayer>().healingSoulTorch = false;
                    CombatText.NewText(player.getRect(), Main.errorColor, "Full life");
                    return;
                }
                if (bankedDamage == 0)
                {
                    player.GetModPlayer<ErilipahPlayer>().healingSoulTorch = false;
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
                else if (bankedDamage == 0)
                {
                    if (!pressedSoulBank)
                        CombatText.NewText(player.getRect(), Main.errorColor, "Empty bank", true);
                    pressedSoulBank = true;
                }
                else
                {
                    Main.PlaySound(SoundID.Item4, player.Center);
                    healingSoulTorch = !healingSoulTorch;
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
