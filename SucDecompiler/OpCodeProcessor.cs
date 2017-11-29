using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using banallib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace SucDecompiler
{
    public class OpCodeProcessor
    {
        public List<Operation> OpCodes { get; set; }
        public VariableSet VariableSet { get; set; }

        private int codePointer = 0;
        private int totalOpCodes = 0;
        bool firstPrint = true;
        Stack<DataIndex> stack = null;
        SyntaxToken semi = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n")));

        public OpCodeProcessor()
        {
            stack = new Stack<DataIndex>();
        }

        internal void ProcessOpCodes()
        {
            totalOpCodes = OpCodes.Count;
            codePointer = 0;
            //parse the opcodes
            while (codePointer < totalOpCodes)
            {
                processOperation(OpCodes[codePointer]);
                codePointer++;
            }
        }

        private void processOperation(Operation operation)
        {
            if (operation.OpCode == OpCodeType.OP_JMP)
            {
                ProcessJump();
            }
            else if (operation.OpCode == OpCodeType.OP_JMPF)
            {
                ProcessJumpF();
            }
            else if (operation.OpCode == OpCodeType.JUMPTARGET)
            {
                ProcessJumpTarget();
            }
            else if (operation.OpCode == OpCodeType.OP_PRINT)
            {
                ProcessPrint();
            }
            else if (operation.OpCode == OpCodeType.OP_FUNCTION)
            {
                ProcessFunction();
            }
            else if (operation.OpCode == OpCodeType.OP_PUSH)
            {
                stack.Push(operation.DataIndex);
            }
            else if (operation.OpCode == OpCodeType.OP_GETTOP)
            {
                // function return value handled in function
                // so this is only for assigns
                ProcessAssign();
            }
            else if (operation.OpCode == OpCodeType.OP_DISCARD)
            {
                stack.Clear();
            }
        }

        private void AddVariablesToBlock()
        {
            TypeSyntax intType = SyntaxFactory.ParseTypeName("int ");
            TypeSyntax stringType = SyntaxFactory.ParseTypeName("String ");
            TypeSyntax floatType = SyntaxFactory.ParseTypeName("double ");
            TypeSyntax pointType = SyntaxFactory.ParseTypeName("Point ");
            TypeSyntax characterType = SyntaxFactory.ParseTypeName("Character ");
            TypeSyntax quartonianType = SyntaxFactory.ParseTypeName("Quartonian ");

            var q = from variable in VariableSet.Variables
                    where variable.Value.Static == false
                    orderby variable.Key
                    select variable;
            foreach (var variable in q)
            {
                //sourceBuilder.Insert(0, string.Format("{0} variable{1};\n", variable.Value, variable.Key));

                // Build the field variable 1
                VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(
                  identifier: SyntaxFactory.Identifier(variable.Value.Name)
                );

                TypeSyntax theType = null;
                switch (variable.Value.DataType)
                {
                    case ("Int"):
                        theType = intType;
                        break;
                    case ("String"):
                        theType = stringType;
                        break;
                    case ("Point"):
                        theType = pointType;
                        break;
                    case ("Character"):
                        theType = characterType;
                        break;
                    case ("Float"):
                        theType = floatType;
                        break;
                    case ("Quartonian"):
                        theType = quartonianType;
                        break;
                    case ("Quaternion"):
                        theType = quartonianType;
                        break;
                }

                VariableDeclarationSyntax variableDeclaration = SyntaxFactory.VariableDeclaration(type: theType, variables: SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>().Add(declarator));

                SyntaxTokenList modifiers = new SyntaxTokenList();

                LocalDeclarationStatementSyntax localDeclaration = SyntaxFactory.LocalDeclarationStatement(modifiers, variableDeclaration, semi);
                BlockHelper.AddToCurrentBlock(localDeclaration);
            }
        }

        private void ProcessAssign()
        {
            DataIndex assignValue = stack.Pop();

            var variableIdentifier = SyntaxFactory.IdentifierName(VariableSet.Variables[OpCodes[codePointer].DataIndex.Value].Name);
            var variableExpression = GetVariableExpression(assignValue.Value);

            AssignmentExpressionSyntax assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, variableExpression);

            var expressionStatement = SyntaxFactory.ExpressionStatement(assignment, semi);

            //add to the current block
            BlockHelper.AddToCurrentBlock(expressionStatement);
        }

        private void ProcessFunction()
        {
            DataIndex function = stack.Pop();
            string functionName = FunctionTable.Functions[function.Value];
            ExpressionSyntax expressionSyntax = SyntaxFactory.IdentifierName(functionName);
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList();
            List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();

            stack = new Stack<DataIndex>(stack);
            while (stack.Count > 0)
            {
                DataIndex param = stack.Pop();

                arguments.Add(SyntaxFactory.Argument(GetVariableExpression(param.Value)));
            }
            argumentList = argumentList.AddArguments(arguments.ToArray());

            InvocationExpressionSyntax invocationExpression = SyntaxFactory.InvocationExpression(expressionSyntax, argumentList);

            ExpressionStatementSyntax expressionStatement = SyntaxFactory.ExpressionStatement(invocationExpression, semi);

            //are we assigning to a variable?
            if (OpCodes[codePointer + 1].OpCode == OpCodeType.OP_GETTOP)
            {
                var variableIdentifier = SyntaxFactory.IdentifierName(VariableSet.Variables[OpCodes[codePointer + 1].DataIndex.Value].Name);
                var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, expressionStatement.Expression);
                expressionStatement = SyntaxFactory.ExpressionStatement(assignment, semi);

                //make sure we move past the OP_GETTOP
                codePointer++;
            }

            //add to the current block
            BlockHelper.AddToCurrentBlock(expressionStatement);
        }

        private void ProcessPrint()
        {
            DataIndex printParam = null;
            string argument;
            if (stack.Count > 0)
            {
                printParam = stack.Pop();

                if (!VariableSet.Variables[printParam.Value].Static)
                {
                    argument = VariableSet.Variables[printParam.Value].Name;
                }
                else
                {
                    argument = VariableSet.GetCurrentValue(printParam.Value).ToString();
                }
            }
            else
            {
                argument = "";
            }

            ExpressionSyntax expressionSyntax = SyntaxFactory.IdentifierName("Print");
            ArgumentListSyntax argumentList = SyntaxFactory.ParseArgumentList("(" + argument + ")");

            InvocationExpressionSyntax invocationExpression = SyntaxFactory.InvocationExpression(expressionSyntax, argumentList);

            ExpressionStatementSyntax expressionStatement = SyntaxFactory.ExpressionStatement(invocationExpression, semi);

            var statements = new SyntaxList<StatementSyntax>().Add(expressionStatement);
            //add to the current block
            BlockHelper.AddToCurrentBlock(expressionStatement);
            //currentBlock = currentBlock.WithStatements(statements);

            if (firstPrint)
            {
                firstPrint = false;
                AddVariablesToBlock(); // = currentBlock.AddStatements(expressionStatement);
            }

            //GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);
        }

        private void ProcessJump()
        {
            //go back to parent of if block
            BlockHelper.GetPreviousBlock();

            // start a new block for the else
            BlockSyntax elseBlock = BlockHelper.CreateNewBlock("else");

            BlockHelper.AddToCurrentBlock(elseBlock);

            //current block is now the else
            BlockHelper.SetBlockAsCurrent(elseBlock);

            //////// this is an else jump so create an else block
            //////BlockSyntax elseBlock = SyntaxFactory.Block();

            ////////get the current if block
            //////BlockSyntax ifBlock = blockStack.Pop();

            ////////build the else clause with block
            //////ElseClauseSyntax elseClause = SyntaxFactory.ElseClause(elseBlock);

            ////////add the else to the if
            //////IfStatementSyntax ifStatement = currentIf;
            //////ifStatement = ifStatement.WithElse(elseClause);
            //////blockStack.Push(elseBlock);
        }

        private void ProcessJumpF()
        {

            //find variable that was pushed
            DataIndex conditionVariable = stack.Pop();

            // start a new block for the if
            BlockSyntax ifBlock = BlockHelper.CreateNewBlock("if");

            //add the if to the current block
            ExpressionSyntax expressionSyntax = GetVariableExpression(conditionVariable.Value);
            IfStatementSyntax ifStatement = SyntaxFactory.IfStatement(expressionSyntax, ifBlock);
            BlockHelper.AddToCurrentBlock(ifStatement);

            //current block is now the if
            BlockHelper.SetBlockAsCurrent(ifBlock);

            //SyntaxToken operatorToken = null;
            //if (falseCheck)
            //{
            //    operatorToken = SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken);
            //}
            //else
            //{
            //    operatorToken = SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken);
            //}

            //BinaryExpressionSyntax binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.)

            //////currentIf = ifStatement;

            //////BlockSyntax currentBlock = blockStack.Pop();
            //////currentBlock = currentBlock.AddStatements(ifStatement);
            //////blockStack.Push(currentBlock);

            ////////if block is new current block
            //////blockStack.Push(ifBlock);
        }

        private void ProcessJumpTarget()
        {
            //if previous opcode was an else then don't step back a block
            if (OpCodes[codePointer - 1].OpCode != OpCodeType.OP_JMP)
            {
                BlockHelper.GetPreviousBlock();
            }
        }

        private ExpressionSyntax GetVariableExpression(short variable)
        {
            ExpressionSyntax expressionSyntax = null;
            //if not static condition is the variable itself
            if (!VariableSet.Variables[variable].Static)
            {
                expressionSyntax = SyntaxFactory.IdentifierName(VariableSet.Variables[variable].Name);
            }
            else
            {
                var dataType = VariableSet.Variables[variable].DataType;

                if (dataType == "String")
                {
                    string value = (string)VariableSet.GetCurrentValue(variable).ToString().Replace("\"", "");
                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
                }
                else if (dataType == "Float")
                {
                    float value = float.Parse(VariableSet.GetCurrentValue(variable).ToString().Replace("\"", "").Replace(@"\", ""));
                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
                }
                else if (dataType == "Int")
                {
                    int value = int.Parse(VariableSet.GetCurrentValue(variable).ToString().Replace("\"", "").Replace(@"\", ""));
                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
                }
                else
                {
                    throw (new NotImplementedException());
                }
            }

            return expressionSyntax;
        }

    }
}
