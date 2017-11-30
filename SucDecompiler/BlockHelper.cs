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
        private static int lastBlockNumber = 0;

        public static BlockSyntax CreateNewBlock(string blockName)
        {
            SyntaxAnnotation syntaxAnnotation = new SyntaxAnnotation("Block", lastBlockNumber.ToString());
            lastBlockNumber++;
            BlockSyntax block = SyntaxFactory.Block().WithAdditionalAnnotations(syntaxAnnotation);

            return block;
        }

        public static void SetBlockAsCurrent(BlockSyntax block)
        {
            CurrentBlock = block.GetAnnotations("Block").Single();
        }

        public static void AddElseToIfBlock()
        {
            //get current block
            BlockSyntax block = Root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(CurrentBlock)).Single();
            if (!(block.Parent is IfStatementSyntax))
            {
                return;
            }
            SyntaxAnnotation syntaxAnnotation = new SyntaxAnnotation("Block", "else");
            BlockSyntax elseBlock = SyntaxFactory.Block().WithAdditionalAnnotations(syntaxAnnotation);
            ElseClauseSyntax elseClauseSyntax = SyntaxFactory.ElseClause(elseBlock);

            //get if statement
            IfStatementSyntax oldIfStatement = block.Parent as IfStatementSyntax;
            //add an empty else block
            IfStatementSyntax newIfStatement = oldIfStatement.WithElse(elseClauseSyntax);

            //do the replacement
            Root = Root.ReplaceNode(oldIfStatement, newIfStatement);

            //the else block is now current
            SetBlockAsCurrent(elseBlock);
        }

        public static BlockSyntax GetCurrentBlock()
        {
            return Root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(CurrentBlock)).Single();
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
            BlockSyntax prevBlock = null;

            if (block.Parent is ElseClauseSyntax elseStatement)
            {
                //ElseClauseSyntax elseStatement = block.Parent as ElseClauseSyntax;
                IfStatementSyntax ifStatement = elseStatement.Parent as IfStatementSyntax;
                prevBlock = ifStatement.Parent as BlockSyntax;
            }
            else if (block.Parent is IfStatementSyntax ifStatement)
            {
                //IfStatementSyntax ifStatement = block.Parent as IfStatementSyntax;
                prevBlock = ifStatement.Parent as BlockSyntax;
            }
            else if (block.Parent is WhileStatementSyntax whileStatement)
            {
                //IfStatementSyntax ifStatement = block.Parent as IfStatementSyntax;
                prevBlock = whileStatement.Parent as BlockSyntax;
            }
            else
            {
                prevBlock = block.Parent as BlockSyntax;
            }

            if (prevBlock == null)
            {
                //CurrentBlock = null;
                return block;
            }

            CurrentBlock = prevBlock.GetAnnotations("Block").Single();

            return prevBlock;
        }

        internal static BlockSyntax GetFirstBlock()
        {
            return Root.DescendantNodes().OfType<BlockSyntax>().First();
        }

        internal static void ConvertIfToWhile()
        {
            //get current block
            BlockSyntax block = Root.DescendantNodes().OfType<BlockSyntax>().Where(n => n.HasAnnotation(CurrentBlock)).Single();
            if (!(block.Parent is IfStatementSyntax))
            {
                return;
            }
            //SyntaxAnnotation syntaxAnnotation = new SyntaxAnnotation("Block", "else");
            //BlockSyntax elseBlock = SyntaxFactory.Block().WithAdditionalAnnotations(syntaxAnnotation);
            //ElseClauseSyntax elseClauseSyntax = SyntaxFactory.ElseClause(elseBlock);

            //get if statement
            IfStatementSyntax ifStatement = block.Parent as IfStatementSyntax;
            //change it to a while
            WhileStatementSyntax whileStatement = SyntaxFactory.WhileStatement(ifStatement.Condition, ifStatement.Statement);

            //do the replacement
            Root = Root.ReplaceNode(ifStatement, whileStatement);

            BlockSyntax prevBlock = GetPreviousBlock();
            CurrentBlock = prevBlock.GetAnnotations("Block").Single();
        }
    }
}
