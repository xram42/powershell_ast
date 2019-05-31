// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Management.Automation.Language;

public class BasePassthroughCustomAstVisitor : ICustomAstVisitor {

    public virtual object VisitScriptBlock(ScriptBlockAst scriptBlockAst) {
        VisitElement(scriptBlockAst.ParamBlock);
        VisitElement(scriptBlockAst.BeginBlock);
        VisitElement(scriptBlockAst.ProcessBlock);
        VisitElement(scriptBlockAst.EndBlock);
        VisitElement(scriptBlockAst.DynamicParamBlock);
        return scriptBlockAst;
    }

    public void VisitElements<T>(ReadOnlyCollection<T> elements) where T : Ast {
        if (elements == null) {
            return;
        }

        foreach (T t in elements) {
            t.Visit(this);
        }
    }

    public void VisitElement<T>(T element) where T : Ast {
        if (element == null)
            return;
        element.Visit(this);
    }

    public void VisitStatements(ReadOnlyCollection<StatementAst> statements) {
        foreach (var statement in statements) {
            VisitElement(statement);
        }
    }

    public virtual object VisitNamedBlock(NamedBlockAst namedBlockAst) {
        VisitElements(namedBlockAst.Traps);
        VisitStatements(namedBlockAst.Statements);
        return namedBlockAst;
    }

