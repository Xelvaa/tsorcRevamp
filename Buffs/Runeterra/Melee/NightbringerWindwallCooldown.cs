﻿using Terraria;
using Terraria.ModLoader;

namespace tsorcRevamp.Buffs.Runeterra.Melee
{
    public class NightbringerWindwallCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
