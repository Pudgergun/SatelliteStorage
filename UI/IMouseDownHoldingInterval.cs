namespace SatelliteStorage.UI
{
    internal interface IMouseDownHoldingInterval
    {
        bool CheckIsReady(double totalMilliseconds, out int multiplier);
        bool CheckIsReady(double totalMilliseconds);
        void SetCooldown(double cooldown);
        void Reset(double totalMilliseconds);
    }
}