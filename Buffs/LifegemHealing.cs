﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace tsorcRevamp.Buffs
{
    public class LifegemHealing : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<tsorcRevampPlayer>().LifegemHealing = true;

            if (Main.rand.NextBool(4))
            {
                var dust = Dust.NewDustDirect(new Vector2(player.position.X, player.position.Y + 10), player.width, player.height, 43, 0, -1f, 100, Color.White, Main.rand.NextFloat(.8f, 1f));
                dust.noGravity = true;
                dust.velocity.X = 0;
            }
        }
    }
}