    public virtual object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) {
        VisitElement(functionDefinitionAst.Body);
        VisitElements(functionDefinitionAst.Parameters);
        return functionDefinitionAst;
    }

    public virtual object VisitStatementBlock(StatementBlockAst statementBlockAst) {
        VisitStatements(statementBlockAst.Statements);
        VisitElements(statementBlockAst.Traps);
        return statementBlockAst;
    }

    public virtual object VisitIfStatement(IfStatementAst ifStmtAst) {
        foreach (var clause in ifStmtAst.Clauses) {
            VisitElement(clause.Item1);
            VisitElement(clause.Item2);
        }
        VisitElement(ifStmtAst.ElseClause);
        return ifStmtAst;
    }

    public virtual object VisitTrap(TrapStatementAst trapStatementAst) {
        VisitElement(trapStatementAst.TrapType);
        VisitElement(trapStatementAst.Body);
        return trapStatementAst;
    }

    public virtual object VisitSwitchStatement(SwitchStatementAst switchStatementAst) {
        VisitElement(switchStatementAst.Condition);
        foreach (var clause in switchStatementAst.Clauses) {
            VisitElement(clause.Item1);
            VisitElement(clause.Item2);
        }
        VisitElement(switchStatementAst.Default);
        return switchStatementAst;
    }

    public virtual object VisitDataStatement(DataStatementAst dataStatementAst) {
        VisitElement(dataStatementAst.Body);
        VisitElements(dataStatementAst.CommandsAllowed);
        return dataStatementAst;
    }

    public virtual object VisitForEachStatement(ForEachStatementAst forEachStatementAst) {
        VisitElement(forEachStatementAst.Variable);
        VisitElement(forEachStatementAst.Condition);
        VisitElement(forEachStatementAst.Body);
        return forEachStatementAst;
    }

    public virtual object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst) {
        VisitElement(doWhileStatementAst.Condition);
        VisitElement(doWhileStatementAst.Body);
        return doWhileStatementAst;
    }

    public virtual object VisitForStatement(ForStatementAst forStatementAst) {
        VisitElement(forStatementAst.Initializer);
        VisitElement(forStatementAst.Condition);
        VisitElement(forStatementAst.Iterator);
        VisitElement(forStatementAst.Body);
        return forStatementAst;
    }

    public virtual object VisitWhileStatement(WhileStatementAst whileStatementAst) {
        VisitElement(whileStatementAst.Condition);
        VisitElement(whileStatementAst.Body);
        return whileStatementAst;
    }

    public virtual object VisitCatchClause(CatchClauseAst catchClauseAst) {
        VisitElement(catchClauseAst.Body);
        return catchClauseAst;
    }

    public virtual object VisitTryStatement(TryStatementAst tryStatementAst) {
        VisitElement(tryStatementAst.Body);
        VisitElements(tryStatementAst.CatchClauses);
        VisitElement(tryStatementAst.Finally);
        return tryStatementAst;
    }

    public virtual object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst) {
        VisitElement(doUntilStatementAst.Condition);
        VisitElement(doUntilStatementAst.Body);
        return doUntilStatementAst;
    }

    public virtual object VisitParamBlock(ParamBlockAst paramBlockAst) {
        VisitElements(paramBlockAst.Attributes);
        VisitElements(paramBlockAst.Parameters);
        return paramBlockAst;
    }

    public virtual object VisitErrorStatement(ErrorStatementAst errorStatementAst) {
        return errorStatementAst;
    }

    public virtual object VisitErrorExpression(ErrorExpressionAst errorExpressionAst) {
        return errorExpressionAst;
    }

    public virtual object VisitTypeConstraint(TypeConstraintAst typeConstraintAst) {
        return typeConstraintAst;
    }

    public virtual object VisitAttribute(AttributeAst attributeAst) {
        VisitElements(attributeAst.PositionalArguments);
        VisitElements(attributeAst.NamedArguments);
        return attributeAst;
    }

    public virtual object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst) {
        VisitElement(namedAttributeArgumentAst.Argument);
        return namedAttributeArgumentAst;
    }

    public virtual object VisitParameter(ParameterAst parameterAst) {
        VisitElement(parameterAst.Name);
        VisitElements(parameterAst.Attributes);
        VisitElement(parameterAst.DefaultValue);
        return parameterAst;
    }

    public virtual object VisitBreakStatement(BreakStatementAst breakStatementAst) {
        VisitElement(breakStatementAst.Label);
        return breakStatementAst;
    }

    public virtual object VisitContinueStatement(ContinueStatementAst continueStatementAst) {
        VisitElement(continueStatementAst.Label);
        return continueStatementAst;
    }

    public virtual object VisitReturnStatement(ReturnStatementAst returnStatementAst) {
        VisitElement(returnStatementAst.Pipeline);
        return returnStatementAst;
    }

    public virtual object VisitExitStatement(ExitStatementAst exitStatementAst) {
        VisitElement(exitStatementAst.Pipeline);
        return exitStatementAst;
    }

    public virtual object VisitThrowStatement(ThrowStatementAst throwStatementAst) {
        VisitElement(throwStatementAst.Pipeline);
        return throwStatementAst;
    }

    public virtual object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst) {
        VisitElement(assignmentStatementAst.Left);
        VisitElement(assignmentStatementAst.Right);
        return assignmentStatementAst;
    }

    public virtual object VisitPipeline(PipelineAst pipelineAst) {
        VisitElements(pipelineAst.PipelineElements);
        return pipelineAst;
    }

    public virtual object VisitCommand(CommandAst commandAst) {
        VisitElements(commandAst.CommandElements);
        VisitElements(commandAst.Redirections);
        return commandAst;
    }

    public virtual object VisitCommandExpression(CommandExpressionAst commandExpressionAst) {
        VisitElement(commandExpressionAst.Expression);
        VisitElements(commandExpressionAst.Redirections);
        return commandExpressionAst;
    }

    public virtual object VisitCommandParameter(CommandParameterAst commandParameterAst) {
        VisitElement(commandParameterAst.Argument);
        return commandParameterAst;
    }

    public virtual object VisitFileRedirection(FileRedirectionAst fileRedirectionAst) {
        VisitElement(fileRedirectionAst.Location);
        return fileRedirectionAst;
    }

    public virtual object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst) {
        return mergingRedirectionAst;
    }

    public virtual object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst) {
        VisitElement(binaryExpressionAst.Left);
        VisitElement(binaryExpressionAst.Right);
        return binaryExpressionAst;
    }

    public virtual object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst) {
        VisitElement(unaryExpressionAst.Child);
        return unaryExpressionAst;
    }

    public virtual object VisitConvertExpression(ConvertExpressionAst convertExpressionAst) {
        VisitElement(convertExpressionAst.Child);
        VisitElement(convertExpressionAst.Type);
        return convertExpressionAst;
    }

    public virtual object VisitTypeExpression(TypeExpressionAst typeExpressionAst) {
        return typeExpressionAst;
    }

    public virtual object VisitConstantExpression(ConstantExpressionAst constantExpressionAst) {
        return constantExpressionAst;
    }

    public virtual object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst) {
        return stringConstantExpressionAst;
    }

    public virtual object VisitSubExpression(SubExpressionAst subExpressionAst) {
        VisitElement(subExpressionAst.SubExpression);
        return subExpressionAst;
    }

    public virtual object VisitUsingExpression(UsingExpressionAst usingExpressionAst) {
        VisitElement(usingExpressionAst.SubExpression);
        return usingExpressionAst;
    }

    public virtual object VisitVariableExpression(VariableExpressionAst variableExpressionAst) {
        return variableExpressionAst;
    }

    public virtual object VisitMemberExpression(MemberExpressionAst memberExpressionAst) {
        VisitElement(memberExpressionAst.Expression);
        VisitElement(memberExpressionAst.Member);
        return memberExpressionAst;
    }

    public virtual object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst) {
        VisitElement(invokeMemberExpressionAst.Expression);
        VisitElement(invokeMemberExpressionAst.Member);
        VisitElements(invokeMemberExpressionAst.Arguments);
        return invokeMemberExpressionAst;
    }

    public virtual object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst) {
        VisitElement(arrayExpressionAst.SubExpression);
        return arrayExpressionAst;
    }

    public virtual object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst) {
        VisitElements(arrayLiteralAst.Elements);
        return arrayLiteralAst;
    }

    public virtual object VisitHashtable(HashtableAst hashtableAst) {
        foreach (var keyValuePair in hashtableAst.KeyValuePairs) {
            VisitElement(keyValuePair.Item1);
            VisitElement(keyValuePair.Item2);
        }
        return hashtableAst;
    }

    public virtual object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst) {
        VisitElement(scriptBlockExpressionAst.ScriptBlock);
        return scriptBlockExpressionAst;
    }

    public virtual object VisitParenExpression(ParenExpressionAst parenExpressionAst) {
        VisitElement(parenExpressionAst.Pipeline);
        return parenExpressionAst;
    }

    public virtual object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst) {
        return expandableStringExpressionAst;
    }

    public virtual object VisitIndexExpression(IndexExpressionAst indexExpressionAst) {
        VisitElement(indexExpressionAst.Target);
        VisitElement(indexExpressionAst.Index);
        return indexExpressionAst;
    }

    public virtual object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst) {
        VisitElement(attributedExpressionAst.Attribute);
        VisitElement(attributedExpressionAst.Child);
        return attributedExpressionAst;
    }

    public virtual object VisitBlockStatement(BlockStatementAst blockStatementAst) {
        VisitElement(blockStatementAst.Body);
        return blockStatementAst;
    }
}
