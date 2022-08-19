﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tsorcRevamp.Items.Accessories.Expert
{
    public class CrackedDragonStone : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("[c/ffbf00:Provides immunity to catching on fire, burning, knockback, and fire blocks]" +
                                "\nRaises damage dealt by 3%");
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.value = PriceByRarity.LightRed_4;
            Item.expert = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.03f;
            player.noKnockback = true;
            player.fireWalk = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;

        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CobaltBar, 3);
            recipe.AddIngredient(ModContent.ItemType<DarkSoul>(), 6300);
            recipe.AddTile(TileID.DemonAltar);

            recipe.Register();
        }


    }
}
