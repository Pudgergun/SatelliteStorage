using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ID;
using log4net;
using SatelliteStorage.DriveSystem;
using Terraria.GameContent.Achievements;
using System;

namespace SatelliteStorage
{
    class SatelliteStorageSystem : ModSystem
    {
        private double lastGeneratorsTickTime = 0;
        private long lastGeneratorsServerTimestamp = 0;
        private bool requestStates = false;
        private List<TagCompound> notFoundItems = new List<TagCompound>();

        public override void UpdateUI(GameTime gameTime)
        {
            SatelliteStorage.instance.OnUpdateUI(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            SatelliteStorage.instance.OnModifyInterfaceLayers(layers);
        }

        public override void SaveWorldData(TagCompound tag)
        {
            IList<DriveItem> t_items = DriveChestSystem.GetItems();

            IList<TagCompound> itemsCompound = new List<TagCompound>();

            for (int i = 0; i < t_items.Count; i++)
            {
                DriveItem item = t_items[i];
                itemsCompound.Add(Utils.DriveItemsSerializer.SaveDriveItem(item));
            }

            foreach(TagCompound notFoundCompound in notFoundItems)
            {
                itemsCompound.Add(notFoundCompound);
            }
            notFoundItems.Clear();

            IList<TagCompound> generatorsCompound = new List<TagCompound>();

            Dictionary<int, int> generators = DriveChestSystem.GetGenerators();
            foreach (int key in generators.Keys)
            {
                TagCompound generatorCompound = new TagCompound();
                generatorCompound.Add("type", key);
                generatorCompound.Add("count", generators[key]);
                generatorsCompound.Add(generatorCompound);
            }

            tag.Set("SatelliteStorage_DriveChestItems", itemsCompound);
            tag.Set("SatelliteStorage_IsSputnikPlaced", DriveChestSystem.isSputnikPlaced);
            tag.Set("SatelliteStorage_Generators", generatorsCompound);
            tag.Set("SatelliteStorage_Version", SatelliteStorage.ModVersion);

            base.SaveWorldData(tag);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            DriveChestSystem.isSputnikPlaced = false;
            DriveChestSystem.ClearGenerators();
            DriveChestSystem.ClearItems();

            IList<TagCompound> items = tag.GetList<TagCompound>("SatelliteStorage_DriveChestItems");
            IList<TagCompound> generatorsCompound = tag.GetList<TagCompound>("SatelliteStorage_Generators");
            DriveChestSystem.isSputnikPlaced = tag.GetBool("SatelliteStorage_IsSputnikPlaced");
            int modversion = tag.GetInt("SatelliteStorage_Version");
            //SatelliteStorage.Debug("Mod Version: " + modversion);
            List<DriveItem> loadedItems = new List<DriveItem>();


            for(int i = 0; i < items.Count; i++)
            {
                TagCompound itemCompound = items[i];
                DriveItem item = Utils.DriveItemsSerializer.LoadDriveItem(itemCompound, modversion);
                if (item != null) loadedItems.Add(item);
                else
                {
                    notFoundItems.Add(itemCompound);
                }
            }

            Dictionary<int, int> generators = DriveChestSystem.GetGenerators();
            foreach (TagCompound generatorCompound in generatorsCompound)
            {
                generators[generatorCompound.GetInt("type")] = generatorCompound.GetInt("count");
            }

            DriveChestSystem.InitItems(loadedItems);
            DriveChestSystem.InitGenerators(generators);

            base.LoadWorldData(tag);
        }

        public override void OnWorldUnload()
        {
            base.OnWorldUnload();
            DriveChestSystem.ClearItems();
        }

        public override void PreUpdateItems()
        {
            base.PreUpdateItems();
            
        }

        public override void AddRecipes()
        {
            Recipe.Create(ItemID.MagicMirror, 1)
            .AddIngredient(ItemID.Glass, 250)
            .AddIngredient(ItemID.FallenStar, 25)
            .Register();

            Recipe.Create(ItemID.IceMirror, 1)
            .AddIngredient(ItemID.MagicMirror, 1)
            .AddIngredient(ItemID.IceBlock, 50)
            .Register();

            void AddQuartzRecipe(int itemID, int stack)
            {
                Recipe.Create(ModContent.ItemType<Items.QuartzShard>(), 1)
                .AddIngredient(ItemID.GoldBar, 3)
                .AddIngredient(ItemID.Obsidian, 5)
                .AddIngredient(itemID, stack)
                .AddTile(TileID.Anvils)
                .Register();

                Recipe.Create(ModContent.ItemType<Items.QuartzShard>(), 1)
                .AddIngredient(ItemID.PlatinumBar, 3)
                .AddIngredient(ItemID.Obsidian, 5)
                .AddIngredient(itemID, stack)
                .AddTile(TileID.Anvils)
                .Register();
            }

            AddQuartzRecipe(ItemID.Amethyst, 1);
        }

        public override void PostUpdateWorld()
        {
            int interval = SatelliteStorage.GeneratorsInterval;
            if (Main.raining) interval = interval / 2;

            if (Main.netMode == NetmodeID.SinglePlayer && Main.gameTimeCache.TotalGameTime.TotalMilliseconds > lastGeneratorsTickTime + interval)
            {
                lastGeneratorsTickTime = Main.gameTimeCache.TotalGameTime.TotalMilliseconds;

                DriveChestSystem.OnGeneratorsTick();
            }

            base.PostUpdateWorld();
        }

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server) {
                long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                int interval = SatelliteStorage.GeneratorsInterval;
                if (Main.raining) interval = interval / 2;

                if (timestamp > lastGeneratorsServerTimestamp + interval)
                {
                    lastGeneratorsServerTimestamp = timestamp;
                    DriveChestSystem.OnGeneratorsTick();
                }
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (!requestStates)
                {
                    requestStates = true;

                    Player player = Main.LocalPlayer;

                    ModPacket packet = SatelliteStorage.instance.GetPacket();
                    packet.Write((byte)SatelliteStorage.MessageType.RequestStates);
                    packet.Write((byte)player.whoAmI);
                    packet.Send();
                    packet.Close();
                }
            }

            base.PostUpdateEverything();
        }
    }
}
