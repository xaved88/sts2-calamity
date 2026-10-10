using MegaCrit.Sts2.Core.Map;

namespace Calamity.CalamityCode.Map;

/// <summary>
/// Wraps a freshly generated vanilla map and inserts three full rows:
/// * a Trial row right after the Ancient (the first floor of the act),
/// * a Calamity row directly below the treasure row,
/// * a Calamity row between the final rest row and the boss.
///
/// Every path crosses all three. Each inserted point is wired 1:1 to the point it precedes, so the path topology
/// (branching, crossings) is exactly vanilla's, and no RNG is consumed, so multiplayer clients stay in sync.
/// </summary>
public sealed class CalamityActMap : ActMap
{
    // StandardActMap.AssignPointTypes puts the treasure row (or Warden-ascension elites) at RowCount - 7.
    private const int TreasureRowOffsetFromTop = 7;

    private readonly MapPoint?[,] _grid;

    public override MapPoint BossMapPoint { get; }

    public override MapPoint StartingMapPoint { get; }

    public override MapPoint? SecondBossMapPoint { get; }

    protected override MapPoint?[,] Grid => _grid;

    public CalamityActMap(ActMap source)
    {
        BossMapPoint = source.BossMapPoint;
        StartingMapPoint = source.StartingMapPoint;
        SecondBossMapPoint = source.SecondBossMapPoint;

        int cols = source.GetColumnCount();
        int rows = source.GetRowCount();
        int treasureRow = rows - TreasureRowOffsetFromTop;
        int restRow = rows - 1;

        List<MapPoint> firstRow = source.GetPointsInRow(1).ToList();
        List<MapPoint> treasurePoints = source.GetPointsInRow(treasureRow).ToList();
        List<MapPoint> restPoints = source.GetPointsInRow(restRow).ToList();

        // Shift rows to make room. MapPoint hashes by reference, so mutating coords in place is safe (vanilla
        // post-processing does the same).
        foreach (MapPoint point in source.GetAllMapPoints())
        {
            point.coord.row += (point.coord.row >= 1 ? 1 : 0) + (point.coord.row >= treasureRow ? 1 : 0);
        }
        BossMapPoint.coord.row += 3;
        if (SecondBossMapPoint != null)
        {
            SecondBossMapPoint.coord.row += 3;
        }

        _grid = new MapPoint?[cols, rows + 3];
        foreach (MapPoint point in source.GetAllMapPoints())
        {
            _grid[point.coord.col, point.coord.row] = point;
        }

        // Ancient -> [trial] -> first fight. The trial points become the paths' starting points.
        foreach (MapPoint first in firstRow)
        {
            MapPoint trial = CreatePoint(first.coord.col, 1, CalamityMapPoint.Trial);
            StartingMapPoint.RemoveChildPoint(first);
            StartingMapPoint.AddChildPoint(trial);
            trial.AddChildPoint(first);
            startMapPoints.Add(trial);
        }

        // ... -> [calamity] -> treasure
        foreach (MapPoint treasure in treasurePoints)
        {
            MapPoint calamity = CreatePoint(treasure.coord.col, treasureRow + 1, CalamityMapPoint.Type);
            foreach (MapPoint parent in treasure.parents.ToList())
            {
                parent.RemoveChildPoint(treasure);
                parent.AddChildPoint(calamity);
            }
            calamity.AddChildPoint(treasure);
        }

        // rest -> [calamity] -> boss
        foreach (MapPoint rest in restPoints)
        {
            MapPoint calamity = CreatePoint(rest.coord.col, rows + 2, CalamityMapPoint.Type);
            rest.RemoveChildPoint(BossMapPoint);
            rest.AddChildPoint(calamity);
            calamity.AddChildPoint(BossMapPoint);
        }
    }

    private MapPoint CreatePoint(int col, int row, MapPointType type)
    {
        var point = new MapPoint(col, row)
        {
            PointType = type,
            CanBeModified = false,
        };
        _grid[col, row] = point;
        return point;
    }
}
