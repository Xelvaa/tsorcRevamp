﻿using Terraria;
using Terraria.ModLoader;

namespace tsorcRevamp.Buffs.Runeterra.Magic
{
    public class OrbOfFlameFireballCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
