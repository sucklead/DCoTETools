﻿using banallib;
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

        CSharpSyntaxTree tree = null;

        public DeCompiler()
            {
            FunctionTable.LoadData();

            this.Analyse = new Analyse();

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

        VariableSet variableSet = new VariableSet();

        private void BuildSource()
        {
            this.SourceCode = "";

            //create tree
            CSharpParseOptions cSharpParseOptions = new CSharpParseOptions(LanguageVersion.CSharp1, DocumentationMode.Parse, SourceCodeKind.Script);
            tree = CSharpSyntaxTree.ParseText("", options: cSharpParseOptions) as CSharpSyntaxTree;
            BlockHelper.Root = tree.GetCompilationUnitRoot(); //(CompilationUnitSyntax)tree.GetRoot();

            variableSet.BuildVariables(this.ParsedContent);

            ParseOpCodes();

            //AddCodeToRoot();

            //AddVariablesToRoot();

            this.SourceCode = BlockHelper.Root.GetText().ToString(); //.GetFullText().ToString();
        }


        private void ParseOpCodes()
        {
            //add our base block to root
            BlockSyntax block = BlockHelper.CreateNewBlock("name");
            
            //add block to root
            GlobalStatementSyntax globalStatement = SyntaxFactory.GlobalStatement(block);
            BlockHelper.Root = BlockHelper.Root.AddMembers(globalStatement);

            //set as current
            BlockHelper.SetBlockAsCurrent(block);

            OpCodeProcessor opCodeProcessor = new OpCodeProcessor();
            opCodeProcessor.OpCodes = ParsedContent.OpCodeList;
            opCodeProcessor.VariableSet = this.variableSet;

            opCodeProcessor.ProcessOpCodes();
        }

    }
}