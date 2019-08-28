using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Erilipah
{
    public partial class Erilipah : Mod
    {
        public const string Copper = "Erilipah:CopperBars";
        public const string Silver = "Erilipah:SilverBars";
        public const string Gold = "Erilipah:GoldBars";
        public const string Evil = "Erilipah:EvilBars";
        public const string Gem = "Erilipah:AnyGem";

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(this);
            #region Guns
            r.AddRecipeGroup("Wood", 30);
            r.AddRecipeGroup("IronBar", 10);
            r.AddIngredient(ItemType("GunMold"));
            r.AddTile(TileID.Anvils);
            r.SetResult(ItemID.FlintlockPistol);
            r.AddRecipe();

            r = new ModRecipe(this);
            r.AddIngredient(ItemID.IllegalGunParts, 1);
            r.AddIngredient(ItemID.SharkFin, 3);
            r.AddRecipeGroup("Erilipah:GoldBars", 10);
            r.AddIngredient(ItemType("GunMold"));
            r.AddTile(TileID.Anvils);
            r.SetResult(ItemID.Minishark);
            r.AddRecipe();

            r = new ModRecipe(this);
            r.AddIngredient(ItemID.CrimtaneBar, 10);
            r.AddIngredient(ItemType("GunMold"));
            r.AddTile(TileID.Anvils);
            r.SetResult(ItemID.TheUndertaker);
            r.AddRecipe();

            r = new ModRecipe(this);
            r.AddIngredient(ItemID.DemoniteBar, 10);
            r.AddIngredient(ItemType("GunMold"));
            r.AddTile(TileID.Anvils);
            r.SetResult(ItemID.Musket);
            r.AddRecipe();

            r = new ModRecipe(this);
            r.AddIngredient(ItemID.Bone, 300);
            r.AddRecipeGroup("Erilipah:SilverBars", 8);
            r.AddIngredient(ItemID.IllegalGunParts);
            r.AddIngredient(ItemType("GunMold"));
            r.AddTile(TileID.Anvils);
            r.SetResult(ItemID.Handgun);
            r.AddRecipe();

            r = new ModRecipe(this);
            r.AddIngredient(ItemID.RichMahogany, 50);
            r.AddIngredient(ItemID.Vine, 3);
            r.AddIngredient(ItemID.JungleSpores, 5);
            r.AddIngredient(ItemType("GunMold"));
            r.AddTile(TileID.Anvils);
            r.SetResult(ItemID.Boomstick);
            r.AddRecipe();
            #endregion
        }
        public override void AddRecipeGroups()
        {
#pragma warning disable CS0618
            string any = Language.GetText("Any").Value;
#pragma warning restore CS0618

            RecipeGroup copperBars = new RecipeGroup(() => any + " copper bar", new int[]
            {
                ItemID.CopperBar,
                ItemID.TinBar
            });
            RecipeGroup.RegisterGroup("Erilipah:CopperBars", copperBars);

            RecipeGroup silverbars = new RecipeGroup(() => any + " silver bar", new int[]
            {
                ItemID.SilverBar,
                ItemID.TungstenBar,
            });
            RecipeGroup.RegisterGroup("Erilipah:SilverBars", silverbars);

            RecipeGroup goldbars = new RecipeGroup(() => any + " gold bar", new int[]
            {
                ItemID.GoldBar,
                ItemID.PlatinumBar
            });
            RecipeGroup.RegisterGroup("Erilipah:GoldBars", goldbars);

            RecipeGroup evilbars = new RecipeGroup(() => any + " evil bar", new int[]
            {
                ItemID.DemoniteBar,
                ItemID.CrimtaneBar,
            });
            RecipeGroup.RegisterGroup("Erilipah:EvilBars", evilbars);

            RecipeGroup anygem = new RecipeGroup(() => any + " gemstone", new int[]
            {
                ItemID.Diamond,
                ItemID.Amethyst,
                ItemID.Topaz,
                ItemID.Sapphire,
                ItemID.Emerald,
                ItemID.Ruby,
                ItemID.Amber
            });
            RecipeGroup.RegisterGroup("Erilipah:AnyGem", anygem);
        }

    }
}
