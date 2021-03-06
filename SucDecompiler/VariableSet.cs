﻿using banallib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace SucDecompiler
{
    public class VariableSet
    {
        public Dictionary<short, Variable> Variables { get; set; }
        public List<ValueViewModel> ValueList { get; set; }

        TypeSyntax intType = SyntaxFactory.ParseTypeName("int ");
        TypeSyntax stringType = SyntaxFactory.ParseTypeName("String ");
        TypeSyntax floatType = SyntaxFactory.ParseTypeName("Float ");
        TypeSyntax pointType = SyntaxFactory.ParseTypeName("Point ");
        TypeSyntax characterType = SyntaxFactory.ParseTypeName("Character ");
        TypeSyntax quaternionType = SyntaxFactory.ParseTypeName("Quaternion ");

        SyntaxToken semi = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine("\n")));

        public void BuildVariables(ParsedContent parsedContent)
        {
            Variables = new Dictionary<short, Variable>();
            ValueList = new List<ValueViewModel>();

            this.ValueList = new List<ValueViewModel>();
            foreach (Value value in parsedContent.ValuesList)
            {
                ValueViewModel valueViewModel = new ValueViewModel()
                {
                    Address = value.Address,
                    AddressHex = (short)(value.Address - parsedContent.BaseAddress + parsedContent.StartOfValues),
                    AddressHexBase = (short)(value.Address + parsedContent.StartOfValues),
                    DataType = value.DataType,
                    Reference = value.Reference,
                    IsMe = value.IsMe,
                    IsPlayer = value.IsPlayer
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

                this.ValueList.Add(valueViewModel);


                //build variables list
                if (!Variables.ContainsKey(value.Address))
                {
                    Variable v = new Variable()
                    {
                        Address = value.Address,
                        DataType = value.DataType.ToString(),
                        //Name = "var" + value.DataType.ToString() + value.Address.ToString()
                    };

                    if (value.IsMe
                        || value.IsPlayer)
                    {
                        v.Used = true;
                        v.Static = true;
                    }

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
                            v.Static = false; //points are never static
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

                    if (FixTable.isStaticFix(parsedContent.ScriptName, v.Name))
                    {
                        v.Static = false;
                    }

                    Variables.Add(value.Address, v);
                }
            }

            //work out which variables are static or unused
            for (int i = 0; i < parsedContent.OpCodeList.Count; i++)
            {
                Operation operation = parsedContent.OpCodeList[i];
                if (operation.OpCode == OpCodeType.OP_GETTOP)
                {
                    if (Variables.ContainsKey(operation.DataIndex.Value))
                    {
                        Variables[operation.DataIndex.Value].Static = false;
                        Variables[operation.DataIndex.Value].Used = true;
                    }
                }
                else if (operation.OpCode == OpCodeType.OP_PUSH)
                {
                    if (Variables.ContainsKey(operation.DataIndex.Value))
                    {
                        Variables[operation.DataIndex.Value].Used = true;
                    }
                }
            }
        }

        public BlockSyntax AddVariablesToBlock(BlockSyntax block)
        {
            //block = block.
            int totalVariables = Variables.Count;
            int currentVariable = 0;

            //SyntaxToken insertAfter = block.GetFirstToken();
            SyntaxNode insertNode = null;
            bool insertIsAfter = true;
            int insertSpanStart = 0;

            foreach (var variable in Variables.OrderBy(n => n.Key))
            {

                currentVariable++;
                if (currentVariable < 3)
                {
                    continue;
                }

                if (variable.Value.Static
                    && variable.Value.Used == true)
                {
                    if (variable.Value.Used == false)
                    {
                        Console.WriteLine();
                    }
                    //SyntaxToken firstToken = block.DescendantTokens().Where(n => n.HasAnnotation(new SyntaxAnnotation("StaticVar", variable.Key.ToString()))).Single();
                    //find the static value
                    object staticValue = GetCurrentValue(variable.Key);

                    //SyntaxKind literalKind;
                    //if (staticValue is string)
                    //{
                    //    staticValue = "\"" + (string)staticValue + "\"";
                    //}

                    //foreach (var node in block.DescendantNodes().OfType<LiteralExpressionSyntax>().Where(n => n.GetText().ToString() == staticValue.ToString()))
                    //{
                    //    Console.WriteLine();
                    //    //insertAfter = node.DescendantNodes().Where(n => n.Kind() == SyntaxKind.SemicolonToken).First();
                    //    break;
                    //}

                    //SyntaxNode fromNode = null;
                    //if (insertAfterNode == null)
                    //{
                    //    fromNode = block;
                    //}
                    //else
                    //{
                    //    fromNode = insertAfterNode;
                    //}

                    //foreach (var token in block.DescendantTokens().Where(n => n.ValueText == staticValue.ToString()).Where(o => o.SpanStart > insertSpanStart).First())
                    //{
                    var tokens = block.DescendantTokens().Where(n => n.ValueText == staticValue.ToString() && n.SpanStart > insertSpanStart);
                    if (tokens.Count() > 0)
                    {
                        var token = tokens.First();
                        SyntaxToken insertToken = FindNextSemi(token);
                        if (insertToken.Kind() == SyntaxKind.SemicolonToken)
                        {
                            insertNode = insertToken.Parent;
                            insertIsAfter = true;
                        }
                        else
                        {
                            if (insertToken.Parent.ChildNodes().Count() > 0)
                            {
                                insertNode = insertToken.Parent.ChildNodes().First();
                                insertIsAfter = false;
                            }
                            else
                            {
                                insertNode = insertToken.Parent;
                                insertIsAfter = true;
                            }
                        }
                        insertSpanStart = token.SpanStart;
                        //    break;
                        //}
                    }
                }
                else
                {
                    //SyntaxTokenList newTokens = new SyntaxTokenList();
                    //newTokens = newTokens.Add(insertAfter);
                    LocalDeclarationStatementSyntax localDeclarationStatementSyntax = GetVariableDeclaration(variable.Value);
                    //newTokens.Add(variableToken.DescendantNodesAndTokensAndSelf);
                    //block = block.ReplaceToken(insertAfter, insertAfter);
                    //StatementSyntax statementSyntax = localDeclarationStatementSyntax as StatementSyntax;
                    //var varNode = SyntaxFactory.("\"valid regex\"");
                    //SyntaxAnnotation syntaxAnnotation = new SyntaxAnnotation("Variable", variable.Key.ToString());

                    BlockSyntax addBlock = SyntaxFactory.Block(localDeclarationStatementSyntax); //.WithAdditionalAnnotations(syntaxAnnotation);

                    if (insertNode == null)
                    {
                        block = block.InsertNodesBefore(block.ChildNodes().First(), addBlock.ChildNodes());
                    }
                    else
                    {
                        if (insertIsAfter)
                        {
                            block = block.InsertNodesAfter(insertNode, addBlock.ChildNodes());
                        }
                        else
                        {
                            block = block.InsertNodesBefore(insertNode, addBlock.ChildNodes());
                        }
                    }

                    //insertAfterNode = block.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Where(n => n.HasAnnotation(syntaxAnnotation)).Single();
                    insertNode = block.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Last();
                    insertSpanStart = insertNode.SpanStart;
                    insertIsAfter = true;
                }
            }
            return block;
        }

        private SyntaxToken FindNextSemi(SyntaxToken token)
        {
            SyntaxToken nextToken = token;
            while (nextToken != null
                    && nextToken.Kind() != SyntaxKind.None)
            {
                if (nextToken.IsKind(SyntaxKind.SemicolonToken)
                    || nextToken.IsKind(SyntaxKind.OpenBraceToken))
                {
                    return nextToken;
                }
                nextToken = nextToken.GetNextToken();
            }

            return token;
        }

        public LocalDeclarationStatementSyntax GetVariableDeclaration(Variable variable)
        {
            VariableDeclaratorSyntax declarator = SyntaxFactory.VariableDeclarator(
                  identifier: SyntaxFactory.Identifier(variable.Name)
                );

            TypeSyntax theType = null;
            switch (variable.DataType.ToLower())
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

            return localDeclaration;
        }

        public object GetCurrentValue(short address)
        {
            var q = from v in ValueList
                    where v.Address == address
                    select v;
            ValueViewModel vvm = q.FirstOrDefault();

            StringBuilder sb = new StringBuilder();

            if (vvm.IsMe)
            {
                sb.Append("Me");
                return sb.ToString();
            }
            else if (vvm.IsPlayer)
            {
                sb.Append("Player");
                return sb.ToString();
            }

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

        public string GetSubValue(ValueViewModel vvm, object sv)
        {
            //if (vvm.DataType == DataTypeType.String)
            //{
            //    return string.Format("\"{0}\"", sv);
            //}

            return sv.ToString();
        }


    }
}
