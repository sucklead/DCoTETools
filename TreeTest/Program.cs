﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TreeTest
{
    class Program
    {


        static CSharpSyntaxTree tree = null;
        static CompilationUnitSyntax root = null;
        static SyntaxAnnotation currentBlock = null;
        static SyntaxToken semi;

        static void Main(string[] args)
        {
            Console.WriteLine("Building...");

            semi = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n")));

            CSharpParseOptions cSharpParseOptions = new CSharpParseOptions(LanguageVersion.CSharp1, DocumentationMode.Parse, SourceCodeKind.Script);
            tree = CSharpSyntaxTree.ParseText("", options: cSharpParseOptions) as CSharpSyntaxTree;
            root = tree.GetCompilationUnitRoot(); //(CompilationUnitSyntax)tree.GetRoot();

            //create the block and add it
            var syntaxAnnotation = new SyntaxAnnotation("Block", "1");
            BlockSyntax block = SyntaxFactory.Block().WithAdditionalAnnotations(syntaxAnnotation);
            currentBlock = syntaxAnnotation;

            //add block to root
            GlobalStatementSyntax globalStatement = SyntaxFactory.GlobalStatement(block);
            root = root.AddMembers(globalStatement); //root = root.WithMembers(root.Members.Add(globalStatement));

            //add to first block
            ExpressionSyntax expressionSyntax = SyntaxFactory.IdentifierName("Print");
            ArgumentListSyntax argumentList = SyntaxFactory.ParseArgumentList("(" + "arg1, arg2" + ")");
            InvocationExpressionSyntax invocationExpression = SyntaxFactory.InvocationExpression(expressionSyntax, argumentList);
            ExpressionStatementSyntax expressionStatement = SyntaxFactory.ExpressionStatement(invocationExpression, semi);

            AddToCurrentBlock(expressionStatement);

            //add a block to the block
            syntaxAnnotation = new SyntaxAnnotation("Block", "2");
            BlockSyntax newBlock = SyntaxFactory.Block().WithAdditionalAnnotations(syntaxAnnotation);
            AddToCurrentBlock(newBlock);
            currentBlock = syntaxAnnotation;

            AddVariables();

            //move back a block
            GetPreviousBlock();



            //show source
            block = root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(currentBlock)).Single();
            Console.WriteLine("block source:");
            Console.WriteLine(block.GetText());
            Console.WriteLine("root source:");
            Console.WriteLine(root.GetText());
            Console.WriteLine("Complete.");
            Console.ReadLine();
        }

        private static void GetPreviousBlock()
        {
            BlockSyntax block = root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(currentBlock)).Single();
            block = block.Parent as BlockSyntax;
            currentBlock = block.GetAnnotations("Block").Single();
        }

        private static void AddVariables()
        {
            TypeSyntax intType = SyntaxFactory.ParseTypeName("int ");

            // Build the field variable 1
            VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(
              identifier: SyntaxFactory.Identifier("Var1")
            );

            VariableDeclarationSyntax variableDeclaration = SyntaxFactory.VariableDeclaration(type: intType, variables: SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>().Add(declarator));

            SyntaxTokenList modifiers = new SyntaxTokenList();

            LocalDeclarationStatementSyntax localDeclaration = SyntaxFactory.LocalDeclarationStatement(modifiers, variableDeclaration, semi);
            AddToCurrentBlock(localDeclaration);

        }

        private static void AddToCurrentBlock(StatementSyntax newStatement)
        {
            BlockSyntax oldBlock = root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(currentBlock)).Single();
            BlockSyntax newBlock = oldBlock.AddStatements(newStatement);
            root = root.ReplaceNode(oldBlock, newBlock);
        }
    }
}
