#define BETA
#if BETA
using Terraria;
using Terraria.ModLoader;

namespace Erilipah.Items.Accessories
{
    public class TrailBlazers : ModItem
    {
        public override void SetDefaults()
        {
            item.rare = 10;
            item.width = 42;
            item.height = 38;
            item.value = Item.buyPrice(20, 0, 0, 0);
            item.accessory = true;
        }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault(
                  "\nDramatically increased flight time, scary fast running, and extra mobility on ice"
                + "\nDramatically increased movement speed"
                + "\nProvides the ability to walk on water and lava"
                + "\nGrants immunity to fire blocks, On Fire and lava"
                + "\nGrants immunity to almost all debuffs and knockback"
                + "\nGives a chance to dodge attacks"
                + "\nGrants the holder werewolf-like abilities and turns them into a merfolk when entering water"
                + "\nDoubled max mana, immune to mana sickness, restores mana when damaged"
                + "\n8% reduced mana usage, automatic drinking of mana potions, increased pickup range for stars"
                + "\nNoticeable increases to all stats and negated fall damage"
                + "\nImmense regen boosts"
                + "\nEnhanced Shield of Cthulhu effect"
                + "\nGrants 8 defense and damage reduction increased by 20%"
                + "\n'The epitome of purity, these boots have been in every Erilipah version.'"
                + "\n[c/9999ff:Thank you for everything.]"
                + "\n[c/00ffff:Enjoy.]");
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.noKnockback = true;
            player.statManaMax2 *= (int)2.2;
            player.waterWalk = true;
            player.fireWalk = true;
            player.wingTimeMax *= 3;
            player.lavaImmune = true;
            player.iceSkate = true;
            player.accMerman = true;
            player.moveSpeed += 1f;
            player.maxRunSpeed *= 7f;
            if (player.controlLeft)
            {
                if (player.velocity.X > -4) player.velocity.X -= 0.2f;
                if (player.velocity.X < -6) player.velocity.X += player.velocity.X / 120f;
                if (player.velocity.X > 10) player.velocity.X -= player.velocity.X / 60f;
            }
            if (player.controlRight)
            {
                if (player.velocity.X < 4) player.velocity.X += 0.2f;
                if (player.velocity.X > 6) player.velocity.X += player.velocity.X / 120f;
                if (player.velocity.X < -10) player.velocity.X -= player.velocity.X / 60f;
            }
            player.meleeSpeed += 0.251f;
            player.allDamage += 0.20f;
            player.magicCrit += 4;
            player.meleeCrit += 6;
            player.rangedCrit += 4;
            player.thrownCrit += 4;
            player.statDefense += 20;
            player.pickSpeed = 1.35f;
            player.lifeRegen += 7;
            player.jumpSpeedBoost += 2.3f;
            for (int i = 0; i < player.CountBuffs(); i++) // immune to all debuffs.
            {
                if (player.buffType[i] != Terraria.ID.BuffID.Horrified && Main.debuff[player.buffType[i]])
                    player.buffImmune[player.buffTime[i]] = true;
            }
            player.blackBelt = true;
            player.manaFlower = true;
            player.magicCuffs = true;
            player.manaMagnet = true;
            player.magicQuiver = true;
            player.noFallDmg = true;
            player.dash = 3;
            player.endurance += 0.20f;
            if (player.controlLeft)
            {
                player.lifeRegen += 5;
            }
            if (player.controlRight)
            {
                player.lifeRegen += 5;
            }
            else
            {
                player.lifeRegen += 15;
            }
        }
    }
}
#endif