using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace tsorcRevamp.Items.Weapons.Melee.Broadswords
{
    public class Ragnarok : ModItem
    {
        public static float ArmorPenetration = 50;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ArmorPenetration);

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Cyan;
            Item.useTurn = false;
            Item.autoReuse = true;
            Item.damage = 175;
            Item.crit = 11;
            Item.width = 62;
            Item.height = 62;
            Item.knockBack = 10;
            Item.maxStack = 1;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 15;
            Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.value = PriceByRarity.Cyan_9;
            Item.shoot = ModContent.ProjectileType<Projectiles.Nothing>();
            tsorcInstancedGlobalItem instancedGlobal = Item.GetGlobalItem<tsorcInstancedGlobalItem>();
            instancedGlobal.slashColor = Microsoft.Xna.Framework.Color.Gold;
        }

        public override void ModifyHitNPC(Player player,NPC target, ref NPC.HitModifiers modifiers)
        {
            //50 ArmorPenetration may seem big buff for player, but it actually only increase 25 damage at max
            //may add ArmorPenetration buff to this weapon, but that may be too much for such non-SHM-Boss weapon
            if (player.ZoneUnderworldHeight)
            {
                modifiers.ArmorPenetration += ArmorPenetration;
            }
        }
    }
}
