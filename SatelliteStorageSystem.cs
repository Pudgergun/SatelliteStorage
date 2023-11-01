using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.ID;
using SatelliteStorage.DriveSystem;
using System;
using SatelliteStorage.Utils;
using SatelliteStorage.ModNetwork;

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
            SatelliteStorage.UpdateUI(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            SatelliteStorage.ModifyInterfaceLayers(layers);
        }

        public override void SaveWorldData(TagCompound tag)
        {
            SatelliteStorage.Debug("Saving world data");

            IList<IDriveItem> t_items = SatelliteStorage.driveChestSystem.GetItems();

            IList<TagCompound> itemsCompound = new List<TagCompound>();

            for (int i = 0; i < t_items.Count; i++)
            {
                IDriveItem item = t_items[i];
                itemsCompound.Add(DriveItemsSerializer.SerializeDriveItem(item));
            }

            foreach(TagCompound notFoundCompound in notFoundItems)
            {
                itemsCompound.Add(notFoundCompound);
            }
            notFoundItems.Clear();

            IList<TagCompound> generatorsCompound = new List<TagCompound>();

            Dictionary<int, int> generators = SatelliteStorage.generatorsSystem.GetGeneratorsInv();
            foreach (int key in generators.Keys)
            {
                TagCompound generatorCompound = new TagCompound
                {
                    { "type", key },
                    { "count", generators[key] }
                };

                generatorsCompound.Add(generatorCompound);
            }

            tag.Set("SatelliteStorage_DriveChestItems", itemsCompound);
            tag.Set("SatelliteStorage_IsSputnikPlaced", SatelliteStorage.driveChestSystem.isSputnikPlaced);
            tag.Set("SatelliteStorage_Generators", generatorsCompound);
            tag.Set("SatelliteStorage_Version", SatelliteStorage.ModVersion);

            base.SaveWorldData(tag);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            SatelliteStorage.Debug("Loading world data");

            SatelliteStorage.driveChestSystem.isSputnikPlaced = false;
            SatelliteStorage.driveChestSystem.ClearItems();

            SatelliteStorage.generatorsSystem.ClearInv();

            IList<TagCompound> items = tag.GetList<TagCompound>("SatelliteStorage_DriveChestItems");
            IList<TagCompound> generatorsCompound = tag.GetList<TagCompound>("SatelliteStorage_Generators");
            SatelliteStorage.driveChestSystem.isSputnikPlaced = tag.GetBool("SatelliteStorage_IsSputnikPlaced");
            int modversion = tag.GetInt("SatelliteStorage_Version");

            List<IDriveItem> loadedItems = new List<IDriveItem>();


            for(int i = 0; i < items.Count; i++)
            {
                TagCompound itemCompound = items[i];
                IDriveItem item = DriveItemsSerializer.DeserializeDriveItem(itemCompound, modversion);
                if (item != null) loadedItems.Add(item);
                else
                {
                    notFoundItems.Add(itemCompound);
                }
            }

            Dictionary<int, int> generators = SatelliteStorage.generatorsSystem.GetGeneratorsInv();
            foreach (TagCompound generatorCompound in generatorsCompound)
            {
                generators[generatorCompound.GetInt("type")] = generatorCompound.GetInt("count");
            }

            SatelliteStorage.driveChestSystem.InitItems(loadedItems);
            SatelliteStorage.generatorsSystem.InitGeneratorsInv(generators);

            base.LoadWorldData(tag);
        }

        public override void OnWorldUnload()
        {
            base.OnWorldUnload();
            SatelliteStorage.driveChestSystem.ClearItems();
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

                SatelliteStorage.generatorsSystem.OnGeneratorsTick();
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
                    SatelliteStorage.generatorsSystem.OnGeneratorsTick();
                }
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (!requestStates)
                {
                    requestStates = true;

                    Player player = Main.LocalPlayer;

                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)MessageType.RequestStates);
                    packet.Write((byte)player.whoAmI);
                    packet.Send();
                    packet.Close();
                }
            }

            base.PostUpdateEverything();
        }
    }
}
