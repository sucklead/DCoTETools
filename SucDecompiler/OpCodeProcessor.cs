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
        //bool firstPrint = true;
        //Stack<DataIndex> stack = null;
        //Stack<ExpressionSyntax> expressionStack = null;

        Stack<object> stack = null;

        SyntaxToken semi = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n")));

        public OpCodeProcessor()
        {
            stack = new Stack<object>();
            //expressionStack = new Stack<ExpressionSyntax>();
            //firstPrint = true;
        }

        internal void ProcessOpCodes()
        {
            totalOpCodes = OpCodes.Count;
            codePointer = 0;

            // check for file with no print as first statement
            //if (totalOpCodes > 2
            //    && OpCodes[0].OpCode != OpCodeType.OP_PUSH
            //    && OpCodes[1].OpCode != OpCodeType.OP_PRINT)
            //{
            //}

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
            else if (operation.OpCode == OpCodeType.OP_NOT)
            {
                ProcessNot();
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
                ProcessPush(operation);
            }
            else if (operation.OpCode == OpCodeType.OP_GETTOP)
            {
                // function return value handled in function
                // so this is only for assigns
                ProcessAssign();
            }
            else if (operation.OpCode == OpCodeType.OP_CONCAT)
            {
                ProcessBinary(operation.OpCode);
            }
            else if (operation.OpCode == OpCodeType.OP_MINUS)
            {
                ProcessBinary(operation.OpCode);
            }
            else if (operation.OpCode == OpCodeType.OP_DIVIDE)
            {
                ProcessBinary(operation.OpCode);
            }
            else if (operation.OpCode == OpCodeType.OP_MULTIPLY)
            {
                ProcessBinary(operation.OpCode);
            }
            else if (operation.OpCode == OpCodeType.OP_AND)
            {
                ProcessBinary(operation.OpCode);
            }
            else if (operation.OpCode == OpCodeType.OP_OR)
            {
                ProcessBinary(operation.OpCode);
            }
            else if (operation.OpCode == OpCodeType.OP_DISCARD)
            {
                stack.Clear();
            }
        }

        private void ProcessPush(Operation operation)
        {
            //annotate current node
            //"StaticVar", variable.Key.ToString()
            //short variableValue = operation.DataIndex.Value;
            //Variable variable = VariableSet.Variables[variableValue];

            ////if not static condition is the variable itself
            //if (variable.Static)
            //{
            //    if (variable.FirstToken == null)
            //    {
            //        variable.FirstToken = 
            //    }
            //}
            //add the value to the stack
            stack.Push(operation.DataIndex);
        }

        private ExpressionSyntax PopVariable()
        {
            if (stack.Count == 0)
            {
                return null;
            }        

            object var = stack.Pop();

            //do we have an expression on the stack?
            if (var is ExpressionSyntax varExpression)
            {
                return varExpression;
            }
            else
            {
                //find variable that was pushed
                return GetVariableExpression(var as DataIndex);
            }
        }

        private void ProcessBinary(OpCodeType opCode)
        {
            ExpressionSyntax rhsVariableExpression = PopVariable();
            ExpressionSyntax lhsVariableExpression = PopVariable();

            SyntaxKind syntaxKind;
            if (opCode == OpCodeType.OP_CONCAT)
            {
                syntaxKind = SyntaxKind.AddExpression;
            }
            else if (opCode == OpCodeType.OP_MINUS)
            {
                syntaxKind = SyntaxKind.SubtractExpression;
            }
            else if (opCode == OpCodeType.OP_DIVIDE)
            {
                syntaxKind = SyntaxKind.DivideExpression;
            }
            else if (opCode == OpCodeType.OP_MULTIPLY)
            {
                syntaxKind = SyntaxKind.MultiplyExpression;
            }
            else if (opCode == OpCodeType.OP_AND)
            {
                syntaxKind = SyntaxKind.LogicalAndExpression;
            }
            else if (opCode == OpCodeType.OP_OR)
            {
                syntaxKind = SyntaxKind.LogicalOrExpression;
            }
            else
            {
                throw new Exception();
            }

            ExpressionSyntax assignment;
            //if (lhsVariableExpression is IdentifierNameSyntax)
           // {
                assignment = SyntaxFactory.BinaryExpression(syntaxKind, lhsVariableExpression, rhsVariableExpression);
            //}
            //else
            //{
            //    assignment = SyntaxFactory.BinaryExpression(syntaxKind, rhsVariableExpression, lhsVariableExpression);
            //}

            stack.Push(assignment);
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

            var rhsVariableExpression = PopVariable();
            var lhsVariableExpression = PopVariable();

            var binaryExpression = SyntaxFactory.BinaryExpression(syntaxKind, lhsVariableExpression, rhsVariableExpression);

            //check for an OP_EQUAL followed by OP_DISCARD where == is used instead of =
            if (syntaxKind == SyntaxKind.EqualsExpression
                && OpCodes[codePointer + 1].OpCode == OpCodeType.OP_DISCARD)
            {
                var expressionStatement = SyntaxFactory.ExpressionStatement(binaryExpression, semi);
                //add to the current block
                BlockHelper.AddToCurrentBlock(expressionStatement);
            }
            else
            {
                //push expression onto the stack
                stack.Push(binaryExpression);
            }
        }

        private void ProcessNeg()
        {
            if (stack.Count > 0)
            {
                if (stack.Peek() is DataIndex)
                {
                    DataIndex stackTop = (DataIndex)stack.Pop();
                    stackTop.IsNegative = !stackTop.IsNegative;
                    stack.Push(stackTop);
                }
                else
                {
                    var expressionSyntax = stack.Pop() as ExpressionSyntax;
                    expressionSyntax = SyntaxFactory.ParenthesizedExpression(expressionSyntax);
                    expressionSyntax = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, expressionSyntax);
                    stack.Push(expressionSyntax);
                }
            }
        }

        private void ProcessNot()
        {
            ExpressionSyntax variableExpression = PopVariable();

            var notExpression = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, variableExpression);

            stack.Push(notExpression);
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
            ExpressionSyntax variableExpression = PopVariable();

            var variableIdentifier = SyntaxFactory.IdentifierName(VariableSet.Variables[OpCodes[codePointer].DataIndex.Value].Name);

            AssignmentExpressionSyntax assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, variableExpression);

            //is this a bad comparison that is actually an assign?
            if (OpCodes[codePointer + 1].OpCode == OpCodeType.OP_JMPF)
            {
                //push expression onto the stack
                stack.Push(assignment);
            }
            else
            {
                ////is this a double assign?
                //if (OpCodes[codePointer + 1].OpCode == OpCodeType.OP_GETTOP)
                //{
                //    var variableIdentifier2 = SyntaxFactory.IdentifierName(VariableSet.Variables[OpCodes[codePointer + 1].DataIndex.Value].Name);

                //    assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier2, assignment);
                //    codePointer++;
                //}
                var expressionStatement = SyntaxFactory.ExpressionStatement(assignment, semi);
                //add to the current block
                BlockHelper.AddToCurrentBlock(expressionStatement);
            }
        }

        private void ProcessFunction()
        {
            DataIndex function = (DataIndex)stack.Pop();
            string functionName = FunctionTable.Functions[function.Value];
            ExpressionSyntax expressionSyntax = SyntaxFactory.IdentifierName(functionName);
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList();
            List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();

            //reverse the stacks
            stack = new Stack<object>(stack);
            while (stack.Count > 0)
            {
                ExpressionSyntax paramSyntax = PopVariable();
                arguments.Add(SyntaxFactory.Argument(paramSyntax));
                // IsCharacterDead only takes one parameter and we need to cope with bug
                if (functionName == "IsCharacterDead"
                    && stack.Count > 0)
                {
                    // cope with bug
                    if (OpCodes[codePointer + 1].OpCode == OpCodeType.OP_EQUAL)
                    {
                        arguments.Clear();
                        ExpressionSyntax paramSyntax2 = PopVariable();
                        arguments.Add(SyntaxFactory.Argument(paramSyntax2));
                        stack.Push(paramSyntax);
                    }
                    break;
                }
            }
            argumentList = argumentList.AddArguments(arguments.ToArray());

            InvocationExpressionSyntax invocationExpression = SyntaxFactory.InvocationExpression(expressionSyntax, argumentList);

            ExpressionStatementSyntax expressionStatement = SyntaxFactory.ExpressionStatement(invocationExpression, semi);

            // are we assigning to an equal as a bug?
            if (OpCodes[codePointer + 1].OpCode == OpCodeType.OP_EQUAL)
            {
                var variableIdentifier = PopVariable();
                var binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, variableIdentifier, invocationExpression);
                expressionStatement = SyntaxFactory.ExpressionStatement(binaryExpression, semi);
                //make sure we move past the OP_EQUAL
                codePointer++;
            }
            {
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
                    //add to the current block
                    var variableIdentifier = SyntaxFactory.IdentifierName(VariableSet.Variables[OpCodes[codePointer + 1].DataIndex.Value].Name);
                    var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, expressionStatement.Expression);
                    expressionStatement = SyntaxFactory.ExpressionStatement(assignment, semi);

                    //make sure we move past the OP_GETTOP
                    codePointer++;
                }
            }

            //add to the current block
            BlockHelper.AddToCurrentBlock(expressionStatement);
        }

        private void ProcessPrint()
        {
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList();
            ExpressionSyntax argument = PopVariable();
            List<ArgumentSyntax> arguments = new List<ArgumentSyntax>
            {
                SyntaxFactory.Argument(argument)
            };

            argumentList = argumentList.AddArguments(arguments.ToArray());

            ExpressionSyntax expressionSyntax = SyntaxFactory.IdentifierName("Print");

            InvocationExpressionSyntax invocationExpression = SyntaxFactory.InvocationExpression(expressionSyntax, argumentList);

            ExpressionStatementSyntax expressionStatement = SyntaxFactory.ExpressionStatement(invocationExpression, semi);

            var statements = new SyntaxList<StatementSyntax>().Add(expressionStatement);
            //add to the current block
            BlockHelper.AddToCurrentBlock(expressionStatement);
            //currentBlock = currentBlock.WithStatements(statements);

            //GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);
        }

        private void ProcessJump(Operation jumpOp)
        {
            //a jump back means this must be a while
            if (jumpOp.DataIndex.Value < jumpOp.Address)
            {
                BlockHelper.ConvertIfToWhile();
            }
            else
            {
                //current block is the if so we need to modify the if with a new block then set it as current
                BlockHelper.AddElseToIfBlock();
            }

        }

        private void ProcessJumpF()
        {
            ExpressionSyntax expressionSyntax = PopVariable();
            if (expressionSyntax == null)
            {
                return;
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
            var previousOperation = OpCodes[codePointer - 1];
            //if previous opcode was a jump out of an if
            if (previousOperation.OpCode == OpCodeType.OP_JMP
                && previousOperation.DataIndex.Value > previousOperation.Address)
            {
                return;
            }

            BlockHelper.GetPreviousBlock();
        }

        private ExpressionSyntax GetVariableExpression(DataIndex dataIndex)
        {
            short variable = dataIndex.Value;

            ExpressionSyntax expressionSyntax = null;
            //if not static condition is the variable itself
            if (!VariableSet.Variables[variable].Static)
            {
                expressionSyntax = SyntaxFactory.IdentifierName(VariableSet.Variables[variable].Name);
                if (dataIndex.IsNegative)
                {
                    expressionSyntax = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, expressionSyntax);
                }
            }
            else
            {
                var dataType = VariableSet.Variables[variable].DataType;

                if (dataType == "String")
                {
                    object objValue = VariableSet.GetCurrentValue(variable);
                    string value = (string)objValue;
                    //value = value.ToString();
                    //value = value.Replace("\"", "");
                    //SyntaxTriviaList syntaxTriviaList = new SyntaxTriviaList();
                    var syntaxToken = SyntaxFactory.ParseToken("\"" + value + "\"");
                    //syntaxToken = SyntaxFactory.Literal(syntaxTriviaList, value, syntaxTriviaList,
                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, syntaxToken);
                }
                else if (dataType == "Float")
                {
                    //Double
                    float value = float.Parse(VariableSet.GetCurrentValue(variable).ToString().Replace("\"", "").Replace(@"\", ""));
                    if (dataIndex.IsNegative)
                    {
                        value = value * -1;
                    }

                    expressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
                    // cope with -0 !!!
                    if (value == 0
                        && dataIndex.IsNegative)
                    {
                        expressionSyntax = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, expressionSyntax);
                    }
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
