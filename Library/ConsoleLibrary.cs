using System;
using System.Collections.Generic;

namespace CSharpToLua.Library
{
    internal class ConsoleLibrary : LibraryInterface //Example library
    {
        public static Dictionary<string, string> ConsoleWords = new() {{"Console.WriteLine", "print"}};

        public void Call()
        {
            LuaWriter.WriteComment("Console library in use!");
        }

        public string OnCall(string name)
        {
            try
            {
                return ConsoleWords[name];
            }
            catch (Exception e)
            {
            }

            return name;
        }
    }
}