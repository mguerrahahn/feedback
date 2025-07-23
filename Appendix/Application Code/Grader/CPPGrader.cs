using System.Diagnostics;
using System.Text;

namespace Grader;
internal class CPPGrader : UnitGrader
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

            string tests = File.ReadAllText(root + "\\tests.cpp.txt");

            foreach (FileInfo f in di.GetFiles("*.cpp"))
            {
                Console.WriteLine(f.FullName);

                string code = File.ReadAllText(f.FullName);

                code = code.Replace("main", "run_cigarras");

                string newcode = tests.Replace("# Here", code);

                int unitGrade = RunCode(root, newcode, "cpp", "5");

                string expected = File.ReadAllText(files.First());

                Stopwatch stopwatch = Stopwatch.StartNew();
                int gradeGPT = 0; // GPTGrader.Grade("java", newjavacode, tests, expected);
                stopwatch.Stop();
                double time = stopwatch.Elapsed.TotalSeconds;

                Console.WriteLine($"Grade Unit: {unitGrade}%, Grade GPT: {gradeGPT}%, Time: {time}");

                results.Add((di.Name, unitGrade, gradeGPT, time));
            }
        }
        ExportToCsv($"{root.FullName} - CPP - {d.Name}", results);
    }
}
