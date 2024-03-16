﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using tsorcRevamp.Buffs.Runeterra.Summon;
using tsorcRevamp.Items.Weapons.Summon.Runeterra;
using tsorcRevamp.Projectiles.VFX;

namespace tsorcRevamp.Projectiles.Melee.Axes
{
    class AncientFireAxeFireballBurst : DynamicTrail
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 30;

            trailWidth = 45;
            trailPointLimit = 2000;
            trailMaxLength = 9999999;
            collisionPadding = 50;
            NPCSource = false;
            trailCollision = true;
            collisionFrequency = 5;
            noFadeOut = true;
            customEffect = ModContent.Request<Effect>("tsorcRevamp/Effects/CataluminanceTrail", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.CritChance = (int)Projectile.ai[0];
            Projectile.ai[0] = 0;
        }
        float waitTimer = 0;
        float fixedSpeed = 1;
        public bool FinalStandMode = false;
        Vector2 trailVelocity;
        List<Vector2> trailVelocities;
        public bool PlayAnimation = true;
        public override void AI()
        {
            waitTimer++;
            //Special ring mode during his final phase
            if (waitTimer == 1)
            {
                fadeOut = 0;
                trailPositions = new List<Vector2>();
                trailRotations = new List<float>();
                trailVelocities = new List<Vector2>();
                for (int i = 0; i < 110; i++)
                {
                    float angle = MathHelper.TwoPi * i / 95;
                    trailPositions.Add(Projectile.Center + new Vector2(5, 0).RotatedBy(angle));
                    trailRotations.Add(angle + MathHelper.PiOver2);
                    trailVelocities.Add(new Vector2(5, 0).RotatedBy(angle));
                }
                trailCurrentLength = CalculateLength();
            }
            else
            {
                if (waitTimer < 90 && fixedSpeed < 2)
                {
                    fixedSpeed += 0.01f;
                }
                fixedSpeed += 0.01f;
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    trailPositions[i] += fixedSpeed * new Vector2(3, 0).RotatedBy(trailRotations[i] - MathHelper.PiOver2);
                }

                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (i < trailPositions.Count - 1 && Vector2.Distance(trailPositions[i], trailPositions[i + 1]) > 20 && trailPositions.Count < trailPointLimit)
                    {
                        trailPositions.Insert(i + 1, (trailPositions[i] + trailPositions[i + 1]) / 2f);
                        trailRotations.Insert(i + 1, (trailRotations[i] + trailRotations[i + 1]) / 2f);
                        trailVelocities.Insert(i + 1, (trailVelocities[i] + trailVelocities[i + 1]) / 2f);
                    }
                }
            }
        }
        public override float CollisionWidthFunction(float progress)
        {
            return 25;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        bool PreSetTrail = false;
        Color trailColor = new Color(0.2f, 0.7f, 1f);
        float timeFactor = 0;
        public override void SetEffectParameters(Effect effect)
        {
            collisionFrequency = 2;
            visualizeTrail = false;
            collisionPadding = 8;
            collisionEndPadding = trailPositions.Count / 24;
            trailWidth = 25;

            timeFactor++;

            //Shifts its color slightly over time
            Vector3 hslColor = Main.rgbToHsl(Color.Firebrick);
            hslColor.X += 0.03f * (float)Math.Cos(timeFactor / 25f);
            Color rgbColor = Main.hslToRgb(hslColor);

            effect.Parameters["noiseTexture"].SetValue(tsorcRevamp.NoiseWavy);
            effect.Parameters["fadeOut"].SetValue(0.85f);
            effect.Parameters["finalStand"].SetValue(FinalStandMode.ToInt());
            effect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["shaderColor"].SetValue(rgbColor.ToVector4());
            effect.Parameters["shaderColor2"].SetValue(new Color(2f, 0.5f, 1f).ToVector4());
            effect.Parameters["length"].SetValue(trailCurrentLength);
            effect.Parameters["WorldViewProjection"].SetValue(GetWorldViewProjectionMatrix());
        }
        public override bool PreDraw(ref Color lightColor)
        {
            base.PreDraw(ref lightColor);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
        }
        public override void OnKill(int timeLeft)
        {
            waitTimer = 0;
        }
    }
}