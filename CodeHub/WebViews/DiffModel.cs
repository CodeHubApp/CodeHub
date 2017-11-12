using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeHub.WebViews
{
    public class DiffModel
    {
        static Regex ContextRegex = new Regex("^@@ -(\\d+).+\\+(\\d+)");

        public List<Context> Chunks { get; }

        public List<DiffCommentModel> FileComments { get; }

        public int FontSize { get; }

        public DiffModel(
            IEnumerable<string> patchLines,
            IEnumerable<DiffCommentModel> comments,
            int fontSize)
        {
            FontSize = fontSize;

            var diffComments = comments.GroupBy(x => x.LineFrom).ToDictionary(x => x.Key ?? -1, x => x.ToList());
            FileComments = diffComments.ContainsKey(-1) ? diffComments[-1] : new List<DiffCommentModel>();
            Chunks = ParsePatchLines(patchLines, diffComments).ToList();
        }

        static IEnumerable<Context> ParsePatchLines(IEnumerable<string> patchLines, Dictionary<int, List<DiffCommentModel>> comments)
        {
            int baseLine = 0;
            int newLine = 0;
            string contextLine = null;
            LinkedList<Line> lines = null;

            foreach (var patchLine in patchLines.Select((line, idx) => new { line, idx }))
            {
                var line = patchLine.line;
                var idx = patchLine.idx;

                if (line.StartsWith("@@", StringComparison.Ordinal))
                {
                    if (lines != null)
                        yield return new Context(contextLine, lines);

                    lines = new LinkedList<Line>();
                    var match = ContextRegex.Match(line);
                    int.TryParse(match.Groups[1].Value, out baseLine);
                    int.TryParse(match.Groups[2].Value, out newLine);
                    contextLine = line;
                    continue;
                }

                if (lines == null)
                    continue;


                comments.TryGetValue(idx, out List<DiffCommentModel> lineComments);
            
                if (line.StartsWith("+", StringComparison.Ordinal))
                {
                    lines.AddLast(new Line(baseLine, newLine, LineEquality.Insert, line.Substring(1), idx, lineComments));
                    newLine++;
                }
                else if (line.StartsWith("-", StringComparison.Ordinal))
                {
                    lines.AddLast(new Line(baseLine, newLine, LineEquality.Delete, line.Substring(1), idx, lineComments));
                    baseLine++;
                }
                else if (line.StartsWith("\\", StringComparison.Ordinal))
                {
                    continue;
                }
                else
                {
                    lines.AddLast(new Line(baseLine, newLine, LineEquality.Equal, line.Substring(1), idx, lineComments));
                    baseLine++;
                    newLine++;
                }
            }

            if (lines != null)
                yield return new Context(contextLine, lines);
        }

        public class Context
        {
            public string Content { get; }
            public List<Line> Lines { get; }

            public Context(string content, IEnumerable<Line> lines)
            {
                Content = content;
                Lines = lines.ToList();
            }
        }

        public class Line
        {
            public int? BaseLine { get; }
            public int? NewLine { get; }
            public LineEquality LineEquality { get; }
            public string Content { get; }
            public List<KeyValuePair<int, List<DiffCommentModel>>> CommentSets { get; }
            public int Index { get; }

            public Line(int? baseLine, int? newLine, LineEquality lineEquality, string content, int index, IEnumerable<DiffCommentModel> comments)
            {
                BaseLine = baseLine;
                NewLine = newLine;
                LineEquality = lineEquality;
                Content = content;
                Index = index;

                CommentSets = (comments ?? Enumerable.Empty<DiffCommentModel>())
                    .GroupBy(x => x.GroupId ?? index)
                    .ToDictionary(x => x.Key, x => x.ToList())
                    .ToList();
            }
        }

        public enum LineEquality
        {
            Equal,
            Insert,
            Delete
        }
    }
}
