namespace SatelliteStorage.ModNetwork
{
    public enum MessageType : byte
    {
        RequestDriveChestItems,
        ResponseDriveChestItems,
        TakeDriveChestItem,
        AddDriveChestItem,
        SyncDriveChestItem,
        DepositDriveChestItem,
        TryCraftRecipe,
        SetSputnikState,
        RequestSputnikState,
        ChangeGeneratorState,
        SyncGeneratorState,
        RequestStates,
        StorageItemsCount,
    }
}