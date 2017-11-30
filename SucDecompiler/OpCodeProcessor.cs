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
        Stack<ExpressionSyntax> expressionStack = null;

        SyntaxToken semi = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n")));

        public OpCodeProcessor()
        {
            stack = new Stack<DataIndex>();
            expressionStack = new Stack<ExpressionSyntax>();
            firstPrint = true;
        }

        internal void ProcessOpCodes()
        {
            totalOpCodes = OpCodes.Count;
            codePointer = 0;

            if (totalOpCodes > 2
                && OpCodes[1].OpCode != OpCodeType.OP_PRINT)
            {
                AddVariablesToBlock();
                firstPrint = false;
            }

            //parse the opcodes
            while (codePointer < totalOpCodes)
            {
                ProcessOperation(OpCodes[codePointer]);
                codePointer++;
            }
        }

        private void ProcessOperation(Operation operation)
        {
            if (operation.OpCode == OpCodeType.OP_NOT_EQUAL
                || operation.OpCode == OpCodeType.OP_EQUAL
                || operation.OpCode == OpCodeType.OP_MORE_THAN
                || operation.OpCode == OpCodeType.OP_MORE_THAN_OR_EQUAL
                || operation.OpCode == OpCodeType.OP_LESS_THAN
                || operation.OpCode == OpCodeType.OP_LESS_THAN_OR_EQUAL
                )
            {
                ProcessComparison(operation.OpCode);
            }
            else if (operation.OpCode == OpCodeType.OP_JMP)
            {
                ProcessJump(operation);
            }
            else if (operation.OpCode == OpCodeType.OP_JMPF)
            {
                ProcessJumpF();
            }
            else if (operation.OpCode == OpCodeType.JUMPTARGET)
            {
                ProcessJumpTarget();
            }
            else if (operation.OpCode == OpCodeType.OP_NEG)
            {
                ProcessNeg();
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
            else if (operation.OpCode == OpCodeType.OP_CONCAT)
            {
                ProcessConcat();
            }
            else if (operation.OpCode == OpCodeType.OP_DISCARD)
            {
                stack.Clear();
            }
        }

        private ExpressionSyntax PopVariable()
        {
            ExpressionSyntax variableExpression = null;
            //do we have an expression on the stack?
            if (expressionStack.Count > 0)
            {
                variableExpression = expressionStack.Pop();
            }
            else if (stack.Count > 0)
            {
                //find variable that was pushed
                DataIndex conditionVariable = stack.Pop();
                variableExpression = GetVariableExpression(conditionVariable);
            }

            return variableExpression;
        }

        private void ProcessConcat()
        {
            ExpressionSyntax lhsVariableExpression = PopVariable();
            ExpressionSyntax rhsVariableExpression = PopVariable();
            //get rhs
            //ataIndex rhsValue = stack.Pop();
            //var rhsVariableExpression = GetVariableExpression(rhsValue);
            //get lhs
            //DataIndex lhsValue = stack.Pop();
            //var lhsVariableExpression = GetVariableExpression(lhsValue);

            //SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, variableIdentifier, variableExpression);
            ExpressionSyntax assignment;
            if (lhsVariableExpression is IdentifierNameSyntax)
            {
                assignment = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, rhsVariableExpression, lhsVariableExpression);
            }
            else
            {
                assignment = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, lhsVariableExpression, rhsVariableExpression);
            }
            //var expressionStatement = SyntaxFactory.ExpressionStatement(assignment, semi);

            //BlockHelper.AddToCurrentBlock(expressionStatement);

            expressionStack.Push(assignment);
        }

        private void ProcessComparison(OpCodeType opCode)
        {
            SyntaxKind syntaxKind;
            if (opCode == OpCodeType.OP_NOT_EQUAL)
            {
                syntaxKind = SyntaxKind.NotEqualsExpression;
            }
            else if (opCode == OpCodeType.OP_MORE_THAN)
            {
                syntaxKind = SyntaxKind.GreaterThanExpression;
            }
            else if (opCode == OpCodeType.OP_MORE_THAN_OR_EQUAL)
            {
                syntaxKind = SyntaxKind.GreaterThanOrEqualExpression;
            }
            else if (opCode == OpCodeType.OP_LESS_THAN)
            {
                syntaxKind = SyntaxKind.LessThanExpression;
            }
            else if (opCode == OpCodeType.OP_LESS_THAN_OR_EQUAL)
            {
                syntaxKind = SyntaxKind.LessThanOrEqualExpression;
            }
            else
            {
                syntaxKind = SyntaxKind.EqualsExpression;
            }

            if (stack.Count == 0)
            {
                return;
            }
            //get rhs
            DataIndex rhsValue = stack.Pop();
            var rhsVariableExpression = GetVariableExpression(rhsValue);
            if (stack.Count == 0)
            {
                return;
            }
            //get lhs
            DataIndex lhsValue = stack.Pop();
            var lhsVariableExpression = GetVariableExpression(lhsValue);

            var binaryExpression = SyntaxFactory.BinaryExpression(syntaxKind, lhsVariableExpression, rhsVariableExpression);

            //push expression onto the stack
            expressionStack.Push(binaryExpression);
        }

        private void ProcessNeg()
        {
            DataIndex stackTop = stack.Pop();
            stackTop.IsNegative = !stackTop.IsNegative;
            stack.Push(stackTop);
        }
        
        private void AddVariablesToBlock()
        {
            TypeSyntax intType = SyntaxFactory.ParseTypeName("int ");
            TypeSyntax stringType = SyntaxFactory.ParseTypeName("String ");
            TypeSyntax floatType = SyntaxFactory.ParseTypeName("Float ");
            TypeSyntax pointType = SyntaxFactory.ParseTypeName("Point ");
            TypeSyntax characterType = SyntaxFactory.ParseTypeName("Character ");
            TypeSyntax quaternionType = SyntaxFactory.ParseTypeName("Quaternion ");

            var q = from variable in VariableSet.Variables
                    where (variable.Value.Static == false
                           || variable.Value.Used == false)
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
                switch (variable.Value.DataType.ToLower())
                {
                    case ("int"):
                        theType = intType;
                        break;
                    case ("string"):
                        theType = stringType;
                        break;
                    case ("point"):
                        theType = pointType;
                        break;
                    case ("character"):
                        theType = characterType;
                        break;
                    case ("float"):
                        theType = floatType;
                        break;
                    case ("quaternion"):
                        theType = quaternionType;
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
            ExpressionSyntax variableExpression = null;
            //do we have an expression on the stack?
            if (expressionStack.Count > 0)
            {
                variableExpression = expressionStack.Pop();
            }
            else if (stack.Count > 0)
            {
                //find variable that was pushed
                DataIndex conditionVariable = stack.Peek();
                variableExpression = GetVariableExpression(conditionVariable);
            }
            //DataIndex assignValue = stack.Peek();

            var variableIdentifier = SyntaxFactory.IdentifierName(VariableSet.Variables[OpCodes[codePointer].DataIndex.Value].Name);
            //var variableExpression = GetVariableExpression(assignValue);

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

                arguments.Add(SyntaxFactory.Argument(GetVariableExpression(param)));
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
            if (OpCodes[codePointer + 1].OpCode == OpCodeType.OP_GETTOP)
            {
                //TODO - this needs to work better for a = b = fn(c);

                //add to the current block
                BlockHelper.AddToCurrentBlock(expressionStatement);
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

        private void ProcessJump(Operation jumpOp)
        {
            // TODO - could be a WHILE
            //current block is the if so we need to modify the if with a new block then set it as current
            if (jumpOp.DataIndex.Value < jumpOp.Address)
            {
                BlockHelper.ConvertIfToWhile();
            }
            else
            {
                BlockHelper.AddElseToIfBlock();
            }
            // start a new block for the else
            //BlockSyntax elseBlock = BlockHelper.CreateNewBlock("else");

                //BlockHelper.AddToCurrentBlock(elseBlock);

                //current block is now the else
                //BlockHelper.SetBlockAsCurrent(elseBlock);

            }

        private void ProcessJumpF()
        {
            ExpressionSyntax expressionSyntax = null;
            //do we have an expression on the stack?
            if (expressionStack.Count > 0)
            {
                expressionSyntax = expressionStack.Pop();
            }
            else if (stack.Count > 0)
            {
                //find variable that was pushed
                DataIndex conditionVariable = stack.Pop();
                expressionSyntax = GetVariableExpression(conditionVariable);
            }
            else
            {
                return; // TODO - fix these
            }
            // start a new block for the if
            BlockSyntax ifBlock = BlockHelper.CreateNewBlock("if");

            //add the if to the current block
            IfStatementSyntax ifStatement = SyntaxFactory.IfStatement(expressionSyntax, ifBlock);
            BlockHelper.AddToCurrentBlock(ifStatement);

            //current block is now the if
            BlockHelper.SetBlockAsCurrent(ifBlock);
        }

        private void ProcessJumpTarget()
        {
            //if previous opcode was an else then don't step back a block
            if (OpCodes[codePointer - 1].OpCode != OpCodeType.OP_JMP)
            {
                BlockHelper.GetPreviousBlock();
            }
        }

        private ExpressionSyntax GetVariableExpression(DataIndex dataIndex)
        {
            short variable = dataIndex.Value;

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
                    if (dataIndex.IsNegative)
                    {
                        value = value * -1;
                    }
                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
                }
                else if (dataType == "Int")
                {
                    int value = int.Parse(VariableSet.GetCurrentValue(variable).ToString().Replace("\"", "").Replace(@"\", ""));
                    if (dataIndex.IsNegative)
                    {
                        value = value * -1;
                    }
                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
                }
                else if (dataType == "Character")
                {
                    string value = (string)VariableSet.GetCurrentValue(variable).ToString().Replace("\"", "");
                    if (value == "Me"
                        || value == "Player")
                    {
                        expressionSyntax = SyntaxFactory.IdentifierName(value);
                    }
                    else
                    {
                        expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
                    }
                }
                else if (dataType == "Point")
                {
                    string value = (string)VariableSet.GetCurrentValue(variable).ToString().Replace("\"", "");
                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
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
