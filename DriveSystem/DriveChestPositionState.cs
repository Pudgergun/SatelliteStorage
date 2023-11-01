using Microsoft.Xna.Framework;


namespace SatelliteStorage.DriveSystem
{
    class DriveChestPositionState : IDriveChestPositionState
    {
        public DriveChestPositionState()
        {

        }

        public Vector2 openedPosition { get; private set; }
        public bool positionChecking { get; private set; } = false;

        public void SetOpenedPosition(Vector2 pos)
        {
            positionChecking = true;
            openedPosition = pos;
        }

        public void ResetOpenedPosition()
        {
            positionChecking = false;
        }
    }
}
