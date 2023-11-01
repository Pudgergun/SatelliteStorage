using Microsoft.Xna.Framework;

namespace SatelliteStorage.DriveSystem
{
    interface IDriveChestPositionState
    {
        bool positionChecking { get; }
        Vector2 openedPosition { get; }

        void ResetOpenedPosition();
        void SetOpenedPosition(Vector2 pos);
    }
}