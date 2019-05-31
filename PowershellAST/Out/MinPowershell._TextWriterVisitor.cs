// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Management.Automation.Language;

// Reference documentation: https://technet.microsoft.com/en-us/library/hh847744.aspx

namespace Out {

    public partial class MinPowershell {

        private partial class _TextWriterVisitor : BasePassthroughCustomAstVisitor {

            private TextWriter script_;

            public _TextWriterVisitor(TextWriter writer) {
                script_ = writer;
            }

            public void VisitElements<T>(ReadOnlyCollection<T> elements, string separator) where T : Ast {
                if (elements == null) {
                    return;
                }

                if (elements.Count > 0) {
                    T first = elements.First();
                    first.Visit(this);

                    foreach (T t in elements.Skip(1)) {
                        script_.Write(separator);
                        t.Visit(this);
                    }
                }
            }

            public void VisitStatements(ReadOnlyCollection<StatementAst> statements, string separator) {
                if (statements.Count > 0) {
                    var first = statements.First();
                    VisitElement(first);

                    foreach (var statement in statements.Skip(1)) {
                        script_.Write(";");
                        VisitElement(statement);
                    }
                }
            }

            public override object VisitNamedBlock(NamedBlockAst namedBlockAst) {
                VisitElements(namedBlockAst.Traps);
                VisitStatements(namedBlockAst.Statements, ";");
                return namedBlockAst;
            }

            public override object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) {
                script_.Write("function " + functionDefinitionAst.Name + "{");
                VisitElement(functionDefinitionAst.Body);
                script_.Write("}");
                VisitElements(functionDefinitionAst.Parameters);
                return functionDefinitionAst;
            }

            public override object VisitStatementBlock(StatementBlockAst statementBlockAst) {
                VisitStatements(statementBlockAst.Statements, ";");
                VisitElements(statementBlockAst.Traps);
                return statementBlockAst;
            }

            public override object VisitIfStatement(IfStatementAst ifStmtAst) {
                var firstClause = ifStmtAst.Clauses.First();
                script_.Write("if(");
                VisitElement(firstClause.Item1);
                script_.Write("){");
                VisitElement(firstClause.Item2);
                script_.Write("}");

                foreach (Tuple<PipelineBaseAst, StatementBlockAst> clause in ifStmtAst.Clauses.Skip(1)) {
                    script_.Write("elseif(");
                    VisitElement(clause.Item1);
                    script_.Write("){");
                    VisitElement(clause.Item2);
                    script_.Write("}");
                }

                if (ifStmtAst.ElseClause != null) {
                    script_.Write("else{");
                    VisitElement(ifStmtAst.ElseClause);
                    script_.Write("}");
                } else {
                    VisitElement(ifStmtAst.ElseClause);
                }

                return ifStmtAst;
            }

            public override object VisitTrap(TrapStatementAst trapStatementAst) {
                VisitElement(trapStatementAst.TrapType);
                script_.Write("trap{");
                VisitElement(trapStatementAst.Body);
                script_.Write("}");
                return trapStatementAst;
            }

            public override object VisitSwitchStatement(SwitchStatementAst switchStatementAst) {
                if (switchStatementAst.Label != null) {
                    script_.Write(":" + switchStatementAst.Label + " ");
                }

                script_.Write("switch");
                if (switchStatementAst.Flags != SwitchFlags.None) {
                    script_.Write(" -" + switchStatementAst.Flags.ToString().ToLower() + " ");
                }

                script_.Write("(");
                VisitElement(switchStatementAst.Condition);
                script_.Write("){");

                foreach (var clause in switchStatementAst.Clauses) {
                    VisitElement(clause.Item1);
                    script_.Write("{");
                    VisitElement(clause.Item2);
                    script_.Write("}");
                }

                if (switchStatementAst.Default != null) {
                    script_.Write("default{");
                    VisitElement(switchStatementAst.Default);
                    script_.Write("}");
                } else {
                    VisitElement(switchStatementAst.Default);
                }

                script_.Write("}");
                return switchStatementAst;
            }

