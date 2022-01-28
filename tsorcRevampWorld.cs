using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using tsorcRevamp;
using tsorcRevamp.Items;
using tsorcRevamp.Items.Potions.PermanentPotions;
using tsorcRevamp.Buffs;
using System;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent.NetModules;
using Terraria.Localization;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using tsorcRevamp.UI;

namespace tsorcRevamp {
    public class tsorcRevampWorld : ModWorld {

        public static bool DownedVortex;
        public static bool DownedNebula;
        public static bool DownedStardust;
        public static bool DownedSolar;
        public static bool SuperHardMode;
        public static bool TheEnd;

        public static Dictionary<int, int> Slain;

        public static List<Vector2> LitBonfireList;

        public override void Initialize() {
            DownedVortex = false;
            DownedNebula = false;
            DownedStardust = false;
            DownedSolar = false;
            SuperHardMode = false;
            TheEnd = false;
            Slain = new Dictionary<int, int>();
            LitBonfireList = new List<Vector2>();

            tsorcScriptedEvents.InitializeScriptedEvents();
            Tiles.SoulSkellyGeocache.InitializeSkellys();
        }

		public override TagCompound Save() {

            List<string> downed = new List<string>();

            if (DownedVortex) downed.Add("DownedVortex");
            if (DownedNebula) downed.Add("DownedNebula");
            if (DownedStardust) downed.Add("DownedStardust");
            if (DownedSolar) downed.Add("DownedSolar");

            List<string> world_state= new List<string>();
            if (SuperHardMode) world_state.Add("SuperHardMode");
            //This saves the fact that SuperHardMode has been disabled
            if(world_state.Contains("SuperHardMode") && !SuperHardMode)
            {
                world_state.Remove("SuperHardMode");
            }
            if (TheEnd) world_state.Add("TheEnd");

            TagCompound tagCompound = new TagCompound
			{
                {"downed", downed},
                {"world_state", world_state},
            };
			SaveSlain(tagCompound);
            tsorcScriptedEvents.SaveScriptedEvents(tagCompound);
            return tagCompound;
		}

		private void SaveSlain(TagCompound tag) {
            tag.Add("type", Slain.Keys.ToList());
            tag.Add("value", Slain.Values.ToList());
        }

        public override void Load(TagCompound tag)
        {
            LoadSlain(tag);
            tsorcScriptedEvents.LoadScriptedEvents(tag);

            IList<string> downedList = tag.GetList<string>("downed");
            DownedVortex = downedList.Contains("DownedVortex");
            DownedNebula = downedList.Contains("DownedNebula");
            DownedStardust = downedList.Contains("DownedStardust");
            DownedSolar = downedList.Contains("DownedSolar");

            IList<string> worldStateList = tag.GetList<string>("world_state");
            SuperHardMode = worldStateList.Contains("SuperHardMode");
            TheEnd = worldStateList.Contains("TheEnd");

            LitBonfireList = GetActiveBonfires();

            //If the player leaves the world or turns off their computer in the middle of the fight or whatever, this will de-actuate the pyramid for them next time they load
            if (ModContent.GetInstance<tsorcRevampConfig>().AdventureModeItems)
            {
                if (Main.tile[5810, 1670] != null)
                {
                    if (Main.tile[5810, 1670].active() && Main.tile[5810, 1670].inActive())
                    {
                        NPCs.Bosses.SuperHardMode.DarkCloud.ActuatePyramid();
                    }
                }
            }
        }

