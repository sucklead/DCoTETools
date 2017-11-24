using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace CocCompiler
{
    class CaseFixRewriter : SyntaxRewriter
    {
        protected override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.ValueText == "While")
            {
                SyntaxToken token = node.Identifier;
                SyntaxToken newToken = Syntax.Identifier(token.LeadingTrivia, "while", token.TrailingTrivia);

                IdentifierNameSyntax newNode = Syntax.IdentifierName(newToken);

                return node.ReplaceNode(node, newNode);
            }
            else if (node.Identifier.ValueText == "If")
            {
                SyntaxToken token = node.Identifier;
                SyntaxToken newToken = Syntax.Identifier(token.LeadingTrivia, "if", token.TrailingTrivia);

                IdentifierNameSyntax newNode = Syntax.IdentifierName(newToken);

                return node.ReplaceNode(node, newNode);
            }
            else if (node.Identifier.ValueText == "Else")
            {
                SyntaxToken token = node.Identifier;
                SyntaxToken newToken = Syntax.Identifier(token.LeadingTrivia, "else", token.TrailingTrivia);

                IdentifierNameSyntax newNode = Syntax.IdentifierName(newToken);

                return node.ReplaceNode(node, newNode);
            }

            return base.VisitIdentifierName(node);
        }
    }
}
