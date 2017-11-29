using banallib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SucDecompiler
{
    internal class DeCompiler
    {
        private Analyse _analyse;
        public Analyse Analyse
        {
            get
            {
                return _analyse;
            }
            set
            {
                if (_analyse != value)
                {
                    _analyse = value;
                    //RaisePropertyChanged(() => this.Analyse);
                }
            }
        }

        private ParsedContent _parsedContent;
        public ParsedContent ParsedContent
        {
            get
            {
                return _parsedContent;
            }
            set
            {
                if (_parsedContent != value)
                {
                    _parsedContent = value;
                    //RaisePropertyChanged(() => this.ParsedContent);
                }
            }
        }

        private List<ValueViewModel> _valueList;
        public List<ValueViewModel> ValueList
        {
            get
            {
                return _valueList;
            }
            set
            {
                if (_valueList != value)
                {
                    _valueList = value;
                    //RaisePropertyChanged(() => this.ValueList);
                }
            }
        }

        private string _sourceCode;
        public string SourceCode
        {
            get
            {
                return _sourceCode;
            }
            set
            {
                if (_sourceCode != value)
                {
                    _sourceCode = value;
                    //RaisePropertyChanged(() => this.SourceCode);
                }
            }
        }
        //build list of variables
        Dictionary<short, Variable> variables = new Dictionary<short, Variable>();

        CSharpSyntaxTree tree = null;
        CompilationUnitSyntax root = null;
        bool firstPrint = true;

        List<GlobalStatementSyntax> globalStatements = new List<GlobalStatementSyntax>();
        Stack<BlockSyntax> blockStack = null;
        Stack<DataIndex> stack = null;
        //Stack<BlockSyntax> blockStack = null;
        int codePointer = 0;
        SyntaxToken semi = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n")));

        public DeCompiler()
            {
            FunctionTable.LoadData();

            this.Analyse = new Analyse();

            this.ValueList = new List<ValueViewModel>();
            //this.Analyse.NewBase = 605;
            //this.ValueList = new List<ValueViewModel>();
        }

        public string SourceDirectory { get; internal set; }
        public string TargetDirectory { get; internal set; }

        internal void DeCompileDirectory(string source)
        {
            //this.DirectoryBased = true;
            string[] files = Directory.GetFiles(source, "*.bin", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                //  foreach (var filename in Directory.GetFiles(source, "*.bin", SearchOption.AllDirectories))
                //{

                string filename = file.Replace(source + @"\", "");
                filename = filename.Replace(Directory.GetCurrentDirectory() + @"\", "");
                if (filename.StartsWith("F")
                    || filename.StartsWith("f"))
                {
                    Console.WriteLine("{0}!!!", filename);
                    continue;
                }
                DeCompileFile(filename);
                //this.ParsedContent = this.Analyse.AnalyseFile(filename);

                //if (this.ParsedContent == null)
                //{
                //    return;
                //}
            }
        }

        internal void DeCompileFile(string filename)
        {
            string sourceFilename = Path.Combine(SourceDirectory, filename);
            string targetFilename = Path.Combine(TargetDirectory, filename.Replace(".bin", ".hfs"));
            string targetDir = Path.GetDirectoryName(targetFilename);

            variables.Clear();
            //blockStack.Clear();
            codePointer = 0;
            globalStatements.Clear();
            this.ValueList.Clear();

            this.ParsedContent = this.Analyse.AnalyseFile(sourceFilename);

            if (this.ParsedContent == null)
            {
                return;
            }

            BuildSource();

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            File.WriteAllText(targetFilename, this.SourceCode.Replace(@"\\",@"\"));
        }

        private void BuildSource()
        {
            this.SourceCode = "";

            //create tree
            CSharpParseOptions cSharpParseOptions = new CSharpParseOptions(LanguageVersion.CSharp1, DocumentationMode.Parse, SourceCodeKind.Script);
            tree = CSharpSyntaxTree.ParseText("", options: cSharpParseOptions) as CSharpSyntaxTree;
            root = tree.GetCompilationUnitRoot(); //(CompilationUnitSyntax)tree.GetRoot();
            firstPrint = true;

            BuildVariables();
        
            ParseOpCodes();

            AddCodeToRoot();

            //AddVariablesToRoot();

            this.SourceCode = root.GetText().ToString(); //.GetFullText().ToString();
        }

        private void AddCodeToRoot()
        {
            //add all the global statements
            foreach (GlobalStatementSyntax globalStatement in globalStatements)
            {
                root = root.WithMembers(root.Members.Add(globalStatement));
            }
        }

        private void BuildVariables()
        {
            List<ValueViewModel> valueList = new List<ValueViewModel>();
            foreach (Value value in this.ParsedContent.ValuesList)
            {
                ValueViewModel valueViewModel = new ValueViewModel()
                {
                    Address = value.Address,
                    AddressHex = (short)(value.Address - ParsedContent.BaseAddress + ParsedContent.StartOfValues),
                    AddressHexBase = (short)(value.Address + ParsedContent.StartOfValues),
                    DataType = value.DataType,
                    Reference = value.Reference
                };
                if (value.SubValues.Count > 0)
                {
                    if (valueViewModel.DataType == DataTypeType.Float)
                    {
                        valueViewModel.SubValue1 = BitConverter.ToSingle(BitConverter.GetBytes((Int32)(value.SubValues[0])), 0);
                    }
                    else
                    {
                        valueViewModel.SubValue1 = value.SubValues[0];
                    }
                }
                if (value.SubValues.Count > 1)
                {
                    valueViewModel.SubValue2 = value.SubValues[1];
                }
                if (value.SubValues.Count > 2)
                {
                    valueViewModel.SubValue3 = value.SubValues[2];
                }
                if (value.SubValues.Count > 3)
                {
                    valueViewModel.SubValue4 = value.SubValues[3];
                }
                if (value.SubValues.Count > 4)
                {
                    valueViewModel.SubValue5 = value.SubValues[4];
                }

                valueList.Add(valueViewModel);


                //build variables list
                if (!variables.ContainsKey(value.Address))
                {
                    Variable v = new Variable()
                    {
                        Address = value.Address,
                        DataType = value.DataType.ToString()
                        //Name = "var" + value.DataType.ToString() + value.Address.ToString()
                    };
                    switch (v.DataType)
                    {
                        case ("Int"):
                            v.Name = "nVar" + v.Address.ToString();
                            break;
                        case ("String"):
                            v.Name = "szVar" + v.Address.ToString();
                            break;
                        case ("Point"):
                            v.Name = "ptVar" + v.Address.ToString();
                            break;
                        case ("Character"):
                            v.Name = "cVar" + v.Address.ToString();
                            break;
                        case ("Float"):
                            v.Name = "fVar" + v.Address.ToString();
                            break;
                        case ("Quartnian"):
                            v.Name = "qVar" + v.Address.ToString();
                            break;
                        case ("Quaternion"):
                            v.Name = "qVar" + v.Address.ToString();
                            break;
                    }


                    variables.Add(value.Address, v);
                }


            }
            this.ValueList = valueList;

            //work out which variables are static
            for (int i = 0; i < ParsedContent.OpCodeList.Count; i++)
            {
                Operation operation = ParsedContent.OpCodeList[i];
                if (operation.OpCode == OpCodeType.OP_GETTOP)
                {
                    if (variables.ContainsKey(operation.DataIndex.Value))
                    {
                        variables[operation.DataIndex.Value].Static = false;
                    }
                }
            }
        }

        private BlockSyntax AddVariablesToBlock(BlockSyntax block)
        {
            TypeSyntax intType = SyntaxFactory.ParseTypeName("int ");
            TypeSyntax stringType = SyntaxFactory.ParseTypeName("String ");
            TypeSyntax floatType = SyntaxFactory.ParseTypeName("double ");
            TypeSyntax pointType = SyntaxFactory.ParseTypeName("Point ");
            TypeSyntax characterType = SyntaxFactory.ParseTypeName("Character ");
            TypeSyntax quartonianType = SyntaxFactory.ParseTypeName("Quartonian ");

            //find insertion point after first print
            //root.

            var q = from variable in variables
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
                block = block.AddStatements(localDeclaration);
            }

            return block;
        }

        private void ParseOpCodes()
        {
            //create new block stack
            blockStack = new Stack<BlockSyntax>();
            stack = new Stack<DataIndex>();
 
            //codePointer = ParsedContent.OpCodeList.Count - 1;
            codePointer = 0;
            //add our base block to stack
            BlockSyntax block = SyntaxFactory.Block();
            blockStack.Push(block);

            while (codePointer < ParsedContent.OpCodeList.Count)
            {
                processOpCode();
            }

            block = blockStack.Pop();

            foreach (var statement in block.Statements)
            {
                GlobalStatementSyntax globalStatement = SyntaxFactory.GlobalStatement(statement);
                globalStatements.Add(globalStatement);
            }
        }

        private void processOpCode()
        {
            Operation operation = ParsedContent.OpCodeList[codePointer];

            if (operation.OpCode == OpCodeType.OP_JMP)
            {
                //ProcessJumpTarget();
                stack.Clear();
            }
            if (operation.OpCode == OpCodeType.OP_JMPF)
            {
                //ProcessJumpTarget();
                stack.Clear();
            }
            else if (operation.OpCode == OpCodeType.JUMPTARGET)
            {
                //ProcessJumpTarget();
                stack.Clear();
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
            codePointer++;
        }

        private void ProcessAssign()
        {
            DataIndex assignValue = stack.Pop();

            var variableIdentifier = SyntaxFactory.IdentifierName(variables[ParsedContent.OpCodeList[codePointer].DataIndex.Value].Name);

            AssignmentExpressionSyntax assignment = null;
            if (!variables[assignValue.Value].Static)
            {
                var valueIdentifier = SyntaxFactory.IdentifierName(variables[assignValue.Value].Name);
                assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, valueIdentifier);
            }
            else
            {
                string dataType = variables[assignValue.Value].DataType;

                LiteralExpressionSyntax literalExpression = null;
                if (dataType == "String")
                {
                    string value = (string)GetCurrentValue(assignValue.Value).ToString().Replace("\"", "");
                    literalExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
                }
                else if (dataType == "Float")
                {
                    float value = float.Parse(GetCurrentValue(assignValue.Value).ToString().Replace("\"", "").Replace(@"\", ""));
                    literalExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
                }
                else if (dataType == "Int")
                {
                    int value = int.Parse(GetCurrentValue(assignValue.Value).ToString().Replace("\"", "").Replace(@"\", ""));
                    literalExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
                }
                else
                {
                    throw (new NotImplementedException());
                }
                assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, literalExpression);
            }

            //var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, expressionStatement.Expression);
            var expressionStatement = SyntaxFactory.ExpressionStatement(assignment, semi);

            //expressionStatement

            //add to the current block
            BlockSyntax currentBlock = blockStack.Pop();
            currentBlock = currentBlock.AddStatements(expressionStatement);
            blockStack.Push(currentBlock);
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

                if (!variables[param.Value].Static)
                {
                    //argument = variables[param.Value].Name;
                    arguments.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(variables[param.Value].Name)));
                }
                else
                {
                    string dataType = variables[param.Value].DataType;
                    
                    if (dataType == "String")
                    {
                        string value = (string)GetCurrentValue(param.Value).ToString().Replace("\"", "");
                        arguments.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value))));
                    }
                    else if(dataType == "Float")
                    {
                        float value = float.Parse(GetCurrentValue(param.Value).ToString().Replace("\"", "").Replace(@"\", ""));
                        arguments.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value))));
                    }
                    else if (dataType == "Int")
                    {
                        int value = int.Parse(GetCurrentValue(param.Value).ToString().Replace("\"", "").Replace(@"\", ""));
                        arguments.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value))));
                    }

                }

            }
            argumentList = argumentList.AddArguments(arguments.ToArray());

            InvocationExpressionSyntax invocationExpression = SyntaxFactory.InvocationExpression(expressionSyntax, argumentList);

            ExpressionStatementSyntax expressionStatement = SyntaxFactory.ExpressionStatement(invocationExpression, semi);

            //are we assigning to a variable?
            if (ParsedContent.OpCodeList[codePointer + 1].OpCode == OpCodeType.OP_GETTOP)
            {
                var variableIdentifier = SyntaxFactory.IdentifierName(variables[ParsedContent.OpCodeList[codePointer + 1].DataIndex.Value].Name);
                var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, variableIdentifier, expressionStatement.Expression);
                expressionStatement = SyntaxFactory.ExpressionStatement(assignment, semi);

                //make sure we move past the OP_GETTOP
                codePointer++;
            }

            //add to the current block
            BlockSyntax currentBlock = blockStack.Pop();
            currentBlock = currentBlock.AddStatements(expressionStatement);
            blockStack.Push(currentBlock);
        }

        private void ProcessPrint()
        {
            DataIndex printParam = null;
            string argument;
            if (stack.Count > 0)
            {
                printParam = stack.Pop();

                if (!variables[printParam.Value].Static)
                {
                    argument = variables[printParam.Value].Name;
                }
                else
                {
                    argument = GetCurrentValue(printParam.Value).ToString();
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

            //add to the current block
            BlockSyntax currentBlock = blockStack.Pop();
            currentBlock = currentBlock.AddStatements(expressionStatement);

            if (firstPrint)
            {
                firstPrint = false;
                currentBlock = AddVariablesToBlock(currentBlock); // = currentBlock.AddStatements(expressionStatement);
            }

            blockStack.Push(currentBlock);

            //GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);
        }

        private void ProcessJumpTarget()
        {
            Operation jumpTargetOperation = ParsedContent.OpCodeList[codePointer];
            //codePointer--;

            DataIndex jumpTarget = stack.Pop();

            //look at where we came from if its OP_JMP then
            Console.WriteLine("JUMPTARGET {0}", jumpTarget.Value);

            var q = from opcode in ParsedContent.OpCodeList
                    where opcode.Address == jumpTarget.Value
                    select opcode;
            Operation jumpSource = q.FirstOrDefault();
            if (jumpSource.OpCode == OpCodeType.OP_JMP)
            {
                //process as else
                ProcessElseBody(jumpTargetOperation.DataIndex.Value);
                //create a body to host else content
            }
            else
            {
                //process as an if
                //processOpCode();
                //create a body to host if content
            }
            codePointer++;
        }

        private void ProcessIfBody()
        {
            //throw new NotImplementedException();
        }

        private void ProcessElseBody(short jumptarget)
        {
            BlockSyntax block = SyntaxFactory.Block();
            blockStack.Push(block);

            while (codePointer >= 0)
            {
                //Operation op = // TODO - this was left unfinished
                processOpCode();
            }

            //ElseClauseSyntax elseClause = SyntaxFactory.ElseClause(block); //TODO this doesn't work
            //blockStack.Peek().AddStatements((StatementSyntax)elseClause);
        }
        private object GetCurrentValue(short address)
        {
            var q = from v in this.ValueList
                    where v.Address == address
                    select v;
            ValueViewModel vvm = q.FirstOrDefault();

            StringBuilder sb = new StringBuilder();
            if (vvm.SubValue1 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue1));
            }
            if (vvm.SubValue2 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue2));
            }
            if (vvm.SubValue3 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue3));
            }
            if (vvm.SubValue4 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue4));
            }
            if (vvm.SubValue5 != null)
            {
                sb.Append(GetSubValue(vvm, vvm.SubValue5));
            }

            return sb.ToString();
        }

        private string GetSubValue(ValueViewModel vvm, object sv)
        {
            if (vvm.DataType == DataTypeType.String)
            {
                return string.Format("\"{0}\"", sv);
            }

            return sv.ToString();
        }
    }
}