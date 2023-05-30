using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using tsorcRevamp.Items.Weapons.Summon.Whips;

namespace tsorcRevamp.Buffs.Summon
{
    public class TerraFallBuff : ModBuff
	{
        public float AttackSpeed;

        public override LocalizedText Description => base.Description.WithFormatArgs(AttackSpeed);

        public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = false;
		}

		public override void Update(Player player, ref int buffIndex)
        {
            AttackSpeed = player.GetModPlayer<tsorcRevampPlayer>().TerraFallStacks * 12f;
            player.GetAttackSpeed(DamageClass.Summon) += AttackSpeed / 100f;

            if (player.whoAmI == Main.myPlayer)
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Summon.Whips.TerraFallTerraprisma>()] == 0)
                {
                    int whipDamage = (int)player.GetTotalDamage(DamageClass.SummonMeleeSpeed).ApplyTo(TerraFall.BaseDamage);
                    Projectile.NewProjectileDirect(player.GetSource_Buff(buffIndex), player.Center, Vector2.One, ModContent.ProjectileType<Projectiles.Summon.Whips.TerraFallTerraprisma>(), whipDamage, 1f, Main.myPlayer);
                }
            }
        }
    }
}
