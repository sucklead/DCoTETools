using banallib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        //private List<ValueViewModel> _valueList;
        //public List<ValueViewModel> ValueList
        //{
        //    get
        //    {
        //        return _valueList;
        //    }
        //    set
        //    {
        //        if (_valueList != value)
        //        {
        //            _valueList = value;
        //            //RaisePropertyChanged(() => this.ValueList);
        //        }
        //    }
        //}
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

        public DeCompiler()
            {
            FunctionTable.LoadData();

            this.Analyse = new Analyse();
            //this.Analyse.NewBase = 605;
            //this.ValueList = new List<ValueViewModel>();


        }


        public string SourceDirectory { get; internal set; }
        public string TargetDirectory { get; internal set; }

        internal void DeCompileFile(string filename)
        {
            string sourceFilename = Path.Combine(SourceDirectory, filename);
            string targetFilename = Path.Combine(TargetDirectory, filename.Replace(".bin", ".hfs"));
            string targetDir = Path.GetDirectoryName(targetFilename);

            variables.Clear();

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
            File.WriteAllText(targetFilename, this.SourceCode);
        }

        private void BuildSource()
        {
            this.SourceCode = "";

            //create tree
            tree = CSharpSyntaxTree.ParseText("") as CSharpSyntaxTree;
            root = tree.GetCompilationUnitRoot(); //(CompilationUnitSyntax)tree.GetRoot();

            BuildVariables();
        
            AddVariablesToRoot();

            //ParseOpCodes();

            //AddCodeToRoot();

            this.SourceCode = root.GetText().ToString(); //.GetFullText().ToString();
        }

        private void BuildVariables()
        {
            //List<ValueViewModel> valueList = new List<ValueViewModel>();
            foreach (Value value in this.ParsedContent.ValuesList)
            {
                //ValueViewModel valueViewModel = new ValueViewModel()
                //{
                //    Address = value.Address,
                //    AddressHex = (short)(value.Address - ParsedContent.BaseAddress + ParsedContent.StartOfValues),
                //    AddressHexBase = (short)(value.Address + ParsedContent.StartOfValues),
                //    DataType = value.DataType,
                //    Reference = value.Reference
                //};
                //if (value.SubValues.Count > 0)
                //{
                //    if (valueViewModel.DataType == DataTypeType.Float)
                //    {
                //        valueViewModel.SubValue1 = BitConverter.ToSingle(BitConverter.GetBytes((Int32)(value.SubValues[0])), 0);
                //    }
                //    else
                //    {
                //        valueViewModel.SubValue1 = value.SubValues[0];
                //    }
                //}
                //if (value.SubValues.Count > 1)
                //{
                //    valueViewModel.SubValue2 = value.SubValues[1];
                //}
                //if (value.SubValues.Count > 2)
                //{
                //    valueViewModel.SubValue3 = value.SubValues[2];
                //}
                //if (value.SubValues.Count > 3)
                //{
                //    valueViewModel.SubValue4 = value.SubValues[3];
                //}
                //if (value.SubValues.Count > 4)
                //{
                //    valueViewModel.SubValue5 = value.SubValues[4];
                //}

                //valueList.Add(valueViewModel);


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
                        //case ("Quartnian"):
                        //    v.Name = "qVar" + v.Address.ToString();
                        //    break;
                        case ("Quaternion"):
                            v.Name = "qVar" + v.Address.ToString();
                            break;
                    }


                    variables.Add(value.Address, v);
                }


            }
            //this.ValueList = valueList;

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

        private void AddVariablesToRoot()
        {
            TypeSyntax intType = SyntaxFactory.ParseTypeName("int ");
            TypeSyntax stringType = SyntaxFactory.ParseTypeName("String ");
            TypeSyntax floatType = SyntaxFactory.ParseTypeName("float ");
            TypeSyntax pointType = SyntaxFactory.ParseTypeName("Point ");
            TypeSyntax characterType = SyntaxFactory.ParseTypeName("Character ");
            TypeSyntax quartonianType = SyntaxFactory.ParseTypeName("Quartonian ");
            SyntaxToken semi = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n")));

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

                //FieldDeclarationSyntax newField = SyntaxFactory.FieldDeclaration(
                //    //attributes: null,
                //    modifiers: new SyntaxTokenList(),
                //    declaration: SyntaxFactory.VariableDeclaration(type: theType, variables: SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>().Add(declarator)),
                //    semicolonToken: semi);
                FieldDeclarationSyntax newField = SyntaxFactory.FieldDeclaration(
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(),
                    SyntaxFactory.VariableDeclaration(type: theType, variables: SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>().Add(declarator)),
                    semi);

                root = root.WithMembers(root.Members.Add(newField));
            }
        }

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
    }
}