using System.Diagnostics;
using System.Text;

namespace Grader;
public class JavaGrader: UnitGrader
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

            string tests = File.ReadAllText(root + "\\tests.java.txt");

            foreach (FileInfo f in di.GetFiles("*.java"))
            {
                Console.WriteLine(f.FullName);

                string javacode = File.ReadAllText(f.FullName);
                string newjavacode = TransformJavaWithTestHarness(javacode, tests);

                int unitGrade = RunCode(root, newjavacode, "java", "4");

                string expected = File.ReadAllText(files.First());

                Stopwatch stopwatch = Stopwatch.StartNew();
                int gradeGPT = 0; // GPTGrader.Grade("java", newjavacode, tests, expected);
                stopwatch.Stop();
                double time = stopwatch.Elapsed.TotalSeconds;

                Console.WriteLine($"Grade Unit: {unitGrade}%, Grade GPT: {gradeGPT}%, Time: {time}");

                results.Add((di.Name, unitGrade, gradeGPT, time));
            }
        }
        ExportToCsv($"{root.FullName} - Java - {d.Name}", results);
    }

    static string TransformJavaWithTestHarness(string javaCode, string tests)
    {
        javaCode = SetPublic(javaCode);

        javaCode = javaCode.Replace("void main", "void main1");

        int insertIndex = javaCode.IndexOf("public static void main");
        if (insertIndex == -1)
            insertIndex = javaCode.IndexOf("public static String main");

        StringBuilder sb = new();

        _ = sb.Append(javaCode.AsSpan(0, insertIndex));

        _ = sb.Append(tests);

        _ = sb.Append(javaCode.AsSpan(insertIndex));

        return sb.ToString();
    }

    static string SetPublic(string javaCode)
    {
        if (javaCode.Contains("public class"))
            return javaCode;

        string[] lines = javaCode.Split('\n');
        int mainLineIndex = -1;
        int classLineIndex = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("void main"))
            {
                mainLineIndex = i;
                break;
            }
        }

        if (mainLineIndex != -1)
        {
            for (int i = mainLineIndex; i >= 0; i--)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("class ") && !line.StartsWith("public class"))
                {
                    classLineIndex = i;
                    break;
                }
            }
        }

        if (classLineIndex != -1)
        {
            lines[classLineIndex] = lines[classLineIndex].Replace("class", "public class");
        }

        string modified = string.Join('\n', lines);

        return modified;
    }
}
