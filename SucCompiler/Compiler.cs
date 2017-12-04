using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CocCompiler.OpCodes;

namespace CocCompiler
{
    public class Compiler
    {
        private string _scriptFilename;
        public string ScriptFilename
        {
            get
            {
                return _scriptFilename;
            }
            set
            {
                _scriptFilename = value;
                //BinaryFilename = _scriptFilename.Replace(".hfs", ".coc");
                BinaryFilename = Path.Combine(TargetDirectory, _scriptFilename).Replace(".hfs", ".bin");
            } 
        
        }
        public string BinaryFilename { get; set; }


        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }

        private bool DirectoryBased { get; set; }

        //private int NumberOfOpCodesPosition;
        //private int StartOfValuesPosition;

        private short useBaseAddress = 760;

        public bool AddFunctions { get; set; }

        private short nextValueAddress = 760;
        private Int32 nextReferenceAddress = 0;

        private List<Variable> AllVariables = new List<Variable>();

        private Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();
        private Dictionary<LiteralExpressionSyntax, Variable> Literals = new Dictionary<LiteralExpressionSyntax, Variable>();
        private List<OpCode> OpCodes = new List<OpCode>();

        private int OpCodesSize = 0;

        private bool hasMissingFunctions = false;
        private bool justPatchFunctions = false;

        //private OP_JMP ElseJmp;

        private Stack<OP_JMP> ElseJmps = new Stack<OP_JMP>();

        private CSharpSyntaxTree tree;

        #region Base

