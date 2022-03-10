﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;


namespace tsorcRevamp.Items
{
    class GlintstonePebble : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("\nCrush in your hand to send out Glintstone Specks" +
                "\nGlintstone Specks hover a moment before firing off toward the closest enemy" +
                "\nGreat for setting up ambushes on unsuspecting foes. Scales with magic damage");
        }

        public override void SetDefaults()
        {
            item.consumable = true;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.width = 16;
            item.height = 18;
            item.maxStack = 9999;
            item.value = 1000;
            item.useAnimation = 90;
            item.useTime = 90;
            item.useTurn = true;
            item.rare = ItemRarityID.Blue;
        }


        public override bool CanUseItem(Player player)
        {
            if (player.HasBuff(BuffID.Frozen) || player.HasBuff(BuffID.Stoned))
            {
                return false;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            if (player.itemAnimation != 0)
            {
                float slowdownX = player.velocity.X * .9f;
                float slowdownY = player.velocity.Y * .9f;

                player.velocity.X = slowdownX;
                player.velocity.Y = slowdownY;
            }
        }

        public override void UseStyle(Player player)
        {
            if (player.itemTime == 0)
            {
                player.itemTime = (int)(item.useTime / PlayerHooks.TotalUseTimeMultiplier(player, item));
                player.AddBuff(ModContent.BuffType<Buffs.Crippled>(), 90);
                player.AddBuff(ModContent.BuffType<Buffs.GrappleMalfunction>(), 90);
            }

            if (player.itemTime < (int)(item.useTime / PlayerHooks.TotalUseTimeMultiplier(player, item)) / 2)
            {
                item.useStyle = ItemUseStyleID.HoldingUp;

                if (Main.rand.Next(4) == 0)
                {
                    if (player.direction == 1)
                    {
                        Dust dust = Main.dust[Dust.NewDust(new Vector2(player.position.X + 10, player.position.Y - 4), 10, 10, 68, player.velocity.X, player.velocity.Y, 100, Color.White, Main.rand.NextFloat(.8f, 1.2f))];
                        dust.noGravity = true;
                    }
                    else
                    {
                        Dust dust = Main.dust[Dust.NewDust(new Vector2(player.position.X - 2, player.position.Y - 4), 10, 10, 68, player.velocity.X, player.velocity.Y, 100, Color.White, Main.rand.NextFloat(.8f, 1.2f))];
                        dust.noGravity = true;
                    }
                }
            }

            if (player.itemTime >= (int)(item.useTime / PlayerHooks.TotalUseTimeMultiplier(player, item)) / 2)
            {
                item.useStyle = ItemUseStyleID.HoldingOut;
            }

            if (player.itemTime == 1)
            {
                Main.PlaySound(SoundID.Item27.WithVolume(.9f).WithPitchVariance(.3f), player.position); // Plays sound.
                Main.PlaySound(SoundID.Item29.WithVolume(.9f).WithPitchVariance(.3f), player.position); // Plays sound.

                for (int i = 0; i < 30; i++)
                {
                    if (player.direction == 1)
                    {
                        Dust dust = Main.dust[Dust.NewDust(new Vector2(player.position.X + 10, player.position.Y - 4), 10, 10, 68, Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-1.2f, 1.2f), 100, Color.White, Main.rand.NextFloat(.8f, 1.2f))];
                        dust.noGravity = true;
                    }
                    else
                    {
                        Dust dust = Main.dust[Dust.NewDust(new Vector2(player.position.X - 2, player.position.Y - 4), 10, 10, 68, Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-1.2f, 1.2f), 100, Color.White, Main.rand.NextFloat(.8f, 1.2f))];
                        dust.noGravity = true;
                    }
                }

                int damage = (int)((8 * (player.magicDamage + player.allDamage - 1) * 3) * player.allDamageMult);

                if (player.GetModPlayer<tsorcRevampPlayer>().BearerOfTheCurse)
                { 
                    damage = (int)(damage * 1.2f); //because player.allDamageMult isnt taking it into accound for some reason :/
                    player.GetModPlayer<tsorcRevampStaminaPlayer>().staminaResourceCurrent -= 20;
                }

                for (int i = 0; i < 7; i++)
                {
                    Projectile.NewProjectile(player.Center + new Vector2(4 * player.direction, - 20), new Vector2(0, 0), ModContent.ProjectileType<Projectiles.GlintstoneSpeck>(), damage, 2.5f, Main.myPlayer);
                }

                //if (Main.mouseItem == null) // Not sure why but seems like it's not null if you're using something
                //{

                if (item.stack == 1) item.TurnToAir();
                else item.stack--;
                //}
                //else
                //{
                if (Main.mouseItem.stack == 1) Main.mouseItem.TurnToAir();
                else Main.mouseItem.stack--;
                //}
            }
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Lighting.AddLight(item.Center, 0.2f, 0.2f, 0.35f);

            if (Main.rand.Next(35) == 0)
            {
                Dust dust = Main.dust[Dust.NewDust(new Vector2(item.position.X, item.position.Y), 14, 14, 68, item.velocity.X, item.velocity.Y, 100, default, Main.rand.NextFloat(.4f, .8f))];
                dust.velocity *= 0f;
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }

            Color color = Color.White * 0.8f;
            Texture2D texture = Main.itemTexture[item.type];
            spriteBatch.Draw(texture, new Vector2(item.position.X - Main.screenPosition.X + item.width * 0.5f, item.position.Y - Main.screenPosition.Y + item.height - texture.Height * 0.5f + 2f),
                new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
    }
}