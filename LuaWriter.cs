using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToLua
{
    internal class LuaWriter
    {
        private static int indent;
        private static readonly int indentLevel = 3;

        public static int BlockLevel;
        public static int FunctionLevel;
        public static int SelfLevel;
        public static StringBuilder sb = new();

        public static string GetIndent()
        {
            return new string(' ', indent);
        }

        public static void WriteL()
        {
            BlockLevel++;
            sb.AppendLine($"{GetIndent()}{{");
            indent += indentLevel;
        }

        public static void WriteLUn()
        {
            BlockLevel++;
            sb.AppendLine("{");
            indent += indentLevel;
        }

        public static void WriteR()
        {
            BlockLevel--;
            indent -= indentLevel;
            sb.AppendLine($"{GetIndent()}}}");
        }

        public static void WriteGlobalTable(string global)
        {
            sb.Append($"{GetIndent()}{global} = ");
            WriteLUn();
        }

        public static void WriteClassNew(string classname)
        {
            WriteFunction(new string[]{"..."}, "new");
            sb.AppendLine($"{GetIndent()}local obj = {{}};");
            sb.AppendLine($"{GetIndent()}setmetatable(obj, self);");
            sb.AppendLine($"{GetIndent()}obj.__index = self;");
            WriteForPairs("v", classname);
            sb.AppendLine($"{GetIndent()}obj[v_index] = v;");
            WriteIf("v_index == \"constructor\"");
            sb.AppendLine($"{GetIndent()}v(obj, ...);");
            WriteEnd();
            WriteEnd();

            sb.AppendLine($"{GetIndent()}return obj;");
            WriteEnd();
        }
        public static List<string> GetAncestorPath(CSharpSyntaxNode node)
        {
            List<string> Path = new();
            foreach (var syntaxNode in node.Ancestors())
                if (syntaxNode.GetType().Name == "ClassDeclarationSyntax")
                {
                    var classDeclaration = (ClassDeclarationSyntax) syntaxNode;
                    Path.Add(classDeclaration.Identifier.Text);
                }

            Path.Reverse();

            return Path;
        }

        public static bool InsideFunction()
        {
            return FunctionLevel > 0;
        }

        public static bool InsideBlock()
        {
            return BlockLevel > 0;
        }
        public static bool UseSelf()
        {
            return SelfLevel > 0;
        }
        public static void WriteTableSep()
        {
            sb.Append($"{GetIndent()},\n");
        }

        public static void WriteGlobalEq(string global)
        {
            sb.Append($"{GetIndent()}{global} = ");
        }

        public static void WriteTableVariable(string var, string val)
        {
            sb.AppendLine($"{GetIndent()}{var} = {val},");
        }

        public static void WriteVariable(string var, string val)
        {
            sb.AppendLine($"{GetIndent()}{var} = {val};");
        }

        public static void WriteFunctionVariable(string var, string val)
        {
            sb.AppendLine($"{GetIndent()}local {var} = {val};");
        }

        public static void WriteBinary(string op, string left, string right)
        {
            sb.Append($"{left} {op} {right}");
        }

        public static void WriteFunction(string[] args, string name)
        {
            sb.AppendLine($"{GetIndent()}{name} = function({string.Join(", ", args)})");
            indent += indentLevel;
            FunctionLevel++;
        }

        public static void WriteIf(string condition)
        {
            sb.AppendLine($"{GetIndent()}if {condition} then");
            indent += indentLevel;
            FunctionLevel++;
        }

        public static void WriteWhile(string condition)
        {
            sb.AppendLine($"{GetIndent()}while {condition} do");
            indent += indentLevel;
            FunctionLevel++;
        }
        public static void WriteFor(string vari, string init, string to)
        {
            sb.AppendLine($"{GetIndent()}for {vari} {init}, {to} do");
            indent += indentLevel;
            FunctionLevel++;
        }

        public static void WriteForPairs(string var, string to)
        {
            sb.AppendLine($"{GetIndent()}for {var + "_index"},{var} in pairs({to}) do");
            indent += indentLevel;
            FunctionLevel++;
        }
        public static void WriteEnd()
        {
            indent -= indentLevel;
            FunctionLevel--;
            if (UseSelf())
                SelfLevel--;
            sb.AppendLine($"{GetIndent()}end");
        }

        public static void WriteAssignment(string var)
        {
            sb.AppendLine($"{GetIndent()}{var}");
        }

        public static void WriteElse()
        {
            sb.AppendLine($"{GetIndent()}else");
        }

        public static void WriteComment(string comment)
        {
            sb.AppendLine($"{GetIndent()}--{comment}");
        }

        public static void CallFunction(string name, string[] args)
        {
            sb.AppendLine($"{GetIndent()}{name}({string.Join(", ", args)});");
        }

        public static void WriteSep()
        {
            if (InsideBlock())
                WriteTableSep();
        }
    }
}