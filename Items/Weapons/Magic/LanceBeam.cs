﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using tsorcRevamp.Items.Materials;

namespace tsorcRevamp.Items.Weapons.Magic
{
    public class LanceBeam : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Channel a storm of razor-sharp glowing shards which shred enemy defense");
        }

        public override void SetDefaults()
        {

            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTurn = true;
            Item.useAnimation = 5;
            Item.useTime = 5;
            Item.maxStack = 1;
            Item.damage = 45;
            Item.autoReuse = true;
            Item.knockBack = (float)4;
            Item.scale = (float)1;
            Item.UseSound = SoundID.Item34;
            Item.rare = ItemRarityID.Red;
            Item.shootSpeed = (float)20;
            Item.crit = 2;
            Item.mana = 10;
            Item.noMelee = true;
            Item.value = PriceByRarity.Red_10;
            Item.DamageType = DamageClass.Magic;
            Item.channel = true;

            Item.shoot = ModContent.ProjectileType<Projectiles.Magic.LanceBeamLaser>();
        }

        //Only one allowed at a time
        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Magic.LanceBeamLaser>()] == 0)
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void HoldItem(Player player)
        {           

        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrystalShard, 1);
            recipe.AddIngredient(ModContent.ItemType<CursedSoul>(), 20);
            recipe.AddIngredient(ModContent.ItemType<WhiteTitanite>(),5);
            recipe.AddIngredient(ModContent.ItemType<GhostWyvernSoul>(), 1);
            recipe.AddIngredient(ModContent.ItemType<DarkSoul>(), 80000);
            recipe.AddTile(TileID.DemonAltar);

            recipe.Register();
        }
    }
}