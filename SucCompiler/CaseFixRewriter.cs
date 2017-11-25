using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CocCompiler
{
    class CaseFixRewriter : CSharpSyntaxRewriter
    {
        //protected override CSharpSyntaxVisitor
        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.ValueText == "While")
            {
                SyntaxToken token = node.Identifier;
                SyntaxToken newToken = SyntaxFactory.Identifier(token.LeadingTrivia, "while", token.TrailingTrivia);

                IdentifierNameSyntax newNode = SyntaxFactory.IdentifierName(newToken);

                return node.ReplaceNode(node, newNode);
            }
            else if (node.Identifier.ValueText == "If")
            {
                SyntaxToken token = node.Identifier;
                SyntaxToken newToken = SyntaxFactory.Identifier(token.LeadingTrivia, "if", token.TrailingTrivia);

                IdentifierNameSyntax newNode = SyntaxFactory.IdentifierName(newToken);

                return node.ReplaceNode(node, newNode);
            }
            else if (node.Identifier.ValueText == "Else")
            {
                SyntaxToken token = node.Identifier;
                SyntaxToken newToken = SyntaxFactory.Identifier(token.LeadingTrivia, "else", token.TrailingTrivia);

                IdentifierNameSyntax newNode = SyntaxFactory.IdentifierName(newToken);

                return node.ReplaceNode(node, newNode);
            }

            return base.VisitIdentifierName(node);
        }
    }
}
