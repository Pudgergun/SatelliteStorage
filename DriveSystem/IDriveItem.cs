using Terraria;

namespace SatelliteStorage.DriveSystem
{
    public interface IDriveItem
    {
        int stack { get; }
        int type { get; }
        int prefix { get; }
        int context { get; }
        int recipe { get; }

        string stackText { get; }

        IDriveItem SetPrefix(int prefix);
        IDriveItem SetStack(int stack);
        IDriveItem AddStack(int count);
        IDriveItem SubStack(int count);
        IDriveItem SetType(int type);
        IDriveItem SetContext(int context);
        IDriveItem SetRecipe(int recipe);
        Item ToItem();
    }
}