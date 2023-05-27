﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tsorcRevamp.Items.Armors
{
    [AutoloadEquip(EquipType.Body)]
    public class ShadowCloakPlateMail : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.defense = 9;
            Item.rare = ItemRarityID.Orange;
            Item.value = PriceByRarity.fromItem(Item);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.08f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ModContent.ItemType<ShadowCloakPlateHelm>() && legs.type == ModContent.ItemType<ShadowCloakGreaves>();
        }
        public override void UpdateArmorSet(Player player)
        {
            if (player.statLife <= (player.statLifeMax2 / 2))
            {
                player.panic = true;

                int dust = Dust.NewDust(new Vector2((float)player.position.X, (float)player.position.Y), player.width, player.height, 21, (player.velocity.X) + (player.direction * 1), player.velocity.Y, 245, Color.Violet, 2.0f);
                Main.dust[dust].noGravity = true;
            }

            if (Main.rand.NextBool(7))
            {
                int dust2 = Dust.NewDust(new Vector2((float)player.position.X, (float)player.position.Y), player.width, player.height, 21, (player.velocity.X) + (player.direction * 1), player.velocity.Y, 200, Color.Violet, 1.0f);
                Main.dust[dust2].noGravity = true;

                Main.dust[dust2].noGravity = true;
            }

            player.hasJumpOption_Santank = true; //so it stacks with commonly used Cloud in a Bottle
            player.shadowArmor = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ShadowScalemail);
            recipe.AddIngredient(ModContent.ItemType<DarkSoul>(), 2000);
            recipe.AddTile(TileID.DemonAltar);
            
            recipe.Register();
        }
    }
}