            public override object VisitDataStatement(DataStatementAst dataStatementAst) {
                script_.Write("data");
                if (dataStatementAst.CommandsAllowed != null) {
                    script_.Write(" -supportedcommand ");
                }
                VisitElements(dataStatementAst.CommandsAllowed, ",");
                script_.Write("{");
                VisitElement(dataStatementAst.Body);
                script_.Write("}");

                return dataStatementAst;
            }

            public override object VisitForEachStatement(ForEachStatementAst forEachStatementAst) {
                if (forEachStatementAst.Label != null) {
                    script_.Write(":" + forEachStatementAst.Label + " ");
                }
                script_.Write("foreach(");
                VisitElement(forEachStatementAst.Variable);
                script_.Write(" in ");
                VisitElement(forEachStatementAst.Condition);
                script_.Write("){");
                VisitElement(forEachStatementAst.Body);
                script_.Write("}");
                return forEachStatementAst;
            }

            public override object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst) {
                if (doWhileStatementAst.Label != null) {
                    script_.Write(":" + doWhileStatementAst.Label + " ");
                }
                script_.Write("do{");
                VisitElement(doWhileStatementAst.Body);
                script_.Write("}while(");
                VisitElement(doWhileStatementAst.Condition);
                script_.Write(")");
                return doWhileStatementAst;
            }

            public override object VisitForStatement(ForStatementAst forStatementAst) {
                if (forStatementAst.Label != null) {
                    script_.Write(":" + forStatementAst.Label + " ");
                }

                script_.Write("for(");
                VisitElement(forStatementAst.Initializer);
                script_.Write(";");
                VisitElement(forStatementAst.Condition);
                script_.Write(";");
                VisitElement(forStatementAst.Iterator);
                script_.Write("){");
                VisitElement(forStatementAst.Body);
                script_.Write("}");
                return forStatementAst;
            }

            public override object VisitWhileStatement(WhileStatementAst whileStatementAst) {
                if (whileStatementAst.Label != null) {
                    script_.Write(":" + whileStatementAst.Label + " ");
                }
                script_.Write("while(");
                VisitElement(whileStatementAst.Condition);
                script_.Write("){");
                VisitElement(whileStatementAst.Body);
                script_.Write("}");
                return whileStatementAst;
            }

            public override object VisitCatchClause(CatchClauseAst catchClauseAst) {
                script_.Write("catch");
                if (catchClauseAst.CatchTypes.Count > 0) {
                    script_.Write(" ");
                    script_.Write(string.Join(",", "[" + catchClauseAst.CatchTypes + "]"));

                }
                script_.Write("{");
                VisitElement(catchClauseAst.Body);
                script_.Write("}");
                return catchClauseAst;
            }

            public override object VisitTryStatement(TryStatementAst tryStatementAst) {
                script_.Write("try{");
                VisitElement(tryStatementAst.Body);
                script_.Write("}");
                VisitElements(tryStatementAst.CatchClauses);
                if (tryStatementAst.Finally != null) {
                    script_.Write("finally{");
                }
                VisitElement(tryStatementAst.Finally);
                if (tryStatementAst.Finally != null) {
                    script_.Write("}");
                }
                return tryStatementAst;
            }

