using System;
using System.IO;
using CSharpToLua.Library;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpToLua
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var src = File.ReadAllText(Environment.CurrentDirectory + "\\Scripts\\source.cs");

            var tree = CSharpSyntaxTree.ParseText(src);
            var root = tree.GetCompilationUnitRoot();

            Library.Library.LibraryDict.Add("Console", new ConsoleLibrary());
            Library.Library.LibraryDict.Add("Roblox", new RobloxLibrary());

            foreach (var node in root.Usings)
            {
                try
                {
                    Library.Library.LibraryDict[node.Name.ToString()].Call();
                    Library.Library.InUse.Add(Library.Library.LibraryDict[node.Name.ToString()]);
                }
                catch
                {
                    LuaWriter.WriteComment("Unable to use library.");
                }
            }

            SyntaxRewriter sr = new();
            var newroot = sr.Visit(root);

            CodeGen cg = new();

            cg.Visit(newroot);

            LuaWriter.sb.AppendLine(cg.EntryPoint + "();");

            Console.WriteLine(LuaWriter.sb.ToString());

            Console.ReadLine();
        }
    }
}