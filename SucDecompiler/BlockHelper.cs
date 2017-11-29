using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SucDecompiler
{
    class BlockHelper
    {
        public static CompilationUnitSyntax Root { get; set; }
        public static SyntaxAnnotation CurrentBlock { get; set; }

        public static BlockSyntax CreateNewBlock(string blockName)
        {
            SyntaxAnnotation syntaxAnnotation = new SyntaxAnnotation("Block", blockName);
            BlockSyntax block = SyntaxFactory.Block().WithAdditionalAnnotations(syntaxAnnotation);

            return block;
        }

        public static void SetBlockAsCurrent(BlockSyntax block)
        {
            CurrentBlock = block.GetAnnotations("Block").Single();
        }

        public static void AddToCurrentBlock(StatementSyntax newStatement)
        {
            BlockSyntax oldBlock = Root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(CurrentBlock)).Single();
            BlockSyntax newBlock = oldBlock.AddStatements(newStatement);
            Root = Root.ReplaceNode(oldBlock, newBlock);
        }

        public static BlockSyntax GetPreviousBlock()
        {
            BlockSyntax block = Root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(CurrentBlock)).Single();
            if (block.Parent is IfStatementSyntax)
            {
                IfStatementSyntax ifStatement = block.Parent as IfStatementSyntax;
                block = ifStatement.Parent as BlockSyntax;
            }
            else
            {
                block = block.Parent as BlockSyntax;
            }
            CurrentBlock = block.GetAnnotations("Block").Single();

            return block;
        }

    }
}
