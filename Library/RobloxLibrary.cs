using System;
using System.Collections.Generic;

namespace CSharpToLua.Library
{
    internal class RobloxLibrary : LibraryInterface //Example library
    {
        public static Dictionary<string, string> PartialReplace = new() {{ ".GetChildren", ":GetChildren"}};

        public void Call()
        {
            LuaWriter.WriteComment("Roblox library in use!");
        }

        public string OnCall(string name)
        {
            foreach (var pr in PartialReplace)
            {
                name = name.Replace(pr.Key, pr.Value);
            }
            return name;
        }
    }
}