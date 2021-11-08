using System.Linq;

namespace MementoHealth.Classes
{
    public class SelectArea
    {
        public SelectAreaPoint Start { get; set; }
        public SelectAreaPoint End { get; set; }

        public bool IsPointInArea(SelectAreaPoint point) => IsBetween(point.X, Start.X, End.X) && IsBetween(point.Y, Start.Y, End.Y);
        public static bool IsPointInAnyArea(SelectAreaPoint point, SelectArea[] areas) => areas.Any(a => a.IsPointInArea(point));
        private static bool IsBetween(double number, double from, double to) => from <= number && number <= to;
    }

    public class SelectAreaPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}