        public void CompileDirectory(string directory)
        {
            this.DirectoryBased = true;
            string[] files = Directory.GetFiles(directory, "*.hfs", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                //Console.WriteLine("file={0}", file);
                //Console.WriteLine("cd={0}", Directory.GetCurrentDirectory());
                //Console.WriteLine("directory={0}", directory);

                string filename = file.Replace(directory + @"\", "");
                filename = filename.Replace(Directory.GetCurrentDirectory() + @"\", "");
                if (filename.StartsWith("F")
                    || filename.StartsWith("f"))
                {
                    Console.WriteLine("{0}!!!", filename);
                    continue;
                }
                CompileFile(filename);
            }
        }

        public void CompileFile(string filename)
        {
            //Console.WriteLine("Compiling {0}...", filename);
            Console.Write("Compiling {0}...", filename);

            if (filename.EndsWith(".bin"))
            {
                filename = filename.Replace(".bin", ".hfs");
            }

            this.ScriptFilename = filename;

            if (this.ScriptFilename.StartsWith(@"gamescripts\06_refinery_explosion")
                || this.ScriptFilename.StartsWith(@"scripts\06_refinery_explosion")
                || this.ScriptFilename.StartsWith(@"gamescripts\debug")
                )
            {
                useBaseAddress = 605;
            }
            else
            {
                useBaseAddress = 760;
            }

            //parse
            CompilationUnitSyntax root = this.Parse();

            //fix any case issue
            root = this.FixCase(root);

            //do the compile
            this.Compile(root);
            Console.WriteLine("OK");
        }

        private CompilationUnitSyntax FixCase(CompilationUnitSyntax root)
        {
            //var tokens = from token in root.DescendentTokens()
            //             where token.Value == "While"
            //             select token;
            //foreach (SyntaxToken token in tokens)
            //{
            //    SyntaxToken newtoken = Syntax.ex ("", "");
            //    //token.ValueText = "while";
            //}
            //ParseOptions parseOptions = new ParseOptions(CompatibilityMode.None, LanguageVersion.CSharp1, null, false, SourceCodeKind.Script);
            CSharpParseOptions cSharpParseOptions = new CSharpParseOptions(LanguageVersion.CSharp1, DocumentationMode.Parse, SourceCodeKind.Script);

            CSharpSyntaxRewriter cSharpSyntaxRewriter = new CaseFixRewriter();
            SyntaxNode newRoot = cSharpSyntaxRewriter.Visit(root); // new CaseFixRewriter().Visit(root);

            //tree = SyntaxTree.ParseCompilationUnit(newRoot.GetFullText(), options: parseOptions);
            tree = CSharpSyntaxTree.ParseText(newRoot.GetText(), options: cSharpParseOptions) as CSharpSyntaxTree;

            //tree = SyntaxTree.Create(tree.FileName, (CompilationUnitSyntax)newRoot, options: parseOptions);
            //Console.WriteLine(tree.Root.GetFullText());
            //Console.ReadLine();

            //string strings = tree.Root.GetFullText();


            //return (CompilationUnitSyntax)tree.Root;
            return tree.GetCompilationUnitRoot();
        }

        public CompilationUnitSyntax Parse()
        {
            //Console.WriteLine("Parsing...");

            string source_text = File.ReadAllText(Path.Combine(this.SourceDirectory, this.ScriptFilename), System.Text.Encoding.GetEncoding(1252));
            //Console.WriteLine("---Input---\n");
            //Console.WriteLine(source_text);

            //ParseOptions parseOptions = new ParseOptions(CompatibilityMode.None, LanguageVersion.CSharp1, null, false, SourceCodeKind.Script);
            CSharpParseOptions cSharpParseOptions = new CSharpParseOptions(LanguageVersion.CSharp1, DocumentationMode.Parse, SourceCodeKind.Script);
            //var tree = SyntaxTree.ParseCompilationUnit(source_text, parseOptions);

            //parse and get root
            //SyntaxTree tree = SyntaxTree.ParseCompilationUnit(source_text);
            //tree = SyntaxTree.ParseCompilationUnit(source_text, options: parseOptions);
            tree = CSharpSyntaxTree.ParseText(source_text, options: cSharpParseOptions) as CSharpSyntaxTree;

            //return (CompilationUnitSyntax)tree.Root;
            return tree.GetCompilationUnitRoot();
        }

        public bool Compile(CompilationUnitSyntax root)
        {
            List<byte> binary = new List<byte>();

            binary.Clear();
            AllVariables.Clear();
            Variables.Clear();
            Literals.Clear();
            OpCodes.Clear();
            OpCode.NextAddress = 0;
            nextValueAddress = useBaseAddress;
            nextReferenceAddress = 0;
            hasMissingFunctions = false;

            //get the variables
            ExtractVariables(root);

            //get the opcode data
            ExtractOpCodes(root);

            if (hasMissingFunctions
                && !justPatchFunctions)
            {
                if (FunctionTable.IsCorrectOpCodeSize(Path.Combine("binorig",this.ScriptFilename.Replace(".hfs", ".bin")), (short)OpCodesSize))
                {
                    justPatchFunctions = true;
                    Compile(root);
                    justPatchFunctions = false;
                    //save function table for new files to use
                    FunctionTable.SaveData();
                    return true;
                }
                else
                {
                    Console.WriteLine("Script {0} has missing functions", this.ScriptFilename);
                    return false;
                }
            }

            //set references on variables to actual data
            PopulateReferences();


            //write the header information
            WriteHeader(binary);

            //write opcodes
            WriteOpCodes(binary);

            //write value list
            WriteValues(binary);

            //write references
            WriteReferences(binary);

            //save the new binary file
            SaveBinary(BinaryFilename, binary);

            return true;
        }

        #endregion Base

        #region Parsing

        private void ExtractVariables(CompilationUnitSyntax root)
        {
            //add in the character that is always first
            Variable variable = new Variable()
            {
                Address = nextValueAddress,
                DataType = DataTypeType.Character,
                Name = "Me",
            };
            nextValueAddress++;

            AllVariables.Add(variable);
            Variables.Add(variable.Name, variable);
            //never referenced
            //Variables.Add(variableNumber.ToString(), variable);
            //variableNumber++;

            //add the player variable
            variable = new Variable()
            {
                Address = nextValueAddress,
                DataType = DataTypeType.Character,
                Name = "Player",
            };
            nextValueAddress++;

            AllVariables.Add(variable);
            Variables.Add(variable.Name, variable);
            //never referenced
            //Variables.Add(variableNumber.ToString(), variable);
            //variableNumber++;

            var nodes = from syntaxNodes in root.DescendantNodes()
                        where syntaxNodes.Kind() == SyntaxKind.FieldDeclaration
                           || syntaxNodes.Kind() == SyntaxKind.LocalDeclarationStatement
                           || syntaxNodes.Kind() == SyntaxKind.CharacterLiteralExpression
                           || syntaxNodes.Kind() == SyntaxKind.NumericLiteralExpression
                           || syntaxNodes.Kind() == SyntaxKind.StringLiteralExpression
                        select syntaxNodes;

            foreach (var node in nodes)
            {
                //how big is data?
                if (node.Kind() == SyntaxKind.FieldDeclaration)
                {
                    variable = new Variable()
                    {
                        Address = nextValueAddress,
                    };
                    nextValueAddress++;

                    FieldDeclarationSyntax field = node as FieldDeclarationSyntax;
                    variable.DataType = GetType(field.Declaration.Type);
                    if (variable.DataType == DataTypeType.Int)
                    {
                        variable.Value = 0;
                    }
                    else if (variable.DataType == DataTypeType.Float)
                    {
                        variable.Value = 0.0;
                    }

                    variable.Name = field.Declaration.Variables[0].Identifier.Value.ToString();

                    //Variables.Add(variable.Name, variable);
                    AllVariables.Add(variable);
                    Variables.Add(variable.Name, variable);
                    //Variables.Add(node, variable);

                    //SyntaxAnnotation 
                    //field.ChildNodesAndTokens().
                    //var formattedLocalDeclaration = FormattingAnnotation //Annotations.FormattingAnnotation.AddAnnotationTo(node);
                    //SyntaxAnnotation annotation = new SyntaxAnnotation();
                    //annotation.AddAnnotationTo<FieldDeclarationSyntax>(field);
                    //AttributeSyntax attribute = Syntax.Attribute(NameSyntax);
                    //attribute

                    //variableNumber++;
                }
                else if (node.Kind() == SyntaxKind.LocalDeclarationStatement)
                {
                    variable = new Variable()
                    {
                        Address = nextValueAddress,
                    };
                    nextValueAddress++;

                    LocalDeclarationStatementSyntax local = node as LocalDeclarationStatementSyntax;
                    variable.DataType = GetType(local.Declaration.Type);
                    if (variable.DataType == DataTypeType.Int)
                    {
                        variable.Value = 0;
                    }
                    else if (variable.DataType == DataTypeType.Float)
                    {
                        variable.Value = 0.0;
                    }

                    variable.Name = local.Declaration.Variables[0].Identifier.Value.ToString();
                    //Variables.Add(variable.Name, variable);
                    AllVariables.Add(variable);
                    Variables.Add(variable.Name, variable);
                    //variableNumber++;
                }
                else if (node.Kind() == SyntaxKind.CharacterLiteralExpression
                           || node.Kind() == SyntaxKind.NumericLiteralExpression
                           || node.Kind() == SyntaxKind.StringLiteralExpression)
                {
                    LiteralExpressionSyntax literalExpression = node as LiteralExpressionSyntax;

                    variable = new Variable()
                    {
                        Address = nextValueAddress,
                        Name = literalExpression.Token.Text,
                        Value = literalExpression.Token.Value
                    };
                    nextValueAddress++;

                    if (node.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        string stringValue = literalExpression.Token.Text;
                        stringValue = stringValue.Substring(1, stringValue.Length - 2);

                        //reset these values
                        //variable.Name = "\"" + variable.Name + "\"";
                        //if (stringValue.Contains('\\')) // TODO - check this
                        //{
                        //    stringValue = stringValue.Replace(@"\\", @"\");
                        //}
                        //if (stringValue.Contains('\t'))
                        //{
                        //    stringValue = stringValue.Replace("\t", SyntaxFactory.Tab.ToFullString());
                        //}

                        variable.Value = stringValue;
                        variable.DataType = DataTypeType.String;
                        //variable.Reference = nextReferenceAddress;
                        //IncrementReferenceAddress(variable.DataType);
                    }
                    else if (node.Kind() == SyntaxKind.NumericLiteralExpression)
                    {
                        if (variable.Value is double
                            || variable.Value is float)
                        {
                            variable.DataType = DataTypeType.Float;
                            if (node.Parent.Kind() == SyntaxKind.UnaryMinusExpression) // TODO - check this
                            {
                                //variable.Value = (double)variable.Value * -1;
                                variable.IsNegative = true;
                            }
                        }
                        else
                        {
                            variable.DataType = DataTypeType.Int;
                            if (node.Parent.Kind() == SyntaxKind.UnaryMinusExpression) // TODO - check this
                            {
                                //variable.Value = (Int32)variable.Value * -1;
                                variable.IsNegative = true;
                            }
                        }
                        //variable.Reference = nextReferenceAddress;
                        //IncrementReferenceAddress(variable.DataType);
                    }
                    else if (node.Kind() == SyntaxKind.CharacterLiteralExpression)
                    {
                        variable.DataType = DataTypeType.Character;
                        //variable.Reference = nextReferenceAddress;
                        //IncrementReferenceAddress(variable.DataType);
                    }

                    //if (!Variables.ContainsKey(variable.Name))
                    //{
                    //    Variables.Add(variable.Name, variable);
                    //}
                    AllVariables.Add(variable);
                    Literals.Add(literalExpression, variable);
                    //Literals.Add(literalNumber, variable);
                    //literalNumber++;
                }


            }

        }
        
        private void ExtractOpCodes(CompilationUnitSyntax root)
        {
            //foreach(var blah in root)
            foreach (var member in root.Members)
            {
                CompileNode(member);
            }

            OpCodesSize = 0;
            foreach (OpCode opcode in OpCodes)
            {
                OpCodesSize++;
                if (opcode is OP_PUSH
                    || opcode is OP_GETTOP
                    || opcode is OP_JMP
                    || opcode is OP_JMPF
                    || opcode is JUMPTARGET)
                {
                    OpCodesSize += 2;
                }
            }
        }

        private void CompileNode(SyntaxNode node)
        {
            //if (node is GlobalStatementSyntax)
            //{
                //GlobalStatementSyntax globalStatement = node as GlobalStatementSyntax;

            if (node is GlobalStatementSyntax)
            {
                node = ((GlobalStatementSyntax)node).Statement;
            }

            if (node is BlockSyntax)
            {
                BlockSyntax block = node as BlockSyntax;
                foreach (StatementSyntax statement in block.Statements)
                {
                    CompileNode(statement);
                }
            }
            else if (node is ParenthesizedExpressionSyntax)
            {
                ParenthesizedExpressionSyntax parenthesizedExpression = node as ParenthesizedExpressionSyntax;

                CompileNode(parenthesizedExpression.Expression);

                if (node.Parent.Kind() == SyntaxKind.LogicalNotExpression // TODO - check this still works
                    || node.Parent.Kind() == SyntaxKind.BitwiseNotExpression
                    || node.Parent.Kind() == SyntaxKind.UnaryMinusExpression)
                {
                    OP_NEG opNeg = new OP_NEG();
                    OpCodes.Add(opNeg);
                }
            }
            else if (node is ExpressionStatementSyntax)
            {
                ExpressionStatementSyntax expressionStatement = node as ExpressionStatementSyntax;

                CompileNode(expressionStatement.Expression);
            }
            else if (node is LiteralExpressionSyntax)
            {
                LiteralExpressionSyntax literalExpression = node as LiteralExpressionSyntax;

                OP_PUSH opPush = new OP_PUSH();
                opPush.DataIndex = Literals[literalExpression].Address;
                OpCodes.Add(opPush);

                if (Literals[literalExpression].IsNegative)
                {
                    //push a negative on top
                    OP_NEG opNeg = new OP_NEG();
                    OpCodes.Add(opNeg);
                }

            }
            //else if (node is null)
            //{

            //}
            else if (node is PrefixUnaryExpressionSyntax)
            {
                PrefixUnaryExpressionSyntax prefixUnaryExpression = node as PrefixUnaryExpressionSyntax;
                //a not? negatives are handled later
                if (prefixUnaryExpression.Kind() == SyntaxKind.LogicalNotExpression)
                {
                    CompileNode(prefixUnaryExpression.Operand);

                    //add a not
                    OP_NOT opNot = new OP_NOT();
                    OpCodes.Add(opNot);
                }
                else
                {
                    CompileNode(prefixUnaryExpression.Operand);
                }
            }
            else if (node is IdentifierNameSyntax)
            {
                IdentifierNameSyntax identifierName = node as IdentifierNameSyntax;

                OP_PUSH opPush = new OP_PUSH();
                if (identifierName != null)
                {
                    opPush.DataIndex = Variables[identifierName.ToString()].Address; // TODO check this  .PlainName
            }
                OpCodes.Add(opPush);

                if (node.Parent.Kind() == SyntaxKind.UnaryMinusExpression) // TODO - check this
            {
                    //add a not
                    OP_NEG opNeg = new OP_NEG();
                    OpCodes.Add(opNeg);
                }
            }
            else if (node is ArgumentSyntax)
            {
                ArgumentSyntax argument = node as ArgumentSyntax;

                if (argument.Expression is PrefixUnaryExpressionSyntax)
                {
                    //for a negative parse the operand
                    PrefixUnaryExpressionSyntax prefixUnaryExpression = argument.Expression as PrefixUnaryExpressionSyntax;
                    CompileNode(prefixUnaryExpression.Operand);
                }
                //if (argument.Expression is LiteralExpressionSyntax
                //    || argument.Expression is IdentifierNameSyntax)
                else
                {
                    CompileNode(argument.Expression);
                }
            }
            //a function invocation
            else if (node is InvocationExpressionSyntax)
            {
                InvocationExpressionSyntax invocationExpressionSyntax = node as InvocationExpressionSyntax;

                //first we have an identifier expression
                IdentifierNameSyntax identifierNameSyntax = invocationExpressionSyntax.Expression as IdentifierNameSyntax;

                //then arguments
                foreach (ArgumentSyntax argumentSyntax in invocationExpressionSyntax.ArgumentList.Arguments) //TODO .Reverse()?
                {
                    //Console.WriteLine("    [Literal]");
                    //Console.WriteLine("    " + (LiteralExpressionSyntax)(argumentSyntax.Expression));
                    CompileNode(argumentSyntax);
                }

                if (identifierNameSyntax.ToString() == "Print")
                {
                    OP_PRINT opPrint = new OP_PRINT();
                    OpCodes.Add(opPrint);
                }
                else
                {
                    //push the function code
                    OP_PUSH opPush = new OP_PUSH();

                    if (!FunctionTable.Functions.ContainsKey(identifierNameSyntax.ToString()))
                    {
                        if (!this.AddFunctions && !this.justPatchFunctions)
                        {
                            hasMissingFunctions = true;

                            //if (!this.DirectoryBased)
                            if (true)
                            {
                                Console.WriteLine(string.Format("Missing function {0}", identifierNameSyntax.ToString()));
                            }
                            opPush.DataIndex = 0x0FF0;
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Fetching function {0}", identifierNameSyntax.ToString()));

                            short functionPointer = FunctionTable.FindFunction(this.ScriptFilename.Replace(".hfs", ".bin"), identifierNameSyntax.ToString(), opPush.Address + 1);

                            opPush.DataIndex = functionPointer;
                        }
                    }
                    else
                    {
                        opPush.DataIndex = FunctionTable.Functions[identifierNameSyntax.ToString()];
                    }
                    OpCodes.Add(opPush);

                    //add function call
                    OP_FUNCTION opFunction = new OP_FUNCTION();
                    OpCodes.Add(opFunction);

                    //do a discard
                    if (node.Parent is BinaryExpressionSyntax
                        || node.Parent is AssignmentExpressionSyntax)
                    {
                    }
                    else
                    {
                        OP_DISCARD opDiscard = new OP_DISCARD();
                        OpCodes.Add(opDiscard);
                    }
                }
            }
            else if (node is AssignmentExpressionSyntax)
            {
                AssignmentExpressionSyntax assignmentExpression = node as AssignmentExpressionSyntax;
                CompileNode(assignmentExpression.Right);

                IdentifierNameSyntax identifierName = assignmentExpression.Left as IdentifierNameSyntax;

                //gettop the left
                OP_GETTOP opGettop = new OP_GETTOP();
                opGettop.DataIndex = Variables[identifierName.ToString()].Address;
                OpCodes.Add(opGettop);

                //for an if we need to keep the value
                //or for a double assign
                if (assignmentExpression.Parent.Kind() != SyntaxKind.IfStatement
                    && assignmentExpression.Parent.Kind() != SyntaxKind.SimpleAssignmentExpression)
                {
                    //do a discard
                    OP_DISCARD opDiscard = new OP_DISCARD();
                    OpCodes.Add(opDiscard);
                }
            }
            else if (node is BinaryExpressionSyntax)
            {
                BinaryExpressionSyntax binaryExpression = node as BinaryExpressionSyntax;
                if (binaryExpression.OperatorToken.Kind() == SyntaxKind.EqualsToken)
                {
                    CompileNode(binaryExpression.Right);

                    IdentifierNameSyntax identifierName = binaryExpression.Left as IdentifierNameSyntax;

                    //gettop the left
                    OP_GETTOP opGettop = new OP_GETTOP();
                    opGettop.DataIndex = Variables[identifierName.ToString()].Address;
                    OpCodes.Add(opGettop);

                    //for an if we need to keep the value
                    if (binaryExpression.Parent.Kind() != SyntaxKind.IfStatement)
                    {
                        //do a discard
                        OP_DISCARD opDiscard = new OP_DISCARD();
                        OpCodes.Add(opDiscard);
                    }
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.EqualsEqualsToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_EQUAL opEqual = new OP_EQUAL();
                    OpCodes.Add(opEqual);

                    if (node.Parent.Kind() == SyntaxKind.ExpressionStatement)
                    {
                        //just discard the result
                        OP_DISCARD opDiscard= new OP_DISCARD();
                        OpCodes.Add(opDiscard);
                    }
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.ExclamationEqualsToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_NOT_EQUAL opNotEqual = new OP_NOT_EQUAL();
                    OpCodes.Add(opNotEqual);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.MinusToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_MINUS opEqual = new OP_MINUS();
                    OpCodes.Add(opEqual);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.PlusToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_CONCAT opEqual = new OP_CONCAT();
                    OpCodes.Add(opEqual);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.GreaterThanToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_MORE_THAN opEqual = new OP_MORE_THAN();
                    OpCodes.Add(opEqual);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.LessThanToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_LESS_THAN opEqual = new OP_LESS_THAN();
                    OpCodes.Add(opEqual);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.LessThanEqualsToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_LESS_THAN_OR_EQUAL opLessOrEqual = new OP_LESS_THAN_OR_EQUAL();
                    OpCodes.Add(opLessOrEqual);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.GreaterThanEqualsToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_MORE_THAN_OR_EQUAL opGreatOrEqual = new OP_MORE_THAN_OR_EQUAL();
                    OpCodes.Add(opGreatOrEqual);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.BarBarToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_OR opOr = new OP_OR();
                    OpCodes.Add(opOr);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.AmpersandAmpersandToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_AND opAnd = new OP_AND();
                    OpCodes.Add(opAnd);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.AsteriskToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_MULTIPLY opMultiply = new OP_MULTIPLY();
                    OpCodes.Add(opMultiply);
                }
                else if (binaryExpression.OperatorToken.Kind() == SyntaxKind.SlashToken)
                {
                    CompileNode(binaryExpression.Left);

                    CompileNode(binaryExpression.Right);

                    OP_DIVIDE opDivide = new OP_DIVIDE();
                    OpCodes.Add(opDivide);
                }
            }
            else if (node is WhileStatementSyntax)
            {
                WhileStatementSyntax whileStatement = node as WhileStatementSyntax;

                short whileAddress = OpCode.NextAddress;

                CompileNode(whileStatement.Condition);

                //look at the condition here
                OP_JMPF opJmpF = new OP_JMPF();
                OpCodes.Add(opJmpF);

                CompileNode(whileStatement.Statement);

                //jmp back to the condition
                OP_JMP opJmp = new OP_JMP();
                opJmp.DataIndex = whileAddress;
                OpCodes.Add(opJmp);

                //jump over the jmp back
                opJmpF.DataIndex = (short)(opJmp.Address + 3);
            }
            else if (node is IfStatementSyntax)
            {
                IfStatementSyntax ifStatement = node as IfStatementSyntax;

                //look at the condition here
                CompileNode(ifStatement.Condition);

                short ifAddress = OpCode.NextAddress;
                //do a jump to end if condition is false
                OP_JMPF opJmpF = new OP_JMPF();
                OpCodes.Add(opJmpF);

                CompileNode(ifStatement.Statement);

                //has an else option?
                if (ifStatement.Else != null) //TODO - check this works
                {
                    OP_JMP opJmp = new OP_JMP();
                    OpCodes.Add(opJmp);

                    //save this as the else jump so we can hook it up later
                    this.ElseJmps.Push(opJmp);
                }

                //add in the jump target
                JUMPTARGET jumpTarget = new JUMPTARGET();
                jumpTarget.DataIndex = ifAddress;
                OpCodes.Add(jumpTarget);

                //jump is to here
                opJmpF.DataIndex = jumpTarget.Address;

                CompileNode(ifStatement.Else);
            }
            else if (node is ElseClauseSyntax)
            {
                ElseClauseSyntax elseClause = node as ElseClauseSyntax;

                CompileNode(elseClause.Statement);

                //pop off last one
                OP_JMP opJmp = this.ElseJmps.Pop();

                //add in the jump target
                JUMPTARGET jumpTarget = new JUMPTARGET();
                jumpTarget.DataIndex = opJmp.Address;
                OpCodes.Add(jumpTarget);

                //jump is to here
                opJmp.DataIndex = jumpTarget.Address;
            }

            //}
            //else if (node is BlockSyntax)
            //{
            //    BlockSyntax block = node as BlockSyntax;
            //    foreach (StatementSyntax statement in block.Statements)
            //    {
            //        CompileNode(statement);
            //    }
            //}
        }

        private void PopulateReferences()
        {
            nextReferenceAddress = 276 + (OpCodesSize) + (AllVariables.Count * 5);
            foreach (Variable variable in AllVariables)
            {
                if (variable.DataType == DataTypeType.Character
                    || variable.DataType == DataTypeType.Point
                    || variable.DataType == DataTypeType.String
                    || variable.DataType == DataTypeType.Quaternion)
                {
                    variable.Reference = nextReferenceAddress;
                    if (variable.DataType == DataTypeType.String)
                    {
                        IncrementReferenceAddress(variable.DataType, (string)variable.Value);
                    }
                    else
                    {
                        IncrementReferenceAddress(variable.DataType, "");
                    }
                }
            }
        }

        private void IncrementReferenceAddress(DataTypeType dataTypeType, string stringValue)
        {
            switch (dataTypeType)
            {
                case DataTypeType.Int:
                case DataTypeType.Float:
                    nextReferenceAddress += 4;
                    break;
                case DataTypeType.Point:
                    nextReferenceAddress += 12;
                    break;
                case DataTypeType.Quaternion:
                    nextReferenceAddress += 16;
                    break;
                case DataTypeType.Character:
                    nextReferenceAddress += 4 + 1 + "UnsetCharacter".Length + 1 + 4;
                    break;
                case DataTypeType.String:
                    if (stringValue == null)
                    {
                        nextReferenceAddress += 1;
                    }
                    else
                    {
                        nextReferenceAddress += stringValue.Length + 1;
                    }
                    break;
                default:
                    nextReferenceAddress += 2;
                    break;
            }
        }

        #endregion Parsing

        #region Output

        private void WriteOpCodes(List<byte> binary)
        {
            foreach (var opcode in OpCodes)
            {
                WriteByte(binary, (byte)opcode.Type);

                if (opcode is OP_PUSH)
                {
                    WriteInt16(binary, (Int16)((OP_PUSH)opcode).DataIndex);
                }
                else if (opcode is OP_GETTOP)
                {
                    WriteInt16(binary, (Int16)((OP_GETTOP)opcode).DataIndex);
                }
                else if (opcode is OP_JMP)
                {
                    WriteInt16(binary, (Int16)((OP_JMP)opcode).DataIndex);
                }
                else if (opcode is OP_JMPF)
                {
                    WriteInt16(binary, (Int16)((OP_JMPF)opcode).DataIndex);
                }
                else if (opcode is JUMPTARGET)
                {
                    WriteInt16(binary, (Int16)((JUMPTARGET)opcode).DataIndex);
                }
            }
        }

        private void WriteValues(List<byte> binary)
        {
            foreach (Variable variable in AllVariables)
            {
                binary.Add((byte)variable.DataType);
                if (variable.DataType == DataTypeType.Character
                    || variable.DataType == DataTypeType.String
                    || variable.DataType == DataTypeType.Point
                    || variable.DataType == DataTypeType.Quaternion)
                {
                    WriteInt32(binary, Convert.ToInt16(variable.Reference));
                }
                else if (variable.DataType == DataTypeType.Int)
                {
                    WriteInt32(binary, Convert.ToInt32(variable.Value));
                }
                else if (variable.DataType == DataTypeType.Float)
                {
                    WriteFloat(binary, Convert.ToSingle(Convert.ToDouble(variable.Value)));
                }
            }
        }
   
        private void WriteReferences(List<byte> binary)
        {
            //write the references
            foreach (Variable variable in AllVariables)
            {
                if (variable.DataType == DataTypeType.Character)
                {

                    WriteByte(binary, 0xFF);
                    WriteByte(binary, 0xFF);
                    WriteByte(binary, 0xFF);
                    WriteByte(binary, 0xFF);

                    WriteByte(binary, 0x00);

                    WriteString(binary, "UnsetCharacter");

                    WriteByte(binary, 0x00);

                    WriteByte(binary, 0xFF);
                    WriteByte(binary, 0xFF);
                    WriteByte(binary, 0xFF);
                    WriteByte(binary, 0xFF);
                }
                else if (variable.DataType == DataTypeType.String)
                {
                    WriteString(binary, (string)variable.Value);

                    WriteByte(binary, 0x00);
                }
                else if (variable.DataType == DataTypeType.Point)
                {
                    WriteInt32(binary, 0);
                    WriteInt32(binary, 0);
                    WriteInt32(binary, 0);
                }
                else if (variable.DataType == DataTypeType.Quaternion)
                {
                    WriteInt32(binary, 0);
                    WriteInt32(binary, 0);
                    WriteInt32(binary, 0);
                    WriteInt32(binary, 0);
                }

            }
        }

        public void SaveBinary(string binaryFilename, List<byte> binary)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.BinaryFilename)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this.BinaryFilename));
                }

                File.WriteAllBytes(this.BinaryFilename, binary.ToArray<byte>());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion Output

        #region HelperFunctions

        private DataTypeType GetType(TypeSyntax type)
        {
            switch (type.ToString().ToLower())
            {
                case "character":
                    return DataTypeType.Character;
                case "float":
                    return DataTypeType.Float;
                case "int":
                    return DataTypeType.Int;
                case "point":
                    return DataTypeType.Point;
                case "quaternion":
                    return DataTypeType.Quaternion;
                default:
                    return DataTypeType.String;
            }
        }

        private void WriteHeader(List<byte> binary)
        {
            //Console.WriteLine("Writing name.");
            WriteFilename(binary);

            //Console.WriteLine("Writing placeholders.");

            //NumberOfOpCodesPosition = binary.Count;
            //number of opcodes placeholder
            //WriteInt32(binary, OpCodes.Count);
            WriteInt32(binary, OpCodesSize);

            //startofcodes
            WriteInt32(binary, 276);

            //NumberOfValuesPosition = binary.Count;
            //numberofvalues
            WriteInt32(binary, AllVariables.Count);

            //base address
            WriteInt32(binary, useBaseAddress);

            //StartOfValuesPosition = binary.Count;
            WriteInt32(binary, OpCodesSize + 276);
        }

        private void WriteByte(List<byte> binary, byte value)
        {
            binary.Add(value);
        }

        //private void WriteDouble(List<byte> binary, double value)
        //{
        //    //write the double
        //    foreach (Byte b in BitConverter.GetBytes(value))
        //    {
        //        binary.Add(b);
        //    }
        //}

        private void WriteFloat(List<byte> binary, float value)
        {
            //write the float
            foreach (Byte b in BitConverter.GetBytes(value))
            {
                binary.Add(b);
            }
        }

        private void WriteInt32(List<byte> binary, Int32 value)
        {
            //write the Int32
            foreach (Byte b in BitConverter.GetBytes(value))
            {
                binary.Add(b);
            }
        }

        private void WriteInt16(List<byte> binary, Int16 value)
        {
            //write the Int16
            foreach (Byte b in BitConverter.GetBytes(value))
            {
                binary.Add(b);
            }
        }

        private void WriteString(List<byte> binary, string value)
        {
            if (value == null)
            {
                //binary.Add((byte)0x00); - handled already
                return;
            }

            byte[] bytes = System.Text.Encoding.GetEncoding(1252).GetBytes(value);
            //bytes = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, ansii, bytes);
            foreach (byte b in bytes)
            {
                binary.Add(b);
            }

            ////check for atleast one upper case
            //System.Text.Encoding ascii = ansii;
            //Byte[] encodedBytes = ascii.GetBytes(value);
            //foreach (Byte b in encodedBytes)
            //{
            //    binary.Add(b);
            //}

            //char[] characters = value.ToCharArray();
            //foreach (char c in characters)
            //{
            //    binary.Add((byte)c);
            //}
        }

        private void WriteFilename(List<byte> binary)
        {
            //add the filename
            int nameLength = this.ScriptFilename.Length;

            char[] name = this.ScriptFilename.ToCharArray();
            foreach (char c in name)
            {
                binary.Add((byte)c);
            }
            binary.Add(0x00);
            for (int i = 0; i < 255 - nameLength; i++)
            {
                binary.Add(0xCD);
            }
        }

        #endregion HelperFunctions
    }
}
