namespace NzbDrone.Core.ImportLists
{
    public enum ImportListSyncLevelType
    {
        Disabled = 0,
        LogOnly = 1,
        KeepAndUnmonitor = 2,
        KeepAndMonitor = 3,
        RemoveAndKeep = 4,
        RemoveAndDelete = 5
    }
}
