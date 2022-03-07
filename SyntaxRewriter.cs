using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToLua
{
    internal class SyntaxRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var idn = node.Type as IdentifierNameSyntax;
            var name = idn.Identifier.Text;

            var ident = SyntaxFactory.IdentifierName(name + ".new");

            var expr = SyntaxFactory.InvocationExpression(ident, node.ArgumentList);
            expr = expr.WithoutLeadingTrivia();

            return expr;
        }

        public override SyntaxNode? VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            if (node.Value.ToString() == "null")
            {
                return SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName("nil"));
            }

            return base.VisitEqualsValueClause(node);
        }

        public override SyntaxNode? VisitCastExpression(CastExpressionSyntax node)
        {
            var type = node.Type as PredefinedTypeSyntax;

            switch (type.Keyword.Kind())
            {
                case SyntaxKind.IntKeyword:
                {
                    var ident = SyntaxFactory.IdentifierName("tonumber");
                    var args = SyntaxFactory.ArgumentList();
                    args = args.AddArguments(SyntaxFactory.Argument(node.Expression));

                    var expr = SyntaxFactory.InvocationExpression(ident, args);
                    expr = expr.WithoutLeadingTrivia();

                    return expr;
                }
                case SyntaxKind.StringKeyword:
                {
                    var ident = SyntaxFactory.IdentifierName("tostring");
                    var args = SyntaxFactory.ArgumentList();
                    args = args.AddArguments(SyntaxFactory.Argument(node.Expression));

                    var expr = SyntaxFactory.InvocationExpression(ident, args);
                    expr = expr.WithoutLeadingTrivia();

                    return expr;
                }
            }

            return node;
        }

        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);

            switch (node.Left.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                {
                    if (node.OperatorToken.Kind() == SyntaxKind.PlusToken)
                    {
                        var tok = SyntaxFactory.Token(node.OperatorToken.LeadingTrivia, SyntaxKind.PlusToken,
                            "..", "..", node.OperatorToken.TrailingTrivia);
                        var binaryexpr = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, node.Left, tok, node.Right);

                        return binaryexpr;
                    }

                    break;
                }
            }

            switch (node.OperatorToken.Kind())
            {
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.ExclamationEqualsToken:
                {
                    var tok = SyntaxFactory.Token(node.OperatorToken.LeadingTrivia, SyntaxKind.ExclamationEqualsToken,
                        "~=", "~=", node.OperatorToken.TrailingTrivia);
                    var newbinaryexpr = SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, node.Left, tok, node.Right);

                    return newbinaryexpr;
                }
                case SyntaxKind.BarBarToken:
                {
                    var tok = SyntaxFactory.Token(node.OperatorToken.LeadingTrivia, SyntaxKind.BarBarToken,
                        "or", "or", node.OperatorToken.TrailingTrivia);
                    var newbinaryexpr = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, node.Left, tok, node.Right);

                    return newbinaryexpr;
                }
                case SyntaxKind.AmpersandAmpersandToken:
                {
                    var tok = SyntaxFactory.Token(node.OperatorToken.LeadingTrivia, SyntaxKind.AmpersandAmpersandToken,
                        "and", "and", node.OperatorToken.TrailingTrivia);
                    var newbinaryexpr = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, node.Left, tok, node.Right);

                    return newbinaryexpr;
                }
            }

            return node;
        }

        public override SyntaxNode? VisitThisExpression(ThisExpressionSyntax node)
        {
            var newthis = SyntaxFactory.ThisExpression(SyntaxFactory.Token(node.GetLeadingTrivia(),
                SyntaxKind.ThisKeyword, "self", "self", node.GetTrailingTrivia()));
            return newthis;
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            string name = CodeGen.GetMethodName(node) ?? "nil";

            foreach (var libraryInterface in Library.Library.InUse) name = libraryInterface.OnCall(name);

            var expr = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(name), node.ArgumentList);

            if (!node.HasLeadingTrivia)
                expr = expr.WithoutLeadingTrivia();

            return expr;
        }
    }
}