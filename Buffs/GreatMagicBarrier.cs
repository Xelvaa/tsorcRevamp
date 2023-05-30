﻿using Terraria;
using Terraria.ModLoader;

namespace tsorcRevamp.Buffs
{
    public class GreatMagicBarrier : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 60;
            Lighting.AddLight(player.Center, .7f, .7f, .45f);
            Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, player.velocity, ModContent.ProjectileType<Projectiles.GreatMagicBarrier>(), 0, 0f, player.whoAmI);
        }
    }
}