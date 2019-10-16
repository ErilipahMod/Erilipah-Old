using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Erilipah.Items.Templar
{
    public class VitalityBar : UIState
    {
        public static bool Visible => alpha > 0;
        public static float alpha;
        public static int charge;
        public static int vitality;
        public static int maxVital;

        private const int frameCount = 24;
        private int Frame => Math.Max(0, vitality / (maxVital / frameCount));
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;

            if (charge > 4) charge = 4;
            alpha = MathHelper.Clamp(alpha, 0f, 1f);
            Texture2D texture = ModContent.GetTexture("Erilipah/Items/Templar/VitalityBar" + charge);

            Color color = new Color(alpha, alpha, alpha, alpha);
            Rectangle rect = texture.Frame(1, frameCount, 0, Frame);
            Vector2 position = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f - 42);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / frameCount) * 0.5f);

            spriteBatch.Draw(texture, position, rect, color, 0, drawOrigin, 1, SpriteEffects.None, 0);
        }
    }
    public class Vitality : ModPlayer
    {
        public int CurrentVitality { get; private set; } = 0;

        private const int healTime = 150;
        public int degradeDelay = 30,
            charge = 0,
            maxVitality = 120;

        private int timer = 0, degradeTimeDelay = 0,
            healing = healTime + 1;

        public override void ResetEffects()
        {
            maxVitality = 120;
            charge = 0;
        }
        public override void UpdateLifeRegen()
        {
            if (healing <= healTime)
            {
                healing++;
                if (healing % (healTime / (75 / 15)) == 0)
                    player.Heal(15);
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Erilipah.VitalityAbilityKey.JustReleased && healing > 75 && CurrentVitality > 120 &&
                player.armor.Take(10).Any(x => x.type == ItemType<SigilOfVitality>()))
            {
                SubVitality(120);
                healing = 0;
                Main.PlaySound(SoundID.Item29, player.Center);
            }
        }
        public override void PreUpdate()
        {
            if (--degradeTimeDelay < 0)
                if (++timer % degradeDelay == 0 && CurrentVitality > 0)
                    CurrentVitality--;
            if (CurrentVitality > maxVitality)
                CurrentVitality = maxVitality;

            alpha -= alphaReductAmount;
            if (alpha < 0.2f && CurrentVitality > 0)
                alpha = 0.2f;
            if (alpha < 1f && charge > 0)
                alpha = 1f;
            VitalityBar.alpha = MathHelper.Clamp(alpha, 0f, 1f);
            VitalityBar.charge = charge;
            VitalityBar.vitality = CurrentVitality;
            VitalityBar.maxVital = maxVitality;
        }

        public void AddVitality(int amount)
        {
            degradeTimeDelay = 60;
            if (player.armor.Take(10).Any(x => x.type == ItemType<SigilOfVitality>()))
                amount += 1;

            if (amount > maxVitality - CurrentVitality)
                amount = maxVitality - CurrentVitality;
            if (amount > 0)
            {
                CombatText.NewText(player.Hitbox, Color.LightBlue, amount);
                alpha = 2;
                CurrentVitality += amount;
            }
            else
                alpha = 0.2f;
        }
        public void SubVitality(int amount)
        {
            if (amount > CurrentVitality)
                amount = CurrentVitality;
            if (amount == 0)
                return;
            alpha = 2;
            CurrentVitality -= amount;
        }

        private float alpha = 0;
        private const float alphaReductAmount = 1 / 120f;
        //public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        //{
        //    bool equippedMonumentis =
        //        PlayerHelper.HasEquip(player, mod.ItemType("Monumentis")) ||
        //        PlayerHelper.HasEquip(player, mod.ItemType("Subterrania")) ||
        //        PlayerHelper.HasEquip(player, mod.ItemType("Veritas"));
        //    bool hasMonumentis = !player.HasBuff(BuffType<Buffs.Debuffs.ReviveCooldown>()) && equippedMonumentis;
        //    if (vitality == maxVitality && PlayerHelper.HasEquip(player, ItemType<SigilOfVitality>()) && 
        //        !hasMonumentis)
        //    {
        //        if (player.HasBuff(BuffType<Buffs.Debuffs.ReviveCooldown1>()))
        //            return true;

        //        playSound = false;
        //        genGore = false;

        //        int healLife = player.statLifeMax2 / 2;
        //        int heal = Math.Min(healLife, player.statLifeMax2 - player.statLife);
        //        player.statLife = heal;

        //        player.immune = true;
        //        player.immuneTime = 180;
        //        for (int i = 0; i < player.hurtCooldowns.Length; i++)
        //        {
        //            player.hurtCooldowns[i] = 180;
        //        }

        //        player.AddBuff(BuffType<Buffs.Debuffs.ReviveCooldown1>(), 3 * 60 * 60, false);

        //        Main.PlaySound(SoundID.Item29, player.Center);
        //        player.GetModPlayer<ErilipahPlayer>().SavedText();

        //        vitality = 0;
        //        return false;
        //    }
        //    return true;
        //}

        //public void DrawBar(SpriteBatch spriteBatch)
        //{
        //    if (alpha <= 0)
        //        return;

        //    const int offsetY = 100;
        //    if (charge > 4) charge = 4;

        //    Texture2D texture = ModLoader.GetTexture("Erilipah/Items/Templar/VitalityBar" + charge);

        //    Color color = new Color(alpha, alpha, alpha, alpha);
        //    Rectangle rect = texture.Frame(1, frameCount, 0, Frame);
        //    Vector2 position = player.Center - new Vector2(0, offsetY);

        //    Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, (texture.Height / frameCount) * 0.5f);
        //    Vector2 drawPos = position - Main.screenPosition;

        //    spriteBatch.Draw(texture, drawPos, rect, color, 0, drawOrigin, 1, SpriteEffects.None, 0);
        //}
    }
    public class VitalArrow : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (Main.player[projectile.owner].HeldItem.type == mod.ItemType("TemplarsLifebow") && projectile.arrow && !target.immortal && !target.dontTakeDamage)
            {
                Vitality v = Main.player[projectile.owner].GetModPlayer<Vitality>();
                v.AddVitality(damage / 8);
            }
        }
    }
}