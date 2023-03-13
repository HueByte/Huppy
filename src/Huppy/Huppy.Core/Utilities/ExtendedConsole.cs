using System.Text;

namespace Huppy.Core.Utilities
{
    public static class ExtendedConsole
    {
        public static void Table<T>(List<T?> input, int offset = 20) where T : class
        {
            if (input is null) Console.WriteLine();
            StringBuilder sb = new();

            // gets <T> type
            var listType = input!.GetType().GetGenericArguments().FirstOrDefault();
            if (listType is null) Console.WriteLine();

            var properties = listType!.GetProperties();

            var heading = string.Join(" | ", properties.Select(e => FillToOffset(e.Name, offset)));
            sb.AppendLine(heading);
            sb.AppendLine(string.Concat(Enumerable.Repeat("~", heading.Length)));

            foreach (var item in input)
            {
                object?[] propValues = properties.Select(e => e.GetValue(item, null)).ToArray();
                sb.AppendJoin(" | ", propValues.Select(e => FillToOffset(string.IsNullOrEmpty(e?.ToString()) ? "" : e.ToString()!, offset)));
                sb.AppendLine();
            }

            Console.WriteLine(sb.ToString());
        }

        private static string FillToOffset(string input, int offset) =>
            string.Concat(input, string.Concat(Enumerable.Repeat(" ", offset - input.Length)));
    }
}