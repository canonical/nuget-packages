namespace Canonical.Madison;

public static class MadisonResponseExtensions
{
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupByPackageName(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.PackageName);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupBySuite(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.Suite);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupBySeries(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.Series);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupByComponent(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.Component);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupByArchitecture(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries
            .SelectMany(entry => entry.Architectures.Select(arch => (arch, entry)))
            .GroupBy(x => x.arch, x => x.entry);
    }
}