        private void LoadSlain(TagCompound tag) {
            if (tag.ContainsKey("type")) {
                List<int> list = tag.Get<List<int>>("type");
                List<int> list2 = tag.Get<List<int>>("value");
                for (int i = 0; i < list.Count; i++) {
                    Slain.Add(list[i], list2[i]);
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            if(Main.netMode == NetmodeID.Server)
            {
                //Storing it in an int32 just so its exact type is guranteed, since that does matter
                int slainSize = Slain.Count;
                writer.Write(slainSize);
                foreach (KeyValuePair<int, int> pair in Slain)
                {
                    //Fuck it, i'm encoding each entry of slain as a Vector2. It's probably more sane than doing it byte by byte.
                    writer.WriteVector2(new Vector2(pair.Key, pair.Value));
                }
                int litBonfireSize = LitBonfireList.Count;
                writer.Write(litBonfireSize);
                foreach (Vector2 location in LitBonfireList) {
                    writer.WriteVector2(location);
                }
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            int slainSize = reader.ReadInt32();
            for (int i = 0; i < slainSize; i++)
            {
                Vector2 readData = reader.ReadVector2();
                if (Slain.ContainsKey((int)readData.X))
                {
                    Slain[(int)readData.X] = (int)readData.Y;
                }
                else
                {
                    Slain.Add((int)readData.X, (int)readData.Y);
                }
            }
            int litBonfireSize = reader.ReadInt32();
            for (int i = 0; i < litBonfireSize; i++) {
                Vector2 readLocation = reader.ReadVector2();
                if (LitBonfireList.Contains(readLocation)) {
                    continue;
                }
                else {
                    LitBonfireList.Add(readLocation);
                }
                
            }
        }

        public static bool JustPressed(Keys key) {
            return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
        }


        #region CampfireToBonfire (Is also Skelly Loot Cache replacement code)

        public static void CampfireToBonfire() {
            Mod mod = ModContent.GetInstance<tsorcRevamp>();
            for (int x = 0; x < Main.maxTilesX - 2; x++) {
                for (int y = 0; y < Main.maxTilesY - 2; y++) {

                    //Campfire to Bonfire
                    if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.Campfire) {

                        //kill the space above the campfire, to remove vines and such
                        for (int q = 0; q < 3; q++) {
                            for (int w = -2; w < 2; w++) {
                                WorldGen.KillTile(x + q, y + w, false, false, true);  
                            }
                        }
                        Dust.QuickBox(new Vector2(x + 1, y + 1) * 16, new Vector2(x + 2, y + 2) * 16, 2, Color.YellowGreen, null);
                        //WorldGen.Place3x4(x + 1, y + 1, (ushort)ModContent.TileType<Tiles.BonfireCheckpoint>(), 0);

                        int style = 0;
                        ushort type = (ushort)ModContent.TileType<Tiles.BonfireCheckpoint>();
                        //reimplement WorldGen.Place3x4 minus SolidTile2 checking because this game is fucked 
                        {
                            if (x+1 < 5 || x + 1 > Main.maxTilesX - 5 || y + 1 < 5 || y + 1 > Main.maxTilesY - 5) {
                                return;
                            }
                            bool flag = true;
                            for (int i = x + 1 - 1; i < x + 1 + 2; i++) {
                                for (int j = y + 1 - 3; j < y + 1 + 1; j++) {
                                    if (Main.tile[i, j] == null) {
                                        Main.tile[i, j] = new Tile();
                                    }
                                    if (Main.tile[i, j].active()) {
                                        flag = false;
                                    }
                                }
                                if (Main.tile[i, y + 1 + 1] == null) {
                                    Main.tile[i, y + 1 + 1] = new Tile();
                                }
                            }
                            if (flag) {
                                int num = style * 54;
                                for (int k = -3; k <= 0; k++) {
                                    short frameY = (short)((3 + k) * 18);
                                    Main.tile[x + 1 - 1, y + 1 + k].active(active: true);
                                    Main.tile[x + 1 - 1, y + 1 + k].frameY = frameY;
                                    Main.tile[x + 1 - 1, y + 1 + k].frameX = (short)num;
                                    Main.tile[x + 1 - 1, y + 1 + k].type = type;
                                    Main.tile[x + 1, y + 1 + k].active(active: true);
                                    Main.tile[x + 1, y + 1 + k].frameY = frameY;
                                    Main.tile[x + 1, y + 1 + k].frameX = (short)(num + 18);
                                    Main.tile[x + 1, y + 1 + k].type = type;
                                    Main.tile[x + 1 + 1, y + 1 + k].active(active: true);
                                    Main.tile[x + 1 + 1, y + 1 + k].frameY = frameY;
                                    Main.tile[x + 1 + 1, y + 1 + k].frameX = (short)(num + 36);
                                    Main.tile[x + 1 + 1, y + 1 + k].type = type;
                                }
                            }

                        }
                    }

                    //Slime blocks to SkullLeft - SlimeBlock-PinkSlimeBlock (I tried to stick right and lefts together but the code refuses to work for both, I swear I'm not just being dumb) 
                    if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.PinkSlimeBlock && Main.tile[x - 1, y].type == TileID.SlimeBlock) 
                    {

                        //kill the space the skull occupies, to remove vines and such
                        for (int q = -1; q < 1; q++)
                        {
                            for (int w = -1; w < 1; w++)
                            {
                                WorldGen.KillTile(x + q, y + w, false, false, true);
                            }
                        }
                        //WorldGen.Place3x4(x + 1, y + 1, (ushort)ModContent.TileType<Tiles.BonfireCheckpoint>(), 0);

                        int style = 0;
                        ushort type = (ushort)ModContent.TileType<Tiles.SoulSkullL>();
                        //reimplement WorldGen.Place2x2 minus SolidTile2 checking
                        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5) {
                            return;
                        }
                        short num = 0;
                        bool flag = true;
                        for (int i = x - 1; i < x + 1; i++) {
                            for (int j = y - 1; j < y + 1; j++) {
                                if (Main.tile[i, j] == null) {
                                    Main.tile[i, j] = new Tile();
                                }
                                if (Main.tile[i, j].active()) {
                                    flag = false;
                                }
                            }
                            if (Main.tile[i, y + 1] == null) {
                                Main.tile[i, y + 1] = new Tile();
                            }
                        }
                        if (flag) {
                            short num2 = (short)(36 * style);
                            Main.tile[x - 1, y - 1].active(active: true);
                            Main.tile[x - 1, y - 1].frameY = num;
                            Main.tile[x - 1, y - 1].frameX = num2;
                            Main.tile[x - 1, y - 1].type = type;
                            Main.tile[x, y - 1].active(active: true);
                            Main.tile[x, y - 1].frameY = num;
                            Main.tile[x, y - 1].frameX = (short)(num2 + 18);
                            Main.tile[x, y - 1].type = type;
                            Main.tile[x - 1, y].active(active: true);
                            Main.tile[x - 1, y].frameY = (short)(num + 18);
                            Main.tile[x - 1, y].frameX = num2;
                            Main.tile[x - 1, y].type = type;
                            Main.tile[x, y].active(active: true);
                            Main.tile[x, y].frameY = (short)(num + 18);
                            Main.tile[x, y].frameX = (short)(num2 + 18);
                            Main.tile[x, y].type = type;
                        }
                    }

                    //Slime block to SkullRight - PinkSlimeBlock-SlimeBlock
                    if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.SlimeBlock && Main.tile[x - 1, y].type == TileID.PinkSlimeBlock)
                    {

                        //kill the space the skull occupies, to remove vines and such
                        for (int q = -1; q < 1; q++)
                        {
                            for (int w = -1; w < 1; w++)
                            {
                                WorldGen.KillTile(x + q, y + w, false, false, true);
                            }
                        }
                        //WorldGen.Place3x4(x + 1, y + 1, (ushort)ModContent.TileType<Tiles.BonfireCheckpoint>(), 0);

                        int style = 0;
                        ushort type = (ushort)ModContent.TileType<Tiles.SoulSkullR>();
                        //reimplement WorldGen.Place2x2 minus SolidTile2 checking
                        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
                        {
                            return;
                        }
                        short num = 0;
                        bool flag = true;
                        for (int i = x - 1; i < x + 1; i++)
                        {
                            for (int j = y - 1; j < y + 1; j++)
                            {
                                if (Main.tile[i, j] == null)
                                {
                                    Main.tile[i, j] = new Tile();
                                }
                                if (Main.tile[i, j].active())
                                {
                                    flag = false;
                                }
                            }
                            if (Main.tile[i, y + 1] == null)
                            {
                                Main.tile[i, y + 1] = new Tile();
                            }
                        }
                        if (flag)
                        {
                            short num2 = (short)(36 * style);
                            Main.tile[x - 1, y - 1].active(active: true);
                            Main.tile[x - 1, y - 1].frameY = num;
                            Main.tile[x - 1, y - 1].frameX = num2;
                            Main.tile[x - 1, y - 1].type = type;
                            Main.tile[x, y - 1].active(active: true);
                            Main.tile[x, y - 1].frameY = num;
                            Main.tile[x, y - 1].frameX = (short)(num2 + 18);
                            Main.tile[x, y - 1].type = type;
                            Main.tile[x - 1, y].active(active: true);
                            Main.tile[x - 1, y].frameY = (short)(num + 18);
                            Main.tile[x - 1, y].frameX = num2;
                            Main.tile[x - 1, y].type = type;
                            Main.tile[x, y].active(active: true);
                            Main.tile[x, y].frameY = (short)(num + 18);
                            Main.tile[x, y].frameX = (short)(num2 + 18);
                            Main.tile[x, y].type = type;
                        }
                    }

                    //Stucco blocks to SkellyLeft - GreyStucco-GreenStuccoBlock-GreyStuccoBlock
                    if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.GreenStucco && Main.tile[x + 1, y].type == TileID.GrayStucco && Main.tile[x - 1, y].type == TileID.GrayStucco)
                    {

                        //kill the space the skelly occupies, to remove vines and such
                        for (int q = -1; q < 2; q++)
                        {
                            for (int w = 0; w < 1; w++)
                            {
                                WorldGen.KillTile(x + q, y + w, false, false, true);
                            }
                        }

                        int style = 0;
                        ushort type = (ushort)ModContent.TileType<Tiles.SoulSkellyL>();
                        //reimplement WorldGen.Place3x1 minus SolidTile2
                        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
                        {
                            return;
                        }
                        bool flag = true;
                        for (int i = x - 1; i < x + 2; i++)
                        {
                            if (Main.tile[i, y] == null)
                            {
                                Main.tile[i, y] = new Tile();
                            }
                            if (Main.tile[i, y].active())
                            {
                                flag = false;
                            }
                            if (Main.tile[i, y + 1] == null)
                            {
                                Main.tile[i, y + 1] = new Tile();
                            }
                        }
                        if (flag)
                        {
                            short num = (short)(54 * style);
                            Main.tile[x - 1, y].active(active: true);
                            Main.tile[x - 1, y].frameY = 0;
                            Main.tile[x - 1, y].frameX = num;
                            Main.tile[x - 1, y].type = type;
                            Main.tile[x, y].active(active: true);
                            Main.tile[x, y].frameY = 0;
                            Main.tile[x, y].frameX = (short)(num + 18);
                            Main.tile[x, y].type = type;
                            Main.tile[x + 1, y].active(active: true);
                            Main.tile[x + 1, y].frameY = 0;
                            Main.tile[x + 1, y].frameX = (short)(num + 36);
                            Main.tile[x + 1, y].type = type;
                        }
                    }

                    //Stucco blocks to SkellyRight - GreenStucco-GreyStuccoBlock-GreenStuccoBlock
                    if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.GrayStucco && Main.tile[x + 1, y].type == TileID.GreenStucco && Main.tile[x - 1, y].type == TileID.GreenStucco)
                    {

                        //kill the space the skelly occupies, to remove vines and such
                        for (int q = -1; q < 2; q++)
                        {
                            for (int w = 0; w < 1; w++)
                            {
                                WorldGen.KillTile(x + q, y + w, false, false, true);
                            }
                        }

                        int style = 0;
                        ushort type = (ushort)ModContent.TileType<Tiles.SoulSkellyR>();
                        //reimplement WorldGen.Place3x1 minus SolidTile2
                        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
                        {
                            return;
                        }
                        bool flag = true;
                        for (int i = x - 1; i < x + 2; i++)
                        {
                            if (Main.tile[i, y] == null)
                            {
                                Main.tile[i, y] = new Tile();
                            }
                            if (Main.tile[i, y].active())
                            {
                                flag = false;
                            }
                            if (Main.tile[i, y + 1] == null)
                            {
                                Main.tile[i, y + 1] = new Tile();
                            }
                        }
                        if (flag)
                        {
                            short num = (short)(54 * style);
                            Main.tile[x - 1, y].active(active: true);
                            Main.tile[x - 1, y].frameY = 0;
                            Main.tile[x - 1, y].frameX = num;
                            Main.tile[x - 1, y].type = type;
                            Main.tile[x, y].active(active: true);
                            Main.tile[x, y].frameY = 0;
                            Main.tile[x, y].frameX = (short)(num + 18);
                            Main.tile[x, y].type = type;
                            Main.tile[x + 1, y].active(active: true);
                            Main.tile[x + 1, y].frameY = 0;
                            Main.tile[x + 1, y].frameX = (short)(num + 36);
                            Main.tile[x + 1, y].type = type;
                        }
                    }

