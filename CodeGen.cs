using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToLua
{
    internal class CodeGen : CSharpSyntaxWalker
    {
        // ReSharper disable once InconsistentNaming
        private readonly Dictionary<string, string> methods = new();
        // ReSharper disable once InconsistentNaming
        private readonly Dictionary<string, (string, bool)> variables = new();
        public string EntryPoint { get; set; }

        public static string GetMethodName(InvocationExpressionSyntax node)
        {
            var name = "nil";
            switch (node.Expression.GetType().Name)
            {
                case "IdentifierNameSyntax":
                {
                    var expr = node.Expression as IdentifierNameSyntax;

                    name = expr?.Identifier.Text;

                    break;
                }
                case "MemberAccessExpressionSyntax":
                {
                    var expr = node.Expression as MemberAccessExpressionSyntax;
                    name = expr?.ToString();

                    break;
                }
                default:
                {
                    LuaWriter.WriteComment($"This type isn't supported. '{node.Expression.GetType().Name}'");
                    break;
                }
            }

            return name;
        }
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            string name = GetMethodName(node) ?? "nil";
            List<string> args = new();
            foreach (var a in node.ArgumentList.Arguments) args.Add(a.ToString());

            var subName = name?.Split('.').Last().Split('(').First() ?? string.Empty;
            if (methods.ContainsKey(subName)) name = methods[subName] + "." + subName;

            if (node.HasLeadingTrivia)
                LuaWriter.CallFunction(name, args.ToArray());

            base.VisitInvocationExpression(node);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var name = node.ToString();

            var subName = name.Split('.').First();
            if (variables.ContainsKey(subName))
            {
                var val = variables[subName];

                if(val.Item2 == false)
                    name = val.Item1 + "." + name;
            }

            LuaWriter.WriteAssignment(name);
            base.VisitAssignmentExpression(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            List<string> args = new();
            foreach (var p in node.ParameterList.Parameters) args.Add(p.Identifier.Text);

            LuaWriter.WriteFunction(args.ToArray(), node.Identifier.Text);
            base.VisitMethodDeclaration(node);
            LuaWriter.WriteEnd();

            LuaWriter.WriteSep();

            if (node.Identifier.Text == "Main" || node.Identifier.Text == "EntryPoint")
            {
                EntryPoint = string.Join('.', LuaWriter.GetAncestorPath(node).ToArray()) + "." + node.Identifier.Text;
            }

            methods.Add(node.Identifier.Text, string.Join('.', LuaWriter.GetAncestorPath(node).ToArray()));
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            LuaWriter.WriteWhile(node.Condition.ToString());
            base.VisitWhileStatement(node);
            LuaWriter.WriteEnd();
        }
        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            var inside = node.Expression.ToString();
            string varname = node.Identifier.Text;

            LuaWriter.WriteForPairs(varname, inside);
            base.VisitForEachStatement(node);
            LuaWriter.WriteEnd();
        }
        public override void VisitForStatement(ForStatementSyntax node)
        {
            var i = node.Declaration.Variables[0];
            var name = i.Identifier.Text;
            var start = i.Initializer.ToString();

            var binaryexpr = node.Condition as BinaryExpressionSyntax;

            LuaWriter.WriteFor(name, start, binaryexpr.Right.ToString());
            base.VisitForStatement(node);
            LuaWriter.WriteEnd();
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var para = new List<string>();
            para.Add("self");
            foreach (var parameterListParameter in node.ParameterList.Parameters)
            {
                para.Add(parameterListParameter.Identifier.Text);
            }

            LuaWriter.WriteFunction(para.ToArray(), "constructor");
            base.VisitConstructorDeclaration(node);
            LuaWriter.WriteEnd();
            LuaWriter.WriteSep();
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            foreach (var v in node.Variables)
            {
                if (node.HasLeadingTrivia == false)
                    break;
                if (LuaWriter.InsideFunction())
                {
                    LuaWriter.WriteFunctionVariable(v.Identifier.Text, v.Initializer?.Value.ToString());
                }
                else
                {
                    LuaWriter.WriteTableVariable(v.Identifier.Text, v.Initializer?.Value.ToString());
                }

                variables.Add(v.Identifier.Text, (string.Join('.', LuaWriter.GetAncestorPath(node).ToArray()), LuaWriter.InsideFunction()));
            }

            base.VisitVariableDeclaration(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            LuaWriter.WriteIf(node.Condition.ToString());
            node.Statement.Accept(this);
            if (node.Else != null)
            {
                LuaWriter.WriteElse();
                node.Else.Accept(this);
            }

            LuaWriter.WriteEnd();
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            LuaWriter.WriteGlobalTable(node.Identifier.Text);
            base.VisitClassDeclaration(node);

            LuaWriter.WriteClassNew(node.Identifier.Text);
            LuaWriter.WriteSep();
            LuaWriter.WriteR();

            LuaWriter.WriteSep();
        }
    }
}