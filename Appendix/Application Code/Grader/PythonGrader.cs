using System.Diagnostics;

namespace Grader;
internal class PythonGrader : UnitGrader
{
    public static void Grade(string dir, string[] files)
    {
        DirectoryInfo d = new(dir);
        DirectoryInfo root = new(d.FullName + @"\..");
        List<(string FileName, double Grade, int GradeGPT, double time)> results = [];

        foreach (DirectoryInfo di in d.GetDirectories())
        {
            if (results.Count > 3)
                break;

            string tests = File.ReadAllText(root + "\\tests.python.txt");

            foreach (FileInfo f in di.GetFiles("*.py"))
            {
                Console.WriteLine(f.FullName);

                string[] lines = File.ReadAllLines(f.FullName);

                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = "\t" + lines[i];
                }
                string code = string.Join(Environment.NewLine, lines);

                string newcode = tests.Replace("# Here", code);

                int unitGrade = RunCode(root, newcode, "python3", "4");

                string expected = File.ReadAllText(files.First());

                Stopwatch stopwatch = Stopwatch.StartNew();
                int gradeGPT = 0; // GPTGrader.Grade("java", newjavacode, tests, expected);
                stopwatch.Stop();
                double time = stopwatch.Elapsed.TotalSeconds;

                Console.WriteLine($"Grade Unit: {unitGrade}%, Grade GPT: {gradeGPT}%, Time: {time}");

                results.Add((di.Name, unitGrade, gradeGPT, time));
            }
        }
        ExportToCsv($"{root.FullName} - Python - {d.Name}", results);
    }
}