            public override object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst) {
                script_.Write("do{");
                VisitElement(doUntilStatementAst.Body);
                script_.Write("}until(");
                VisitElement(doUntilStatementAst.Condition);
                script_.Write(")");
                return doUntilStatementAst;
            }

            public override object VisitParamBlock(ParamBlockAst paramBlockAst) {
                VisitElements(paramBlockAst.Attributes, ",");
                script_.Write("Param(");
                VisitElements(paramBlockAst.Parameters, ",");
                script_.Write(")");
                return paramBlockAst;
            }

            public override object VisitTypeConstraint(TypeConstraintAst typeConstraintAst) {
                script_.Write("[" + typeConstraintAst.TypeName + "]");
                return typeConstraintAst;
            }

            public override object VisitAttribute(AttributeAst attributeAst) {
                script_.Write("[" + attributeAst.TypeName + "(");
                VisitElements(attributeAst.PositionalArguments, ",");
                VisitElements(attributeAst.NamedArguments, ",");
                script_.Write(")]");
                return attributeAst;
            }

            public override object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst) {
                script_.Write(namedAttributeArgumentAst.ArgumentName);
                if (!namedAttributeArgumentAst.ExpressionOmitted) {
                    script_.Write("=");
                }
                VisitElement(namedAttributeArgumentAst.Argument);
                return namedAttributeArgumentAst;
            }

            public override object VisitParameter(ParameterAst parameterAst) {
                VisitElements(parameterAst.Attributes, "");
                VisitElement(parameterAst.Name);
                if (parameterAst.DefaultValue != null) {
                    script_.Write("=");
                }
                VisitElement(parameterAst.DefaultValue);
                return parameterAst;
            }

            public override object VisitBreakStatement(BreakStatementAst breakStatementAst) {
                script_.Write("break");
                if (breakStatementAst.Label != null) {
                    script_.Write(" ");
                }
                VisitElement(breakStatementAst.Label);
                return breakStatementAst;
            }

            public override object VisitContinueStatement(ContinueStatementAst continueStatementAst) {
                script_.Write("continue");
                if (continueStatementAst.Label != null) {
                    script_.Write(" ");
                }
                VisitElement(continueStatementAst.Label);
                return continueStatementAst;
            }

            public override object VisitReturnStatement(ReturnStatementAst returnStatementAst) {
                // Can't count pipeline elements because here it is abstract. So we have to
                // add extra space in case of pipeline elements
                script_.Write("return ");
                VisitElement(returnStatementAst.Pipeline);
                return returnStatementAst;
            }

            public override object VisitExitStatement(ExitStatementAst exitStatementAst) {
                script_.Write("exit");
                VisitElement(exitStatementAst.Pipeline);
                return exitStatementAst;
            }

            public override object VisitThrowStatement(ThrowStatementAst throwStatementAst) {
                script_.Write("throw ");
                VisitElement(throwStatementAst.Pipeline);
                return throwStatementAst;
            }

            public override object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst) {
                VisitElement(assignmentStatementAst.Left);

                switch (assignmentStatementAst.Operator) {
                    case TokenKind.Equals:
                        script_.Write("=");
                        break;

                    case TokenKind.PlusEquals:
                        script_.Write("+=");
                        break;

                    case TokenKind.MinusEquals:
                        script_.Write("-=");
                        break;

                    case TokenKind.DivideEquals:
                        script_.Write("/=");
                        break;

                    case TokenKind.MultiplyEquals:
                        script_.Write("*=");
                        break;

                    case TokenKind.RemainderEquals:
                        script_.Write("%=");
                        break;

                    default:
                        throw new Exception("Assignment operator not supported");
                }

                VisitElement(assignmentStatementAst.Right);
                return assignmentStatementAst;
            }

            public override object VisitPipeline(PipelineAst pipelineAst) {
                VisitElements(pipelineAst.PipelineElements, "|");
                return pipelineAst;
            }

            public override object VisitCommand(CommandAst commandAst) {
                VisitElements(commandAst.CommandElements, " ");
                VisitElements(commandAst.Redirections, " ");
                return commandAst;
            }

            public override object VisitCommandExpression(CommandExpressionAst commandExpressionAst) {
                VisitElement(commandExpressionAst.Expression);
                VisitElements(commandExpressionAst.Redirections, " ");
                return commandExpressionAst;
            }

            public override object VisitCommandParameter(CommandParameterAst commandParameterAst) {
                script_.Write("-" + commandParameterAst.ParameterName);
                VisitElement(commandParameterAst.Argument);
                return commandParameterAst;
            }

            public override object VisitFileRedirection(FileRedirectionAst fileRedirectionAst) {
                switch (fileRedirectionAst.FromStream) {
                    case RedirectionStream.All:
                        script_.Write("*");
                        break;

                    case RedirectionStream.Output:
                    case RedirectionStream.Error:
                    case RedirectionStream.Warning:
                    case RedirectionStream.Verbose:
                    case RedirectionStream.Debug:
                    case RedirectionStream.Information:
                        int value = (int)fileRedirectionAst.FromStream;
                        script_.Write(value);
                        break;

                    default:
                        throw new Exception("File redirection not supported");

                }
                if (fileRedirectionAst.Append) {
                    script_.Write(">");
                }
                script_.Write(">");
                VisitElement(fileRedirectionAst.Location);
                return fileRedirectionAst;
            }

            public override object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst) {
                switch (mergingRedirectionAst.FromStream) {
                    case RedirectionStream.All:
                        script_.Write("*");
                        break;

                    case RedirectionStream.Output:
                    case RedirectionStream.Error:
                    case RedirectionStream.Warning:
                    case RedirectionStream.Verbose:
                    case RedirectionStream.Debug:
                    case RedirectionStream.Information:
                        int value = (int)mergingRedirectionAst.FromStream;
                        script_.Write(value);
                        break;

                    default:
                        throw new Exception("File redirection not supported");

                }

                script_.Write(">&");

                switch (mergingRedirectionAst.ToStream) {
                    case RedirectionStream.All:
                        script_.Write("*");
                        break;

                    case RedirectionStream.Output:
                    case RedirectionStream.Error:
                    case RedirectionStream.Warning:
                    case RedirectionStream.Verbose:
                    case RedirectionStream.Debug:
                    case RedirectionStream.Information:
                        int value = (int)mergingRedirectionAst.ToStream;
                        script_.Write(value);
                        break;

                    default:
                        throw new Exception("File redirection not supported");

                }

                return mergingRedirectionAst;
            }


            public override object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst) {
                VisitElement(binaryExpressionAst.Left);

                switch (binaryExpressionAst.Operator) {
                    case TokenKind.Dot:
                        script_.Write(".");
                        break;

                    case TokenKind.DotDot:
                        script_.Write("..");
                        break;

                    case TokenKind.Exclaim:
                        script_.Write("!");
                        break;

                    case TokenKind.Ilt:
                        script_.Write("-lt");
                        break;

                    case TokenKind.Ile:
                        script_.Write("-le");
                        break;

                    case TokenKind.Ieq:
                        script_.Write("-eq");
                        break;

                    case TokenKind.Ine:
                        script_.Write("-ne");
                        break;

                    case TokenKind.Ige:
                        script_.Write("-ge");
                        break;

                    case TokenKind.Igt:
                        script_.Write("-gt");
                        break;

                    case TokenKind.Format:
                        script_.Write("-f");
                        break;

                    case TokenKind.And:
                    case TokenKind.Or:
                    case TokenKind.Xor:
                    case TokenKind.Not:
                    case TokenKind.Band:
                    case TokenKind.Bor:
                    case TokenKind.Bnot:
                    case TokenKind.Bxor:
                    case TokenKind.Is:
                    case TokenKind.IsNot:
                        script_.Write("-" + binaryExpressionAst.Operator.ToString().ToLower());
                        break;

                    case TokenKind.Ceq:
                    case TokenKind.Cge:
                    case TokenKind.Cin:
                    case TokenKind.Cle:
                    case TokenKind.Clt:
                    case TokenKind.Cmatch:
                    case TokenKind.Cne:
                    case TokenKind.Cnotcontains:
                    case TokenKind.Cnotin:
                    case TokenKind.Cnotlike:
                    case TokenKind.Cnotmatch:
                    case TokenKind.Ilike:
                    case TokenKind.Inotlike:
                    case TokenKind.Imatch:
                    case TokenKind.Inotmatch:
                        script_.Write("-" + binaryExpressionAst.Operator.ToString().Substring(1).ToLower());
                        break;

                    case TokenKind.Plus:
                        script_.Write("+");
                        break;

                    case TokenKind.Minus:
                        script_.Write("-");
                        break;

                    case TokenKind.Multiply:
                        script_.Write("*");
                        break;

                    case TokenKind.Divide:
                        script_.Write("/");
                        break;

                    case TokenKind.Module:
                        script_.Write("%");
                        break;

                    default:
                        throw new Exception("Binary token not supported");
                }

                VisitElement(binaryExpressionAst.Right);
                return binaryExpressionAst;
            }

            public override object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst) {
                // from https://en.wikipedia.org/wiki/Unary_operation#Windows_PowerShell

                // ++x, --x, +x, -x, .x, -not x, &x
                switch (unaryExpressionAst.TokenKind) {
                    case TokenKind.PlusPlus:
                        script_.Write("++");
                        break;

                    case TokenKind.Plus:
                        script_.Write("+");
                        break;

                    case TokenKind.MinusMinus:
                        script_.Write("--");
                        break;

                    case TokenKind.Minus:
                        script_.Write("-");
                        break;

                    case TokenKind.Exclaim:
                        script_.Write("!");
                        break;

                    case TokenKind.Dot:
                        script_.Write(".");
                        break;

                    case TokenKind.Not:
                        script_.Write("-not ");
                        break;

                    case TokenKind.Ampersand:
                        script_.Write("&");
                        break;

                    case TokenKind.PostfixPlusPlus:
                    case TokenKind.PostfixMinusMinus:
                        break;

                    default:
                        throw new Exception("Unary token not supported");
                }

                VisitElement(unaryExpressionAst.Child);

                // x++, x--
                switch (unaryExpressionAst.TokenKind) {
                    case TokenKind.PostfixPlusPlus:
                        script_.Write("++");
                        break;

                    case TokenKind.PostfixMinusMinus:
                        script_.Write("--");
                        break;
                }

                return unaryExpressionAst;
            }

            public override object VisitConvertExpression(ConvertExpressionAst convertExpressionAst) {
                VisitElement(convertExpressionAst.Type);
                VisitElement(convertExpressionAst.Child);
                return convertExpressionAst;
            }

            public override object VisitTypeExpression(TypeExpressionAst typeExpressionAst) {
                script_.Write("[" + typeExpressionAst.TypeName + "]");
                return typeExpressionAst;
            }

            public override object VisitConstantExpression(ConstantExpressionAst constantExpressionAst) {
                // Note: integer value can be expressed as decimal or hexadecimal
                if (constantExpressionAst.Value is double) {
                    double d = (double)constantExpressionAst.Value;
                    script_.Write(d.ToString(new CultureInfo("en-US")));
                } else {
                    script_.Write(constantExpressionAst.Value);
                }

                return constantExpressionAst;
            }

            public string UnVerbatimString(string src) {
                string dst;
                // from https://msdn.microsoft.com/fr-fr/library/aa691090(v=vs.71).aspx

                // Standard escapes
                // ', ", \, 0, a, b, f, n, r, t, v
                dst = src.Replace("\'", @"`'");
                dst = dst.Replace("\\", @"`\");
                dst = dst.Replace("\"", "`\"");
                dst = dst.Replace("\0", @"`0");
                dst = dst.Replace("\a", @"`a");
                dst = dst.Replace("\b", @"`b");
                dst = dst.Replace("\f", @"`f");
                dst = dst.Replace("\n", @"`n");
                dst = dst.Replace("\r", @"`r");
                dst = dst.Replace("\t", @"`t");
                dst = dst.Replace("\v", @"`v");

                // Unicode escapes
                // u, U, x
                StringBuilder sb = new StringBuilder();
                foreach (char c in dst) {
                    if (c > 127) {
                        // This character is too big for ASCII
                        string encodedValue = "$([char]0x" + ((int)c).ToString("x4") + ")";
                        sb.Append(encodedValue);
                    } else {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }

            public override object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst) {
                var value = UnVerbatimString(stringConstantExpressionAst.Value);
                switch (stringConstantExpressionAst.StringConstantType) {
                    case StringConstantType.DoubleQuotedHereString:
                    case StringConstantType.DoubleQuoted:
                        script_.Write('"');
                        script_.Write(value);
                        script_.Write('"');
                        break;

                    case StringConstantType.SingleQuotedHereString:
                    case StringConstantType.SingleQuoted:
                        script_.Write("'");
                        script_.Write(value);
                        script_.Write("'");
                        break;

                    case StringConstantType.BareWord:
                        script_.Write(value);
                        break;

                    default:
                        throw new Exception("StringConstantType not supported");
                }
                return stringConstantExpressionAst;
            }

            public override object VisitSubExpression(SubExpressionAst subExpressionAst) {
                script_.Write("$(");
                VisitElement(subExpressionAst.SubExpression);
                script_.Write(")");
                return subExpressionAst;
            }

            public override object VisitVariableExpression(VariableExpressionAst variableExpressionAst) {
                script_.Write("$" + variableExpressionAst.VariablePath);
                return variableExpressionAst;
            }

            public override object VisitMemberExpression(MemberExpressionAst memberExpressionAst) {
                VisitElement(memberExpressionAst.Expression);

                if (memberExpressionAst.Static) {
                    script_.Write("::");
                } else {
                    script_.Write(".");
                }

                VisitElement(memberExpressionAst.Member);

                return memberExpressionAst;
            }

            public override object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst) {
                VisitElement(invokeMemberExpressionAst.Expression);

                if (invokeMemberExpressionAst.Static) {
                    script_.Write("::");
                } else {
                    script_.Write(".");
                }

                VisitElement(invokeMemberExpressionAst.Member);

                script_.Write("(");
                VisitElements(invokeMemberExpressionAst.Arguments, ", ");
                script_.Write(")");

                return invokeMemberExpressionAst;
            }

            public override object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst) {
                script_.Write("@(");
                VisitElement(arrayExpressionAst.SubExpression);
                script_.Write(")");
                return arrayExpressionAst;
            }

            public override object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst) {
                VisitElements(arrayLiteralAst.Elements, ", ");
                return arrayLiteralAst;
            }

            public override object VisitHashtable(HashtableAst hashtableAst) {
                script_.Write("@{");

                if (hashtableAst.KeyValuePairs.Count > 0) {
                    var firstPair = hashtableAst.KeyValuePairs.First();
                    VisitElement(firstPair.Item1);
                    script_.Write("=");
                    VisitElement(firstPair.Item2);

                    foreach (var keyValuePair in hashtableAst.KeyValuePairs.Skip(1)) {
                        script_.Write(";");
                        VisitElement(keyValuePair.Item1);
                        script_.Write("=");
                        VisitElement(keyValuePair.Item2);
                    }
                }
                script_.Write("}");
                return hashtableAst;
            }

            public override object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst) {
                script_.Write("{");
                VisitElement(scriptBlockExpressionAst.ScriptBlock);
                script_.Write("}");
                return scriptBlockExpressionAst;
            }

            public override object VisitParenExpression(ParenExpressionAst parenExpressionAst) {
                script_.Write("(");
                VisitElement(parenExpressionAst.Pipeline);
                script_.Write(")");
                return parenExpressionAst;
            }

            public override object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst) {
                var value = UnVerbatimString(expandableStringExpressionAst.Value);
                switch (expandableStringExpressionAst.StringConstantType) {
                    case StringConstantType.DoubleQuotedHereString:
                    case StringConstantType.DoubleQuoted:
                        script_.Write('"');
                        script_.Write(value);
                        script_.Write('"');
                        break;

                    case StringConstantType.SingleQuotedHereString:
                    case StringConstantType.SingleQuoted:
                        script_.Write("'");
                        script_.Write(value);
                        script_.Write("'");
                        break;

                    case StringConstantType.BareWord:
                        script_.Write(value);
                        break;

                    default:
                        throw new Exception("StringConstantType not supported");
                }

                return expandableStringExpressionAst;
            }

            public override object VisitIndexExpression(IndexExpressionAst indexExpressionAst) {
                VisitElement(indexExpressionAst.Target);
                script_.Write("[");
                VisitElement(indexExpressionAst.Index);
                script_.Write("]");
                return indexExpressionAst;
            }
        }
    }
}