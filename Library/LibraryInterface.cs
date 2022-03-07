using System.Collections.Generic;

namespace CSharpToLua.Library
{
    internal class Library
    {
        public static List<LibraryInterface> InUse = new();
        public static Dictionary<string, LibraryInterface> LibraryDict = new();
    }

    internal interface LibraryInterface
    {
        public void Call()
        {
        }

        public string OnCall(string name)
        {
            return "nil";
        }
    }
}