using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using banallib;
using System.Windows.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace banalg
{
    public class MainWindowViewModel : ObservableObject
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
                    RaisePropertyChanged(() => this.Analyse);
                }
            }
        }

        private bool _saveAsNew;
        public bool SaveAsNew
        {
            get
            {
                return _saveAsNew;
            }
            set
            {
                if (_saveAsNew != value)
                {
                    _saveAsNew = value;
                    RaisePropertyChanged(() => this.SaveAsNew);
                }
            }
        }

        private bool _buildNew;
        public bool BuildNew
        {
            get
            {
                return _buildNew;
            }
            set
            {
                if (_buildNew != value)
                {
                    _buildNew = value;
                    RaisePropertyChanged(() => this.BuildNew);
                }
            }
        }

        private string _fileName;
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    RaisePropertyChanged(() => this.FileName);
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
                    RaisePropertyChanged(() => this.SourceCode);
                }
            }
        }

        private short _baseAddress;
        public short BaseAddress
        {
            get
            {
                return _baseAddress;
            }
            set
            {
                if (_baseAddress != value)
                {
                    _baseAddress = value;
                    RaisePropertyChanged(() => this.BaseAddress);
                }
            }
        }
        //1AE = 430
        //25D = 605
        //2F8 = 760
        private short _newBase;
        public short NewBase
        {
            get
            {
                return _newBase;
            }
            set
            {
                if (_newBase != value)
                {
                    _newBase = value;
                    RaisePropertyChanged(() => this.NewBase);
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
                    RaisePropertyChanged(() => this.ParsedContent);
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
                    RaisePropertyChanged(() => this.ValueList);
                }
            }
        }

        private List<OpCodeViewModel> _opCodesList;
        public List<OpCodeViewModel> OpCodeList
        {
            get
            {
                return _opCodesList;
            }
            set
            {
                if (_opCodesList != value)
                {
                    _opCodesList = value;
                    RaisePropertyChanged(() => this.OpCodeList);
                }
            }
        }

        private List<ParsedOperationViewModel> _parsedOperationList;
        public List<ParsedOperationViewModel> ParsedOperationList
        {
            get
            {
                return _parsedOperationList;
            }
            set
            {
                if (_parsedOperationList != value)
                {
                    _parsedOperationList = value;
                    RaisePropertyChanged(() => this.ParsedOperationList);
                }
            }
        }

        private ICommand _loadFileCommand = null;
        public ICommand LoadFileCommand
        {
            get
            {
                if (_loadFileCommand == null)
                {
                    _loadFileCommand = new RelayCommand(ExecuteLoadFileCommand, CanExecuteLoadFileCommand);
                }

                return _loadFileCommand;
            }
        }
        public bool CanExecuteLoadFileCommand()
        {
            return true;
        }
        public void ExecuteLoadFileCommand()
        {
            this.LoadFile(FileName);
        }


        public MainWindowViewModel()
        {
            FunctionTable.LoadData();

            this.Analyse = new Analyse();

            this.ValueList = new List<ValueViewModel>();

            //this.FileName = @"F:\Games\Call Of Cthulhu DCoTE\Scripts\gamescripts\08_boat\yithmindswaps\ms_caproomdeepattack.bin";
            //this.FileName = @"S:\Games\Call Of Cthulhu DCoTE\Scripts\bin\scripts\10_air_filled_tunnels\tunnelsstartup.bin";
            this.FileName = @"S:\Games\Call Of Cthulhu DCoTE\Scripts\bin\gamescripts\03_streets_two\trigsewerleavedeathpittoproom.bin";
            //this.NewBase = 760;

        }

        //build list of variables
        Dictionary<short, Variable> variables = new Dictionary<short, Variable>();
        //SyntaxTree tree = null;
        //CompilationUnitSyntax root = null;
        List<GlobalStatementSyntax> globalStatements = new List<GlobalStatementSyntax>();
        //Stack<BlockSyntax> blockStack = null;
        //SyntaxToken semi = Syntax.Token(SyntaxKind.SemicolonToken, Syntax.EndOfLine("\n"));
        //int codePointer = 0;

        //private void ParseOpCodes()
        //{
        //    //create new block stack
        //    blockStack = new Stack<BlockSyntax>();

        //    codePointer = ParsedContent.OpCodeList.Count - 1;

        //    //add our base block to stack
        //    BlockSyntax block = Syntax.Block();
        //    blockStack.Push(block);

        //    while (codePointer >= 0)
        //    {
        //        processOpCode();
        //    }

        //    GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(blockStack.Peek());
        //    globalStatements.Add(globalStatement);
        //}

        //private void processOpCode()
        //{
        //    Operation operation = ParsedContent.OpCodeList[codePointer];

        //    if (operation.OpCode == OpCodeType.JUMPTARGET)
        //    {
        //        ProcessJumpTarget();
        //    }
        //    else if (operation.OpCode == OpCodeType.OP_PRINT)
        //    {
        //        ProcessPrint();
        //    }
        //    else
        //    {
        //        codePointer--;
        //    }
        //}

        //private void ProcessJumpTarget()
        //{
        //    Operation jumpTarget = ParsedContent.OpCodeList[codePointer];
        //    codePointer--;

        //    //look at where we came from if its OP_JMP then
        //    Console.WriteLine("JUMPTARGET {0}", jumpTarget.DataIndex.Value);

        //    var q = from opcode in ParsedContent.OpCodeList
        //            where opcode.Address == jumpTarget.DataIndex.Value
        //            select opcode;
        //    Operation jumpSource = q.FirstOrDefault();
        //    if (jumpSource.OpCode == OpCodeType.OP_JMP)
        //    {
        //        //process as else
        //        ProcessElseBody(jumpTarget.DataIndex.Value);
        //        //create a body to host else content
        //    }
        //    else
        //    {
        //        //process as an if
        //        //processOpCode();
        //        //create a body to host if content
        //    }
        //}

        //private void ProcessIfBody()
        //{
        //    //throw new NotImplementedException();
        //}

        //private void ProcessElseBody(short jumptarget)
        //{
        //    BlockSyntax block = Syntax.Block();
        //    blockStack.Push(block);

        //    while (codePointer >= 0)
        //    {
        //        Operation op = 
        //        processOpCode();
        //    }

        //    StatementSyntax elseClause = Syntax.ElseClause(block);
        //    blockStack.Peek().AddStatements(elseClause);
        //}

        //private void ProcessPrint()
        //{
        //    codePointer--;
        //    Operation printParam = ParsedContent.OpCodeList[codePointer];
        //    codePointer--;

        //    ExpressionSyntax expressionSyntax = Syntax.IdentifierName("Print");
        //    ArgumentListSyntax argumentList = Syntax.ParseArgumentList("(" + GetCurrentValue(printParam.DataIndex.Value).ToString() + ")");

        //    InvocationExpressionSyntax invocationExpression = Syntax.InvocationExpression(expressionSyntax, argumentList);

        //    ExpressionStatementSyntax expressionStatement = Syntax.ExpressionStatement(invocationExpression, semi);

        //    //add to the current block
        //    blockStack.Peek().AddStatements(expressionStatement);

        //    //GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);
        //}

        //private void ParseOpCodesA()
        //{

        //    //process content
        //    for (int i = 0; i < ParsedContent.OpCodeList.Count; i++)
        //    {
        //        Operation operation = ParsedContent.OpCodeList[i];

        //        if (operation.OpCode == OpCodeType.OP_PRINT)
        //        {
        //            //ParsedOperationViewModel vm = new ParsedOperationViewModel();
        //            //vm.OperationType = OperationType.Print;

        //            Operation printparam = ParsedContent.OpCodeList[i - 1];

        //            ExpressionSyntax expressionSyntax = Syntax.IdentifierName("Print");
        //            ArgumentListSyntax argumentList = Syntax.ParseArgumentList("(" + GetCurrentValue(printparam.DataIndex.Value).ToString() + ")");

        //            InvocationExpressionSyntax invocationExpression = Syntax.InvocationExpression(expressionSyntax, argumentList);

        //            ExpressionStatementSyntax expressionStatement = Syntax.ExpressionStatement(invocationExpression, semi);
        //            GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);

        //            globalStatements.Add(globalStatement);
        //            //root = root.WithMembers(root.Members.Add(globalStatement));
        //        }
        //        //function call with assignment
        //        else if (operation.OpCode == OpCodeType.OP_FUNCTION
        //                && ParsedContent.OpCodeList[i + 1].OpCode == OpCodeType.OP_GETTOP)
        //        {
        //            Operation leftSide = ParsedContent.OpCodeList[i + 1];
        //            Operation function = ParsedContent.OpCodeList[i - 1];

        //            //ExpressionSyntax assignExpression = Syntax.ParseExpression("var" + leftSide.Address.ToString + " = " + rightSide.
        //            ExpressionSyntax leftSideExpression = null;
        //            if (variables.ContainsKey(leftSide.DataIndex.Value))
        //            {
        //                leftSideExpression = Syntax.IdentifierName(variables[leftSide.DataIndex.Value].Name);
        //            }
        //            else
        //            {
        //                leftSideExpression = Syntax.ParseExpression("");
        //            }

        //            //invocation
        //            ExpressionSyntax functionExpression = Syntax.IdentifierName(FunctionTable.Functions[function.DataIndex.Value].ToString());

        //            StringBuilder sb = new StringBuilder();

        //            int j = 2;
        //            bool lastWasNeg = false;
        //            while (ParsedContent.OpCodeList[i - j].OpCode == OpCodeType.OP_PUSH
        //                   || ParsedContent.OpCodeList[i - j].OpCode == OpCodeType.OP_NEG)
        //            {
        //                if (ParsedContent.OpCodeList[i - j].OpCode == OpCodeType.OP_NEG)
        //                {
        //                    lastWasNeg = true;
        //                    j++;
        //                    continue;
        //                }

        //                Operation param = ParsedContent.OpCodeList[i - j];

        //                if (j > 2)
        //                {
        //                    sb.Insert(0, ", ");
        //                }
        //                //use static value as not assigned to
        //                if (variables[param.DataIndex.Value].Static)
        //                {
        //                    if (lastWasNeg)
        //                    {
        //                        sb.Insert(0, "-" + GetCurrentValue(param.DataIndex.Value).ToString());
        //                    }
        //                    else
        //                    {
        //                        sb.Insert(0, GetCurrentValue(param.DataIndex.Value).ToString());
        //                    }
        //                }
        //                //use variable name
        //                else
        //                {
        //                    sb.Insert(0, variables[param.DataIndex.Value].Name);
        //                }

        //                lastWasNeg = false;
        //                j++;
        //            }
        //            sb.Insert(0, "(");
        //            sb.Append(")");

        //            ArgumentListSyntax argumentList = Syntax.ParseArgumentList(sb.ToString());


        //            InvocationExpressionSyntax invocationExpression = Syntax.InvocationExpression(functionExpression, argumentList);
        //            //ExpressionStatementSyntax expressionStatement = Syntax.ExpressionStatement(invocationExpression, semi);

        //            ExpressionSyntax assignExpression = Syntax.BinaryExpression(SyntaxKind.AssignExpression, leftSideExpression, invocationExpression);

        //            ExpressionStatementSyntax expressionStatement = Syntax.ExpressionStatement(assignExpression, semi);
        //            GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);

        //            globalStatements.Add(globalStatement);
        //            //root = root.WithMembers(root.Members.Add(globalStatement));
        //        }
        //        else if (operation.OpCode == OpCodeType.OP_FUNCTION)
        //        {
        //            Operation function = null;
        //            function = ParsedContent.OpCodeList[i - 1];

        //            //invocation
        //            ExpressionSyntax functionExpression = Syntax.IdentifierName(FunctionTable.Functions[function.DataIndex.Value].ToString());

        //            StringBuilder sb = new StringBuilder();
        //            int j = 2;
        //            bool lastWasNeg = false;
        //            while (ParsedContent.OpCodeList[i - j].OpCode == OpCodeType.OP_PUSH
        //                  || ParsedContent.OpCodeList[i - j].OpCode == OpCodeType.OP_NEG)
        //            {
        //                if (ParsedContent.OpCodeList[i - j].OpCode == OpCodeType.OP_NEG)
        //                {
        //                    lastWasNeg = true;
        //                    j++;
        //                    continue;
        //                }
        //                else
        //                {
        //                    lastWasNeg = false;
        //                }

        //                Operation param = ParsedContent.OpCodeList[i - j];

        //                if (j > 2)
        //                {
        //                    sb.Insert(0, ", ");
        //                }
        //                //use static value as not assigned to
        //                if (variables[param.DataIndex.Value].Static)
        //                {
        //                    if (lastWasNeg)
        //                    {
        //                        sb.Insert(0, "-" + GetCurrentValue(param.DataIndex.Value).ToString());
        //                    }
        //                    else
        //                    {
        //                        sb.Insert(0, GetCurrentValue(param.DataIndex.Value).ToString());
        //                    }
        //                }
        //                //use variable name
        //                else
        //                {
        //                    sb.Insert(0, variables[param.DataIndex.Value].Name);
        //                }
        //                j++;
        //            }
        //            sb.Insert(0, "(");
        //            sb.Append(")");

        //            ArgumentListSyntax argumentList = Syntax.ParseArgumentList(sb.ToString());

        //            InvocationExpressionSyntax invocationExpression = Syntax.InvocationExpression(functionExpression, argumentList);
        //            //ExpressionStatementSyntax expressionStatement = Syntax.ExpressionStatement(invocationExpression, semi);

        //            //ExpressionSyntax assignExpression = Syntax.BinaryExpression(SyntaxKind.AssignExpression, leftSideExpression, invocationExpression);

        //            ExpressionStatementSyntax expressionStatement = Syntax.ExpressionStatement(invocationExpression, semi);
        //            GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);

        //            globalStatements.Add(globalStatement);
        //            //root = root.WithMembers(root.Members.Add(globalStatement));
        //        }
        //        else if (operation.OpCode == OpCodeType.OP_GETTOP
        //                 && (ParsedContent.OpCodeList[i - 1].OpCode == OpCodeType.OP_PUSH
        //                    || ParsedContent.OpCodeList[i - 1].OpCode == OpCodeType.OP_NEG))
        //        {
        //            Operation leftSide = ParsedContent.OpCodeList[i];
        //            Operation rightSide = null;
        //            if (ParsedContent.OpCodeList[i - 1].OpCode == OpCodeType.OP_NEG)
        //            {
        //                rightSide = ParsedContent.OpCodeList[i - 2];
        //            }
        //            else
        //            {
        //                rightSide = ParsedContent.OpCodeList[i - 1];
        //            }

        //            //ExpressionSyntax assignExpression = Syntax.ParseExpression("var" + leftSide.Address.ToString + " = " + rightSide.
        //            ExpressionSyntax leftSideExpression = null;
        //            if (variables.ContainsKey(leftSide.DataIndex.Value))
        //            {
        //                leftSideExpression = Syntax.IdentifierName(variables[leftSide.DataIndex.Value].Name);
        //            }
        //            else
        //            {
        //                leftSideExpression = Syntax.ParseExpression("");
        //            }

        //            ExpressionSyntax rightSideExpression = null;
        //            if (variables.ContainsKey(rightSide.DataIndex.Value))
        //            {
        //                //use static value as not assigned to
        //                if (variables[rightSide.DataIndex.Value].Static)
        //                {
        //                    if (ParsedContent.OpCodeList[i - 1].OpCode == OpCodeType.OP_NEG)
        //                    {
        //                        rightSideExpression = Syntax.ParseExpression("-" + GetCurrentValue(rightSide.DataIndex.Value).ToString());
        //                    }
        //                    else
        //                    {
        //                        rightSideExpression = Syntax.ParseExpression(GetCurrentValue(rightSide.DataIndex.Value).ToString());
        //                    }
        //                }
        //                //use variable name
        //                else
        //                {
        //                    rightSideExpression = Syntax.IdentifierName(variables[rightSide.DataIndex.Value].Name);
        //                }
        //            }
        //            else
        //            {
        //                rightSideExpression = Syntax.ParseExpression("");
        //            }

        //            ExpressionSyntax assignExpression = Syntax.BinaryExpression(SyntaxKind.AssignExpression, leftSideExpression, rightSideExpression);

        //            ExpressionStatementSyntax expressionStatement = Syntax.ExpressionStatement(assignExpression, semi);
        //            GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(expressionStatement);

        //            globalStatements.Add(globalStatement);
        //            //root = root.WithMembers(root.Members.Add(globalStatement));
        //        }
        //        else if (operation.OpCode == OpCodeType.OP_EQUAL
        //                 || operation.OpCode == OpCodeType.OP_NOT_EQUAL
        //                 || operation.OpCode == OpCodeType.OP_MORE_THAN
        //                 || operation.OpCode == OpCodeType.OP_MORE_THAN_OR_EQUAL
        //                 || operation.OpCode == OpCodeType.OP_LESS_THAN
        //                 || operation.OpCode == OpCodeType.OP_LESS_THAN_OR_EQUAL)
        //        {
        //            Operation leftSide = null;
        //            Operation rightSide = null;
        //            if (ParsedContent.OpCodeList[i - 1].OpCode == OpCodeType.OP_NEG)
        //            {
        //                rightSide = ParsedContent.OpCodeList[i - 2];
        //                leftSide = ParsedContent.OpCodeList[i - 3];
        //            }
        //            else
        //            {
        //                rightSide = ParsedContent.OpCodeList[i - 1];
        //                leftSide = ParsedContent.OpCodeList[i - 2];
        //            }

        //            ExpressionSyntax leftSideExpression = null;
        //            if (variables.ContainsKey(leftSide.DataIndex.Value))
        //            {
        //                leftSideExpression = Syntax.IdentifierName(variables[leftSide.DataIndex.Value].Name);
        //            }
        //            else
        //            {
        //                leftSideExpression = Syntax.ParseExpression("");
        //            }

        //            ExpressionSyntax rightSideExpression = null;
        //            if (variables.ContainsKey(rightSide.DataIndex.Value))
        //            {
        //                //use static value as not assigned to
        //                if (variables[rightSide.DataIndex.Value].Static)
        //                {
        //                    if (ParsedContent.OpCodeList[i - 1].OpCode == OpCodeType.OP_NEG)
        //                    {
        //                        rightSideExpression = Syntax.ParseExpression("-" + GetCurrentValue(rightSide.DataIndex.Value).ToString());
        //                    }
        //                    else
        //                    {
        //                        rightSideExpression = Syntax.ParseExpression(GetCurrentValue(rightSide.DataIndex.Value).ToString());
        //                    }
        //                }
        //                //use variable name
        //                else
        //                {
        //                    rightSideExpression = Syntax.IdentifierName(variables[rightSide.DataIndex.Value].Name);
        //                }
        //            }
        //            else
        //            {
        //                rightSideExpression = Syntax.ParseExpression("");
        //            }


        //            SyntaxToken conditionToken;
        //            if (operation.OpCode == OpCodeType.OP_NOT_EQUAL)
        //            {
        //                conditionToken = Syntax.Token(SyntaxKind.ExclamationEqualsToken, Syntax.Whitespace(" "));
        //            }
        //            else if (operation.OpCode == OpCodeType.OP_MORE_THAN)
        //            {
        //                conditionToken = Syntax.Token(SyntaxKind.GreaterThanToken, Syntax.Whitespace(" "));
        //            }
        //            else if (operation.OpCode == OpCodeType.OP_MORE_THAN_OR_EQUAL)
        //            {
        //                conditionToken = Syntax.Token(SyntaxKind.GreaterThanEqualsToken, Syntax.Whitespace(" "));
        //            }
        //            else if (operation.OpCode == OpCodeType.OP_LESS_THAN)
        //            {
        //                conditionToken = Syntax.Token(SyntaxKind.LessThanToken, Syntax.Whitespace(" "));
        //            }
        //            else if (operation.OpCode == OpCodeType.OP_LESS_THAN_OR_EQUAL)
        //            {
        //                conditionToken = Syntax.Token(SyntaxKind.LessThanEqualsToken, Syntax.Whitespace(" "));
        //            }
        //            else
        //            {
        //                conditionToken = Syntax.Token(SyntaxKind.EqualsEqualsToken, Syntax.Whitespace(" "));
        //            }

        //            ExpressionSyntax condition = Syntax.BinaryExpression(SyntaxKind.EqualsExpression, leftSideExpression.WithTrailingTrivia(Syntax.Whitespace(" ")), conditionToken, rightSideExpression);
        //            StatementSyntax statement = Syntax.Block(Syntax.Token(SyntaxKind.OpenBraceToken, Syntax.EndOfLine("\n")),
        //                                                     null,
        //                                                     Syntax.Token(SyntaxKind.CloseBraceToken, Syntax.EndOfLine("\n")));

        //            //ElseClauseSyntax elseClause = Syntax.ElseClause(Syntax.Token(SyntaxKind.ElseKeyword, Syntax.EndOfLine("\n")),
        //            //                                                Syntax.Block(Syntax.Token(SyntaxKind.OpenBraceToken, Syntax.EndOfLine("\n")),
        //            //                                                null,
        //            //                                                Syntax.Token(SyntaxKind.CloseBraceToken, Syntax.EndOfLine("\n"))));

        //            StatementSyntax ifStatement = Syntax.IfStatement(Syntax.Token(SyntaxKind.IfKeyword, Syntax.Whitespace(" ")),
        //                                                             Syntax.Token(SyntaxKind.OpenParenToken),
        //                                                             condition,
        //                                                             Syntax.Token(SyntaxKind.CloseParenToken, Syntax.EndOfLine("\n")),
        //                                                             statement,
        //                                                             null);

        //            GlobalStatementSyntax globalStatement = Syntax.GlobalStatement(ifStatement);

        //            globalStatements.Add(globalStatement);
        //            //root = root.WithMembers(root.Members.Add(globalStatement));

        //        }
        //    }
        //}


        public void LoadFile(string filename)
        {
            this.ParsedContent = this.Analyse.AnalyseFile(filename);

            if (this.ParsedContent == null)
            {
                return;
            }

            BaseAddress = ParsedContent.BaseAddress;

            
            BuildOpCodes();

            BuildVariables();

            ////new
            //this.SourceCode = "";

            ////create tree
            //tree = SyntaxTree.ParseCompilationUnit("");
            //root = (CompilationUnitSyntax)tree.GetRoot();

            //AddVariablesToRoot();

            //ParseOpCodes();

            //AddCodeToRoot();

            //this.SourceCode = root.GetFullText().ToString();
        }

        //private void AddCodeToRoot()
        //{
        //    //add all the global statements
        //    foreach (GlobalStatementSyntax globalStatement in globalStatements)
        //    {
        //        root = root.WithMembers(root.Members.Add(globalStatement));
        //    }
        //}

        //private void AddVariablesToRoot()
        //{
        //    TypeSyntax intType = Syntax.ParseTypeName("int ");
        //    TypeSyntax stringType = Syntax.ParseTypeName("String ");
        //    TypeSyntax floatType = Syntax.ParseTypeName("float ");
        //    TypeSyntax pointType = Syntax.ParseTypeName("Point ");
        //    TypeSyntax characterType = Syntax.ParseTypeName("Character ");
        //    TypeSyntax quaternionType = Syntax.ParseTypeName("Quaternion ");
        //    SyntaxToken semi = Syntax.Token(SyntaxKind.SemicolonToken, Syntax.EndOfLine("\n"));

        //    var q = from variable in variables
        //            where variable.Value.Static == false
        //            orderby variable.Key
        //            select variable;
        //    foreach (var variable in q)
        //    {
        //        //sourceBuilder.Insert(0, string.Format("{0} variable{1};\n", variable.Value, variable.Key));

        //        // Build the field variable 1
        //        VariableDeclaratorSyntax declarator = Syntax.VariableDeclarator(
        //          identifier: Syntax.Identifier(variable.Value.Name)
        //        );

        //        TypeSyntax theType = null;
        //        switch (variable.Value.DataType)
        //        {
        //            case ("Int"):
        //                theType = intType;
        //                break;
        //            case ("String"):
        //                theType = stringType;
        //                break;
        //            case ("Point"):
        //                theType = pointType;
        //                break;
        //            case ("Character"):
        //                theType = characterType;
        //                break;
        //            case ("Float"):
        //                theType = floatType;
        //                break;
        //            case ("Quaternion"):
        //                theType = quaternionType;
        //                break;
        //        }

        //        FieldDeclarationSyntax newField = Syntax.FieldDeclaration(
        //            attributes: null,
        //            modifiers: new SyntaxTokenList(),
        //            declaration: Syntax.VariableDeclaration(type: theType,
        //            variables: Syntax.SeparatedList(declarator)),
        //            semicolonToken: semi);

        //        root = root.WithMembers(root.Members.Add(newField));

        //    }
        //}

        private void BuildOpCodes()
        {
            List<OpCodeViewModel> opcodeList = new List<OpCodeViewModel>();
            foreach (Operation operation in ParsedContent.OpCodeList)
            {
                OpCodeViewModel opCodeViewModel = new OpCodeViewModel()
                {
                    Address = operation.Address,
                    AddressHex = (short)(operation.Address + ParsedContent.StartOfOpCodes),
                    OpCode = operation.OpCode
                };
                if (operation.DataIndex != null)
                {
                    opCodeViewModel.DataIndexValue = operation.DataIndex.Value;
                    opCodeViewModel.DataIndexValueHex = (short)(operation.DataIndex.Value - ParsedContent.BaseAddress + ParsedContent.StartOfValues);
                    opCodeViewModel.DataIndexValueBase = (short)(operation.DataIndex.Value + ParsedContent.StartOfValues);
                    opCodeViewModel.DataIndexIsFunctionPointer = operation.DataIndex.IsFunctionPointer;
                    if (opCodeViewModel.DataIndexIsFunctionPointer)
                    {
                        if (FunctionTable.Functions.ContainsKey(opCodeViewModel.DataIndexValue))
                        {
                            opCodeViewModel.FunctionPointerName = FunctionTable.Functions[opCodeViewModel.DataIndexValue];
                        }
                    }
                }

                opcodeList.Add(opCodeViewModel);
            }
            this.OpCodeList = opcodeList;
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
                    switch (v.DataType.ToLower())
                    {
                        case ("int"):
                            v.Name = "nVar" + v.Address.ToString();
                            break;
                        case ("string"):
                            v.Name = "szVar" + v.Address.ToString();
                            break;
                        case ("point"):
                            v.Name = "ptVar" + v.Address.ToString();
                            break;
                        case ("character"):
                            v.Name = "cVar" + v.Address.ToString();
                            break;
                        case ("float"):
                            v.Name = "fVar" + v.Address.ToString();
                            break;
                        case ("quaternion"):
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

        private string GetValueDataType(short address)
        {
            var q = from v in this.ValueList
                    where v.Address == address
                    select v;
            ValueViewModel vvm = q.FirstOrDefault();

            return vvm.DataType.ToString();
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
