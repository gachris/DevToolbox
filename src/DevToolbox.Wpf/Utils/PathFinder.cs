using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Utils;

// Note: I couldn't find a useful open source library that does
// orthogonal routing so started to write something on my own.
// Categorize this as a quick and dirty short term solution.
// I will keep on searching.

// Helper class to provide an orthogonal connection path
internal class PathFinder
{
    private const int margin = 20;

    internal static List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine)
    {
        List<Point> linePoints = new List<Point>();

        Rect rectSource = GetRectWithMargin(source, margin);
        Rect rectSink = GetRectWithMargin(sink, margin);

        Point startPoint = GetOffsetPoint(source, rectSource);
        Point endPoint = GetOffsetPoint(sink, rectSink);

        linePoints.Add(startPoint);
        Point currentPoint = startPoint;

        if (!rectSink.Contains(currentPoint) && !rectSource.Contains(endPoint))
        {
            while (true)
            {
                #region source node

                if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource, rectSink }))
                {
                    linePoints.Add(endPoint);
                    currentPoint = endPoint;
                    break;
                }

                Point neighbour = GetNearestVisibleNeighborSink(currentPoint, endPoint, sink, rectSource, rectSink);
                if (!double.IsNaN(neighbour.X))
                {
                    linePoints.Add(neighbour);
                    linePoints.Add(endPoint);
                    currentPoint = endPoint;
                    break;
                }

                if (currentPoint == startPoint)
                {
                    Point n = GetNearestNeighborSource(source, endPoint, rectSource, rectSink, out bool flag);
                    linePoints.Add(n);
                    currentPoint = n;

                    if (!IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                    {
                        GetOppositeCorners(source.Orientation, rectSource, out Point n1, out Point n2);
                        if (flag)
                        {
                            linePoints.Add(n1);
                            currentPoint = n1;
                        }
                        else
                        {
                            linePoints.Add(n2);
                            currentPoint = n2;
                        }
                        if (!IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                        {
                            if (flag)
                            {
                                linePoints.Add(n2);
                                currentPoint = n2;
                            }
                            else
                            {
                                linePoints.Add(n1);
                                currentPoint = n1;
                            }
                        }
                    }
                }
                #endregion

                #region sink node

                else // from here on we jump to the sink node
                {
                    // neighbour corner
                    // opposite corner
                    GetNeighborCorners(sink.Orientation, rectSink, out Point s1, out Point s2);
                    GetOppositeCorners(sink.Orientation, rectSink, out Point n1, out Point n2);

                    bool n1Visible = IsPointVisible(currentPoint, n1, new Rect[] { rectSource, rectSink });
                    bool n2Visible = IsPointVisible(currentPoint, n2, new Rect[] { rectSource, rectSink });

                    if (n1Visible && n2Visible)
                    {
                        if (rectSource.Contains(n1))
                        {
                            linePoints.Add(n2);
                            if (rectSource.Contains(s2))
                            {
                                linePoints.Add(n1);
                                linePoints.Add(s1);
                            }
                            else
                                linePoints.Add(s2);

                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }

                        if (rectSource.Contains(n2))
                        {
                            linePoints.Add(n1);
                            if (rectSource.Contains(s1))
                            {
                                linePoints.Add(n2);
                                linePoints.Add(s2);
                            }
                            else
                                linePoints.Add(s1);

                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }

                        if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
                        {
                            linePoints.Add(n1);
                            if (rectSource.Contains(s1))
                            {
                                linePoints.Add(n2);
                                linePoints.Add(s2);
                            }
                            else
                                linePoints.Add(s1);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                        else
                        {
                            linePoints.Add(n2);
                            if (rectSource.Contains(s2))
                            {
                                linePoints.Add(n1);
                                linePoints.Add(s1);
                            }
                            else
                                linePoints.Add(s2);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                    }
                    else if (n1Visible)
                    {
                        linePoints.Add(n1);
                        if (rectSource.Contains(s1))
                        {
                            linePoints.Add(n2);
                            linePoints.Add(s2);
                        }
                        else
                            linePoints.Add(s1);
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }
                    else
                    {
                        linePoints.Add(n2);
                        if (rectSource.Contains(s2))
                        {
                            linePoints.Add(n1);
                            linePoints.Add(s1);
                        }
                        else
                            linePoints.Add(s2);
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }
                }
                #endregion
            }
        }
        else
        {
            linePoints.Add(endPoint);
        }

        linePoints = OptimizeLinePoints(linePoints, new Rect[] { rectSource, rectSink }, source.Orientation, sink.Orientation);

        CheckPathEnd(source, sink, showLastLine, linePoints);
        return linePoints;
    }

    internal static List<Point> GetConnectionLine(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation)
    {
        List<Point> linePoints = new List<Point>();
        Rect rectSource = GetRectWithMargin(source, 10);
        Point startPoint = GetOffsetPoint(source, rectSource);
        Point endPoint = sinkPoint;

        linePoints.Add(startPoint);
        Point currentPoint = startPoint;

        if (!rectSource.Contains(endPoint))
        {
            while (true)
            {
                if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                {
                    linePoints.Add(endPoint);
                    break;
                }

                Point n = GetNearestNeighborSource(source, endPoint, rectSource, out bool sideFlag);
                linePoints.Add(n);
                currentPoint = n;

                if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                {
                    linePoints.Add(endPoint);
                    break;
                }
                else
                {
                    GetOppositeCorners(source.Orientation, rectSource, out Point n1, out Point n2);
                    if (sideFlag)
                        linePoints.Add(n1);
                    else
                        linePoints.Add(n2);

                    linePoints.Add(endPoint);
                    break;
                }
            }
        }
        else
        {
            linePoints.Add(endPoint);
        }

        linePoints = preferredOrientation != ConnectorOrientation.None
            ? OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Orientation, preferredOrientation)
            : OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Orientation, GetOpositeOrientation(source.Orientation));

        return linePoints;
    }

    private static List<Point> OptimizeLinePoints(List<Point> linePoints, Rect[] rectangles, ConnectorOrientation sourceOrientation, ConnectorOrientation sinkOrientation)
    {
        List<Point> points = new List<Point>();
        int cut = 0;

        for (int i = 0; i < linePoints.Count; i++)
        {
            if (i >= cut)
            {
                for (int k = linePoints.Count - 1; k > i; k--)
                {
                    if (IsPointVisible(linePoints[i], linePoints[k], rectangles))
                    {
                        cut = k;
                        break;
                    }
                }
                points.Add(linePoints[i]);
            }
        }

        #region Line
        for (int j = 0; j < points.Count - 1; j++)
        {
            if (points[j].X != points[j + 1].X && points[j].Y != points[j + 1].Y)
            {
                ConnectorOrientation orientationFrom;
                ConnectorOrientation orientationTo;

                // orientation from point
                orientationFrom = j == 0 ? sourceOrientation : GetOrientation(points[j], points[j - 1]);

                // orientation to pint 
                orientationTo = j == points.Count - 2 ? sinkOrientation : GetOrientation(points[j + 1], points[j + 2]);


                if ((orientationFrom == ConnectorOrientation.Left || orientationFrom == ConnectorOrientation.Right) &&
                    (orientationTo == ConnectorOrientation.Left || orientationTo == ConnectorOrientation.Right))
                {
                    double centerX = Math.Min(points[j].X, points[j + 1].X) + Math.Abs(points[j].X - points[j + 1].X) / 2;
                    points.Insert(j + 1, new Point(centerX, points[j].Y));
                    points.Insert(j + 2, new Point(centerX, points[j + 2].Y));
                    if (points.Count - 1 > j + 3)
                        points.RemoveAt(j + 3);
                    return points;
                }

                if ((orientationFrom == ConnectorOrientation.Top || orientationFrom == ConnectorOrientation.Bottom) &&
                    (orientationTo == ConnectorOrientation.Top || orientationTo == ConnectorOrientation.Bottom))
                {
                    double centerY = Math.Min(points[j].Y, points[j + 1].Y) + Math.Abs(points[j].Y - points[j + 1].Y) / 2;
                    points.Insert(j + 1, new Point(points[j].X, centerY));
                    points.Insert(j + 2, new Point(points[j + 2].X, centerY));
                    if (points.Count - 1 > j + 3)
                        points.RemoveAt(j + 3);
                    return points;
                }

                if ((orientationFrom == ConnectorOrientation.Left || orientationFrom == ConnectorOrientation.Right) &&
                    (orientationTo == ConnectorOrientation.Top || orientationTo == ConnectorOrientation.Bottom))
                {
                    points.Insert(j + 1, new Point(points[j + 1].X, points[j].Y));
                    return points;
                }

                if ((orientationFrom == ConnectorOrientation.Top || orientationFrom == ConnectorOrientation.Bottom) &&
                    (orientationTo == ConnectorOrientation.Left || orientationTo == ConnectorOrientation.Right))
                {
                    points.Insert(j + 1, new Point(points[j].X, points[j + 1].Y));
                    return points;
                }
            }
        }
        #endregion

        return points;
    }

    private static ConnectorOrientation GetOrientation(Point p1, Point p2)
    {
        if (p1.X == p2.X)
        {
            return p1.Y >= p2.Y ? ConnectorOrientation.Bottom : ConnectorOrientation.Top;
        }
        else if (p1.Y == p2.Y)
        {
            return p1.X >= p2.X ? ConnectorOrientation.Right : ConnectorOrientation.Left;
        }
        throw new Exception("Failed to retrieve orientation");
    }

    private static Orientation GetOrientation(ConnectorOrientation sourceOrientation)
    {
        return sourceOrientation switch
        {
            ConnectorOrientation.Left => Orientation.Horizontal,
            ConnectorOrientation.Top => Orientation.Vertical,
            ConnectorOrientation.Right => Orientation.Horizontal,
            ConnectorOrientation.Bottom => Orientation.Vertical,
            _ => throw new Exception("Unknown ConnectorOrientation"),
        };
    }

    private static Point GetNearestNeighborSource(ConnectorInfo source, Point endPoint, Rect rectSource, Rect rectSink, out bool flag)
    {
        // neighbors
        GetNeighborCorners(source.Orientation, rectSource, out Point n1, out Point n2);

        if (rectSink.Contains(n1))
        {
            flag = false;
            return n2;
        }

        if (rectSink.Contains(n2))
        {
            flag = true;
            return n1;
        }

        if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
        {
            flag = true;
            return n1;
        }
        else
        {
            flag = false;
            return n2;
        }
    }

    private static Point GetNearestNeighborSource(ConnectorInfo source, Point endPoint, Rect rectSource, out bool flag)
    {
        // neighbors
        GetNeighborCorners(source.Orientation, rectSource, out Point n1, out Point n2);

        if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
        {
            flag = true;
            return n1;
        }
        else
        {
            flag = false;
            return n2;
        }
    }

    private static Point GetNearestVisibleNeighborSink(Point currentPoint, Point endPoint, ConnectorInfo sink, Rect rectSource, Rect rectSink)
    {
        // neighbors on sink side
        GetNeighborCorners(sink.Orientation, rectSink, out Point s1, out Point s2);

        bool flag1 = IsPointVisible(currentPoint, s1, new Rect[] { rectSource, rectSink });
        bool flag2 = IsPointVisible(currentPoint, s2, new Rect[] { rectSource, rectSink });

        return flag1
            ? flag2
                ? rectSink.Contains(s1) ? s2 : rectSink.Contains(s2) ? s1 : Distance(s1, endPoint) <= Distance(s2, endPoint) ? s1 : s2
                : s1
            : flag2 ? s2 : new Point(double.NaN, double.NaN);
    }

    private static bool IsPointVisible(Point fromPoint, Point targetPoint, Rect[] rectangles)
    {
        foreach (Rect rect in rectangles)
        {
            if (RectangleIntersectsLine(rect, fromPoint, targetPoint))
                return false;
        }
        return true;
    }

    private static bool IsRectVisible(Point fromPoint, Rect targetRect, Rect[] rectangles) => IsPointVisible(fromPoint, targetRect.TopLeft, rectangles)
            ? true
            : IsPointVisible(fromPoint, targetRect.TopRight, rectangles)
            ? true
            : IsPointVisible(fromPoint, targetRect.BottomLeft, rectangles)
            ? true
            : IsPointVisible(fromPoint, targetRect.BottomRight, rectangles);

    private static bool RectangleIntersectsLine(Rect rect, Point startPoint, Point endPoint)
    {
        rect.Inflate(-1, -1);
        return rect.IntersectsWith(new Rect(startPoint, endPoint));
    }

    private static void GetOppositeCorners(ConnectorOrientation orientation, Rect rect, out Point n1, out Point n2)
    {
        switch (orientation)
        {
            case ConnectorOrientation.Left:
                n1 = rect.TopRight; n2 = rect.BottomRight;
                break;
            case ConnectorOrientation.Top:
                n1 = rect.BottomLeft; n2 = rect.BottomRight;
                break;
            case ConnectorOrientation.Right:
                n1 = rect.TopLeft; n2 = rect.BottomLeft;
                break;
            case ConnectorOrientation.Bottom:
                n1 = rect.TopLeft; n2 = rect.TopRight;
                break;
            default:
                throw new Exception("No opposite corners found!");
        }
    }

    private static void GetNeighborCorners(ConnectorOrientation orientation, Rect rect, out Point n1, out Point n2)
    {
        switch (orientation)
        {
            case ConnectorOrientation.Left:
                n1 = rect.TopLeft; n2 = rect.BottomLeft;
                break;
            case ConnectorOrientation.Top:
                n1 = rect.TopLeft; n2 = rect.TopRight;
                break;
            case ConnectorOrientation.Right:
                n1 = rect.TopRight; n2 = rect.BottomRight;
                break;
            case ConnectorOrientation.Bottom:
                n1 = rect.BottomLeft; n2 = rect.BottomRight;
                break;
            default:
                throw new Exception("No neighour corners found!");
        }
    }

    private static double Distance(Point p1, Point p2) => Point.Subtract(p1, p2).Length;

    private static Rect GetRectWithMargin(ConnectorInfo connectorThumb, double margin)
    {
        Rect rect = new Rect(connectorThumb.Left,
                             connectorThumb.Top,
                             connectorThumb.Size.Width,
                             connectorThumb.Size.Height);

        rect.Inflate(margin, margin);

        return rect;
    }

    private static Point GetOffsetPoint(ConnectorInfo connector, Rect rect)
    {
        Point offsetPoint = new Point();

        switch (connector.Orientation)
        {
            case ConnectorOrientation.Left:
                offsetPoint = new Point(rect.Left, connector.Position.Y);
                break;
            case ConnectorOrientation.Top:
                offsetPoint = new Point(connector.Position.X, rect.Top);
                break;
            case ConnectorOrientation.Right:
                offsetPoint = new Point(rect.Right, connector.Position.Y);
                break;
            case ConnectorOrientation.Bottom:
                offsetPoint = new Point(connector.Position.X, rect.Bottom);
                break;
            default:
                break;
        }

        return offsetPoint;
    }

    private static void CheckPathEnd(ConnectorInfo source, ConnectorInfo sink, bool showLastLine, List<Point> linePoints)
    {
        if (showLastLine)
        {
            Point startPoint = new Point(0, 0);
            Point endPoint = new Point(0, 0);
            double marginPath = 15;
            switch (source.Orientation)
            {
                case ConnectorOrientation.Left:
                    startPoint = new Point(source.Position.X - marginPath, source.Position.Y);
                    break;
                case ConnectorOrientation.Top:
                    startPoint = new Point(source.Position.X, source.Position.Y - marginPath);
                    break;
                case ConnectorOrientation.Right:
                    startPoint = new Point(source.Position.X + marginPath, source.Position.Y);
                    break;
                case ConnectorOrientation.Bottom:
                    startPoint = new Point(source.Position.X, source.Position.Y + marginPath);
                    break;
                default:
                    break;
            }

            switch (sink.Orientation)
            {
                case ConnectorOrientation.Left:
                    endPoint = new Point(sink.Position.X - marginPath, sink.Position.Y);
                    break;
                case ConnectorOrientation.Top:
                    endPoint = new Point(sink.Position.X, sink.Position.Y - marginPath);
                    break;
                case ConnectorOrientation.Right:
                    endPoint = new Point(sink.Position.X + marginPath, sink.Position.Y);
                    break;
                case ConnectorOrientation.Bottom:
                    endPoint = new Point(sink.Position.X, sink.Position.Y + marginPath);
                    break;
                default:
                    break;
            }
            linePoints.Insert(0, startPoint);
            linePoints.Add(endPoint);
        }
        else
        {
            linePoints.Insert(0, source.Position);
            linePoints.Add(sink.Position);
        }
    }

    private static ConnectorOrientation GetOpositeOrientation(ConnectorOrientation connectorOrientation)
    {
        return connectorOrientation switch
        {
            ConnectorOrientation.Left => ConnectorOrientation.Right,
            ConnectorOrientation.Top => ConnectorOrientation.Bottom,
            ConnectorOrientation.Right => ConnectorOrientation.Left,
            ConnectorOrientation.Bottom => ConnectorOrientation.Top,
            _ => ConnectorOrientation.Top,
        };
    }
}