                    //Confetti blocks to SkellyHangingUp (wrists chained) - Confetti Block
                    if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.Confetti)
                    {

                        //kill the space the skelly occupies, to remove vines and such
                        for (int q = -1; q < 2; q++)
                        {
                            for (int w = -1; w < 2; w++)
                            {
                                WorldGen.KillTile(x + q, y + w, false, false, true);
                            }
                        }

                        int style = 0;
                        ushort type = (ushort)ModContent.TileType<Tiles.SoulSkellyWall1>();
                        //reimplement WorldGen.Place3x3Wall
                        int num = x - 1;
                        int num2 = y - 1;
                        bool flag = true;
                        for (int i = num; i < num + 3; i++)
                        {
                            for (int j = num2; j < num2 + 3; j++)
                            {
                                if (Main.tile[i, j].active() || Main.tile[i, j].wall == 0)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            return;
                        }
                        int num3 = 0;
                        while (style > 35)
                        {
                            num3++;
                            style -= 36;
                        }
                        int num4 = style * 54;
                        int num5 = num3 * 54;
                        for (int k = num; k < num + 3; k++)
                        {
                            for (int l = num2; l < num2 + 3; l++)
                            {
                                Main.tile[k, l].active(active: true);
                                Main.tile[k, l].type = type;
                                Main.tile[k, l].frameX = (short)(num4 + 18 * (k - num));
                                Main.tile[k, l].frameY = (short)(num5 + 18 * (l - num2));
                            }
                        }
                    }

                    //Confetti blocks to SkellyHangingDown (ankles chained) - Confetti Black Block (aka Midnight Confetti Block)
                    if (Main.tile[x, y].active() && Main.tile[x, y].type == TileID.ConfettiBlack)
                    {

                        //kill the space the skelly occupies, to remove vines and such
                        for (int q = -1; q < 2; q++)
                        {
                            for (int w = -1; w < 2; w++)
                            {
                                WorldGen.KillTile(x + q, y + w, false, false, true);
                            }
                        }

                        int style = 0;
                        ushort type = (ushort)ModContent.TileType<Tiles.SoulSkellyWall2>();
                        //reimplement WorldGen.Place3x3Wall
                        int num = x - 1;
                        int num2 = y - 1;
                        bool flag = true;
                        for (int i = num; i < num + 3; i++)
                        {
                            for (int j = num2; j < num2 + 3; j++)
                            {
                                if (Main.tile[i, j].active() || Main.tile[i, j].wall == 0)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            return;
                        }
                        int num3 = 0;
                        while (style > 35)
                        {
                            num3++;
                            style -= 36;
                        }
                        int num4 = style * 54;
                        int num5 = num3 * 54;
                        for (int k = num; k < num + 3; k++)
                        {
                            for (int l = num2; l < num2 + 3; l++)
                            {
                                Main.tile[k, l].active(active: true);
                                Main.tile[k, l].type = type;
                                Main.tile[k, l].frameX = (short)(num4 + 18 * (k - num));
                                Main.tile[k, l].frameY = (short)(num5 + 18 * (l - num2));
                            }
                        }
                    }

                }
            }
            for (int i = 0; i < 400; i++) {
                if (Main.item[i].type == ItemID.Campfire && Main.item[i].active) {
                    Main.item[i].active = false; //delete ground items (in this case campfires)
                }
            }
        }

        #endregion
        public static List<Vector2> GetActiveBonfires()
        {
            List<Vector2> BonfireList = new List<Vector2>();
            int bonfireType = ModContent.TileType<Tiles.BonfireCheckpoint>();
            for (int i = 1; i < (Main.tile.GetUpperBound(0) - 1); i++)
            {
                for (int j = 1; j < (Main.tile.GetUpperBound(1) - 1); j++)
                {
                    //Check if each tile is a bonfire, and has a bonfire tile to its right and below it, but none to its left and above it. Only the top left corner of each bonfire is valid for this.
                    if ((Main.tile[i, j] != null && Main.tile[i, j].active() && Main.tile[i, j].type == bonfireType) && (Main.tile[i - 1, j] == null || !Main.tile[i - 1, j].active() || Main.tile[i - 1, j].type != bonfireType) && (Main.tile[i, j - 1] == null || !Main.tile[i, j - 1].active() || Main.tile[i, j - 1].type != bonfireType))
                    {
                        if (Main.tile[i, j].frameY / 74 != 0)
                        {
                            BonfireList.Add(new Vector2(i, j));
                        }
                    }
                }
            }

            return BonfireList;
        }

        Texture2D SHMSun1 = ModContent.GetTexture("tsorcRevamp/Textures/SHMSun1");
        Texture2D SHMSun2 = ModContent.GetTexture("tsorcRevamp/Textures/SHMSun2");
        Texture2D SHMSun3 = ModContent.GetTexture("tsorcRevamp/Textures/SHMSun1");
        Texture2D SHMMoon = ModContent.GetTexture("tsorcRevamp/Textures/SHMMoon");
        Texture2D VanillaSun1 = ModContent.GetTexture("Terraria/Sun");
        Texture2D VanillaSun2 = ModContent.GetTexture("Terraria/Sun2");
        Texture2D VanillaSun3 = ModContent.GetTexture("Terraria/Sun3");
        List<Texture2D> VanillaMoonTextures;

        //MAKE CATACOMBS DUNGEON BIOME - This code was blocking spawns in the catacombs, but catacombs now works as dungeon without it likely
        //because of other code improving dungeon spawn detection
        
        //public override void TileCountsAvailable(int[] tileCounts) {
            //Main.dungeonTiles += tileCounts[TileID.BoneBlock];
            //Main.dungeonTiles += tileCounts[TileID.MeteoriteBrick];
            
        //}

        public override void PostUpdate() {
            
            if (JustPressed(Keys.Home) && JustPressed(Keys.NumPad0)) //they have to be pressed *on the same tick*. you can't hold one and then press the other.
                CampfireToBonfire();
            bool charm = false;
            foreach (Player p in Main.player) {
                for (int i = 3; i <= 8; i++) {
                    if (p.armor[i].type == ModContent.ItemType<Items.Accessories.CovenantOfArtorias>()) {
                        charm = true;
                        break;
                    }
                }
            }
            if (charm) {
                Main.bloodMoon = true;
                Main.moonPhase = 0;
                Main.dayTime = false;
                Main.time = 16240.0;
                if (Main.GlobalTime % 120 == 0 && Main.netMode != NetmodeID.SinglePlayer) {
                    //globaltime always ticks up unless the player is in camera mode, and lets be honest: who uses camera mode? 
                    NetMessage.SendData(MessageID.WorldData);
                }
            }
            if (!Main.dedServ) {
                if (SuperHardMode) {
                    for (int i = 0; i < Main.moonTexture.Length; i++) {
                        Main.moonTexture[i] = SHMMoon;
                    }
                    Main.sunTexture = SHMSun1;
                    Main.sun2Texture = SHMSun2;
                    Main.sun3Texture = SHMSun3;
                }
                if (TheEnd) { //super hardmode and the end are mutually exclusive, so there won't be any "z-fighting", but this still feels silly
                    Main.sunTexture = VanillaSun1;
                    Main.sun2Texture = VanillaSun2;
                    Main.sun3Texture = VanillaSun3;
                    if (VanillaMoonTextures == null)
                    {
                        VanillaMoonTextures = new List<Texture2D>();
                        for (int i = 0; i < Main.moonTexture.Length; i++)
                        {
                            VanillaMoonTextures.Add(ModContent.GetTexture("Terraria/Moon_" + i));
                        }
                    }
                    for (int i = 0; i < Main.moonTexture.Length; i++) {
                        Main.moonTexture[i] = VanillaMoonTextures[i];
                    }
                }
            }
        }

        //Called upon the death of Gwyn, Lord of Cinder. Disables both hardmode and superhardmode, and sets the world state to "The End".
        public static void InitiateTheEnd()
        {
            Color c = new Color(255f, 255f, 60f);
            if (tsorcRevampWorld.SuperHardMode)
            {
                if (Main.netMode == 0)
                {
                    Main.NewText("The portal from The Abyss has closed!", c);
                    Main.NewText("The world has been healed. Attraidies' sway over the world has finally ended!", c);
                }
                else if (Main.netMode == 2)
                {
                    NetTextModule.SerializeServerMessage(NetworkText.FromLiteral("The portal from The Abyss has closed!"), c);
                    NetTextModule.SerializeServerMessage(NetworkText.FromLiteral("The world has been healed. Attraidies' sway over the world has finally ended!"), c);
                }
            }
            else
            {
                if (Main.netMode == 0)
                {
                    Main.NewText("You have vanquished the final guardian...", c);
                    Main.NewText("The portal from The Abyss remains closed. All is at peace...", c);
                }
                else if (Main.netMode == 2)
                {
                    NetTextModule.SerializeServerMessage(NetworkText.FromLiteral("You have vanquished the final guardian..."), c);
                    NetTextModule.SerializeServerMessage(NetworkText.FromLiteral("The portal from The Abyss remains closed. All is at peace..."), c);
                }
            }
            
            //These are outside of the if statements just so players can still disable hardmode or superhardmode if they happen to activate them again.
            Main.hardMode = false;
            tsorcRevampWorld.SuperHardMode = false;
            tsorcRevampWorld.TheEnd = true;

            //		Main.NewText("You have vanquished the final guardian of the Abyss...");
            //		Main.NewText("The kiln of the First Flame has been ignited!");
            //		//Main.NewText("Congratulations, you have inherited the fire of this world. You will forever be known as the hero of the age.");  
        }
    }
}