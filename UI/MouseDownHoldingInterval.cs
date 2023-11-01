namespace SatelliteStorage.UI
{
    public partial class UIDriveChest
    {
        class MouseDownHoldingInterval : IMouseDownHoldingInterval
        {
            private double _cachedTime = 0;
            private double _prevTime = 0;
            private double _cooldown = 0;
            private double _intervalBaseValue = 192;

            public MouseDownHoldingInterval()
            {
            }

            public void SetCooldown(double cooldown)
            {
                _cooldown = cooldown;
            }

            public bool CheckIsReady(double totalMilliseconds)
            {
                return CheckIsReady(totalMilliseconds, out var multiplier);
            }

            public void Reset(double totalMilliseconds)
            {
                _cachedTime = totalMilliseconds;
                _prevTime = totalMilliseconds;
            }

            public bool CheckIsReady(double totalMilliseconds, out int multiplier)
            {
                double elapsedTime = totalMilliseconds - _cachedTime;

                int intervalDivider = elapsedTime switch
                {
                    >= 3000 => 12,
                    >= 2500 => 9,
                    >= 2000 => 6,
                    >= 1500 => 3,
                    >= 1000 => 1,
                    _ => 0,
                };


                if (
                    intervalDivider <= 0 || 
                    totalMilliseconds <= _prevTime + _cooldown + _intervalBaseValue / intervalDivider
                )
                {
                    multiplier = 0;
                    return false;
                }

                multiplier = intervalDivider;

                _prevTime = totalMilliseconds;
                return true;
            }
        }
    }
}
