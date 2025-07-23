// See https://aka.ms/new-console-template for more information
using Grader;

string[] folders = File.ReadAllLines("../../../folders.txt");
List<string> subFolders = ["ACCEPTED", "RUNTIME_ERROR", "WRONG_ANSWER"];
foreach(string folder in folders)
{
    DirectoryInfo dirInfo = new(folder);
    string[] expected = [.. dirInfo.GetFiles("expected*").Select(x => x.FullName)];
    foreach (string subFolder in subFolders)
    {
        string fullPath = Path.Combine(folder, subFolder);
        JavaGrader.Grade(fullPath, expected);
        PythonGrader.Grade(fullPath, expected);
        CGrader.Grade(fullPath, expected);
        CPPGrader.Grade(fullPath, expected);
    }
}