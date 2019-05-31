// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Language;

public class BaseCopyCustomAstVisitor : ICustomAstVisitor {

    public virtual object VisitScriptBlock(ScriptBlockAst scriptBlockAst) {
        var newParamBlock = VisitElement(scriptBlockAst.ParamBlock);
        var newBeginBlock = VisitElement(scriptBlockAst.BeginBlock);
        var newProcessBlock = VisitElement(scriptBlockAst.ProcessBlock);
        var newEndBlock = VisitElement(scriptBlockAst.EndBlock);
        var newDynamicParamBlock = VisitElement(scriptBlockAst.DynamicParamBlock);
        return new ScriptBlockAst(scriptBlockAst.Extent, newParamBlock, newBeginBlock, newProcessBlock, newEndBlock,
                                  newDynamicParamBlock);
    }

    public T[] VisitElements<T>(ReadOnlyCollection<T> elements) where T : Ast {
        if (elements == null) {
            return new T[0];
        }

        var newElements = new List<T>();

        foreach (T t in elements) {
            newElements.Add((T)t.Visit(this));
        }
        return newElements.ToArray();
    }

    public T VisitElement<T>(T element) where T : Ast {
        if (element == null)
            return null;
        return (T)element.Visit(this);
    }

    public StatementAst[] VisitStatements(ReadOnlyCollection<StatementAst> statements) {
        var newStatements = new List<StatementAst>();
        foreach (var statement in statements) {
            newStatements.Add(VisitElement(statement));
        }
        return newStatements.ToArray();
    }

    public virtual object VisitNamedBlock(NamedBlockAst namedBlockAst) {
        var newTraps = VisitElements(namedBlockAst.Traps);
        var newStatements = VisitStatements(namedBlockAst.Statements);
        var statementBlock = new StatementBlockAst(namedBlockAst.Extent, newStatements, newTraps);
        return new NamedBlockAst(namedBlockAst.Extent, namedBlockAst.BlockKind, statementBlock, namedBlockAst.Unnamed);
    }

    public virtual object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) {
        var newBody = VisitElement(functionDefinitionAst.Body);
        return new FunctionDefinitionAst(functionDefinitionAst.Extent, functionDefinitionAst.IsFilter,
                                         functionDefinitionAst.IsWorkflow, functionDefinitionAst.Name,
                                         VisitElements(functionDefinitionAst.Parameters), newBody);
    }

    public virtual object VisitStatementBlock(StatementBlockAst statementBlockAst) {
        var newStatements = VisitStatements(statementBlockAst.Statements);
        var newTraps = VisitElements(statementBlockAst.Traps);
        return new StatementBlockAst(statementBlockAst.Extent, newStatements, newTraps);
    }

    public virtual object VisitIfStatement(IfStatementAst ifStmtAst) {
        var newClauses = (from clause in ifStmtAst.Clauses
                          let newClauseTest = VisitElement(clause.Item1)
                          let newStatementBlock = VisitElement(clause.Item2)
                          select new Tuple<PipelineBaseAst, StatementBlockAst>(newClauseTest, newStatementBlock));
        var newElseClause = VisitElement(ifStmtAst.ElseClause);
        return new IfStatementAst(ifStmtAst.Extent, newClauses, newElseClause);
    }

    public virtual object VisitTrap(TrapStatementAst trapStatementAst) {
        return new TrapStatementAst(trapStatementAst.Extent, VisitElement(trapStatementAst.TrapType), VisitElement(trapStatementAst.Body));
    }

    public virtual object VisitSwitchStatement(SwitchStatementAst switchStatementAst) {
        var newCondition = VisitElement(switchStatementAst.Condition);
        var newClauses = (from clause in switchStatementAst.Clauses
                          let newClauseTest = VisitElement(clause.Item1)
                          let newStatementBlock = VisitElement(clause.Item2)
                          select new Tuple<ExpressionAst, StatementBlockAst>(newClauseTest, newStatementBlock));
        var newDefault = VisitElement(switchStatementAst.Default);
        return new SwitchStatementAst(switchStatementAst.Extent, switchStatementAst.Label,
                                      newCondition,
                                      switchStatementAst.Flags, newClauses, newDefault);
    }

    public virtual object VisitDataStatement(DataStatementAst dataStatementAst) {
        var newBody = VisitElement(dataStatementAst.Body);
        var newCommandsAllowed = VisitElements(dataStatementAst.CommandsAllowed);
        return new DataStatementAst(dataStatementAst.Extent, dataStatementAst.Variable, newCommandsAllowed, newBody);
    }

    public virtual object VisitForEachStatement(ForEachStatementAst forEachStatementAst) {
        var newVariable = VisitElement(forEachStatementAst.Variable);
        var newCondition = VisitElement(forEachStatementAst.Condition);
        var newBody = VisitElement(forEachStatementAst.Body);
        return new ForEachStatementAst(forEachStatementAst.Extent, forEachStatementAst.Label, ForEachFlags.None,
                                       newVariable, newCondition, newBody);
    }

    public virtual object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst) {
        var newCondition = VisitElement(doWhileStatementAst.Condition);
        var newBody = VisitElement(doWhileStatementAst.Body);
        return new DoWhileStatementAst(doWhileStatementAst.Extent, doWhileStatementAst.Label, newCondition, newBody);
    }

    public virtual object VisitForStatement(ForStatementAst forStatementAst) {
        var newInitializer = VisitElement(forStatementAst.Initializer);
        var newCondition = VisitElement(forStatementAst.Condition);
        var newIterator = VisitElement(forStatementAst.Iterator);
        var newBody = VisitElement(forStatementAst.Body);
        return new ForStatementAst(forStatementAst.Extent, forStatementAst.Label, newInitializer,
                                   newCondition, newIterator, newBody);
    }

    public virtual object VisitWhileStatement(WhileStatementAst whileStatementAst) {
        var newCondition = VisitElement(whileStatementAst.Condition);
        var newBody = VisitElement(whileStatementAst.Body);
        return new WhileStatementAst(whileStatementAst.Extent, whileStatementAst.Label, newCondition, newBody);
    }

    public virtual object VisitCatchClause(CatchClauseAst catchClauseAst) {
        var newBody = VisitElement(catchClauseAst.Body);
        return new CatchClauseAst(catchClauseAst.Extent, catchClauseAst.CatchTypes, newBody);
    }

    public virtual object VisitTryStatement(TryStatementAst tryStatementAst) {
        var newBody = VisitElement(tryStatementAst.Body);
        var newCatchClauses = VisitElements(tryStatementAst.CatchClauses);
        var newFinally = VisitElement(tryStatementAst.Finally);
        return new TryStatementAst(tryStatementAst.Extent, newBody, newCatchClauses, newFinally);
    }

    public virtual object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst) {
        var newCondition = VisitElement(doUntilStatementAst.Condition);
        var newBody = VisitElement(doUntilStatementAst.Body);
        return new DoUntilStatementAst(doUntilStatementAst.Extent, doUntilStatementAst.Label,
                                       newCondition, newBody);
    }

    public virtual object VisitParamBlock(ParamBlockAst paramBlockAst) {
        var newAttributes = VisitElements(paramBlockAst.Attributes);
        var newParameters = VisitElements(paramBlockAst.Parameters);
        return new ParamBlockAst(paramBlockAst.Extent, newAttributes, newParameters);
    }

    public virtual object VisitErrorStatement(ErrorStatementAst errorStatementAst) {
        return errorStatementAst;
    }

    public virtual object VisitErrorExpression(ErrorExpressionAst errorExpressionAst) {
        return errorExpressionAst;
    }

    public virtual object VisitTypeConstraint(TypeConstraintAst typeConstraintAst) {
        return new TypeConstraintAst(typeConstraintAst.Extent, typeConstraintAst.TypeName);
    }

    public virtual object VisitAttribute(AttributeAst attributeAst) {
        var newPositionalArguments = VisitElements(attributeAst.PositionalArguments);
        var newNamedArguments = VisitElements(attributeAst.NamedArguments);
        return new AttributeAst(attributeAst.Extent, attributeAst.TypeName, newPositionalArguments, newNamedArguments);
    }

    public virtual object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst) {
        var newArgument = VisitElement(namedAttributeArgumentAst.Argument);
        return new NamedAttributeArgumentAst(namedAttributeArgumentAst.Extent,
                                             namedAttributeArgumentAst.ArgumentName, newArgument,
                                             namedAttributeArgumentAst.ExpressionOmitted);
    }

    public virtual object VisitParameter(ParameterAst parameterAst) {
        var newName = VisitElement(parameterAst.Name);
        var newAttributes = VisitElements(parameterAst.Attributes);
        var newDefaultValue = VisitElement(parameterAst.DefaultValue);
        return new ParameterAst(parameterAst.Extent, newName, newAttributes, newDefaultValue);
    }

    public virtual object VisitBreakStatement(BreakStatementAst breakStatementAst) {
        var newLabel = VisitElement(breakStatementAst.Label);
        return new BreakStatementAst(breakStatementAst.Extent, newLabel);
    }

    public virtual object VisitContinueStatement(ContinueStatementAst continueStatementAst) {
        var newLabel = VisitElement(continueStatementAst.Label);
        return new ContinueStatementAst(continueStatementAst.Extent, newLabel);
    }

    public virtual object VisitReturnStatement(ReturnStatementAst returnStatementAst) {
        var newPipeline = VisitElement(returnStatementAst.Pipeline);
        return new ReturnStatementAst(returnStatementAst.Extent, newPipeline);
    }

    public virtual object VisitExitStatement(ExitStatementAst exitStatementAst) {
        var newPipeline = VisitElement(exitStatementAst.Pipeline);
        return new ExitStatementAst(exitStatementAst.Extent, newPipeline);
    }

    public virtual object VisitThrowStatement(ThrowStatementAst throwStatementAst) {
        var newPipeline = VisitElement(throwStatementAst.Pipeline);
        return new ThrowStatementAst(throwStatementAst.Extent, newPipeline);
    }

    public virtual object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst) {
        var newLeft = VisitElement(assignmentStatementAst.Left);
        var newRight = VisitElement(assignmentStatementAst.Right);
        return new AssignmentStatementAst(assignmentStatementAst.Extent, newLeft, assignmentStatementAst.Operator,
                                          newRight, assignmentStatementAst.ErrorPosition);
    }

    public virtual object VisitPipeline(PipelineAst pipelineAst) {
        var newPipeElements = VisitElements(pipelineAst.PipelineElements);
        return new PipelineAst(pipelineAst.Extent, newPipeElements);
    }

    public virtual object VisitCommand(CommandAst commandAst) {
        var newCommandElements = VisitElements(commandAst.CommandElements);
        var newRedirections = VisitElements(commandAst.Redirections);
        return new CommandAst(commandAst.Extent, newCommandElements, commandAst.InvocationOperator, newRedirections);
    }

    public virtual object VisitCommandExpression(CommandExpressionAst commandExpressionAst) {
        var newExpression = VisitElement(commandExpressionAst.Expression);
        var newRedirections = VisitElements(commandExpressionAst.Redirections);
        return new CommandExpressionAst(commandExpressionAst.Extent, newExpression, newRedirections);
    }

    public virtual object VisitCommandParameter(CommandParameterAst commandParameterAst) {
        var newArgument = VisitElement(commandParameterAst.Argument);
        return new CommandParameterAst(commandParameterAst.Extent, commandParameterAst.ParameterName, newArgument,
                                       commandParameterAst.ErrorPosition);
    }

    public virtual object VisitFileRedirection(FileRedirectionAst fileRedirectionAst) {
        var newFile = VisitElement(fileRedirectionAst.Location);
        return new FileRedirectionAst(fileRedirectionAst.Extent, fileRedirectionAst.FromStream, newFile,
                                      fileRedirectionAst.Append);
    }

    public virtual object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst) {
        return new MergingRedirectionAst(mergingRedirectionAst.Extent, mergingRedirectionAst.FromStream,
                                         mergingRedirectionAst.ToStream);
    }

    public virtual object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst) {
        var newLeft = VisitElement(binaryExpressionAst.Left);
        var newRight = VisitElement(binaryExpressionAst.Right);
        return new BinaryExpressionAst(binaryExpressionAst.Extent, newLeft, binaryExpressionAst.Operator, newRight,
                                       binaryExpressionAst.ErrorPosition);
    }

    public virtual object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst) {
        var newChild = VisitElement(unaryExpressionAst.Child);
        return new UnaryExpressionAst(unaryExpressionAst.Extent, unaryExpressionAst.TokenKind, newChild);
    }

    public virtual object VisitConvertExpression(ConvertExpressionAst convertExpressionAst) {
        var newChild = VisitElement(convertExpressionAst.Child);
        var newTypeConstraint = VisitElement(convertExpressionAst.Type);
        return new ConvertExpressionAst(convertExpressionAst.Extent, newTypeConstraint, newChild);
    }

    public virtual object VisitTypeExpression(TypeExpressionAst typeExpressionAst) {
        return new TypeExpressionAst(typeExpressionAst.Extent, typeExpressionAst.TypeName);
    }

    public virtual object VisitConstantExpression(ConstantExpressionAst constantExpressionAst) {
        return new ConstantExpressionAst(constantExpressionAst.Extent, constantExpressionAst.Value);
    }

    public virtual object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst) {
        return new StringConstantExpressionAst(stringConstantExpressionAst.Extent, stringConstantExpressionAst.Value,
                                               stringConstantExpressionAst.StringConstantType);
    }

    public virtual object VisitSubExpression(SubExpressionAst subExpressionAst) {
        var newStatementBlock = VisitElement(subExpressionAst.SubExpression);
        return new SubExpressionAst(subExpressionAst.Extent, newStatementBlock);
    }

    public virtual object VisitUsingExpression(UsingExpressionAst usingExpressionAst) {
        var newUsingExpr = VisitElement(usingExpressionAst.SubExpression);
        return new UsingExpressionAst(usingExpressionAst.Extent, newUsingExpr);
    }

    public virtual object VisitVariableExpression(VariableExpressionAst variableExpressionAst) {
        return new VariableExpressionAst(variableExpressionAst.Extent, variableExpressionAst.VariablePath.UserPath,
                                             variableExpressionAst.Splatted);
    }

    public virtual object VisitMemberExpression(MemberExpressionAst memberExpressionAst) {
        var newExpr = VisitElement(memberExpressionAst.Expression);
        var newMember = VisitElement(memberExpressionAst.Member);
        return new MemberExpressionAst(memberExpressionAst.Extent, newExpr, newMember, memberExpressionAst.Static);
    }

    public virtual object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst) {
        var newExpression = VisitElement(invokeMemberExpressionAst.Expression);
        var newMethod = VisitElement(invokeMemberExpressionAst.Member);
        var newArguments = VisitElements(invokeMemberExpressionAst.Arguments);
        return new InvokeMemberExpressionAst(invokeMemberExpressionAst.Extent, newExpression, newMethod,
                                             newArguments, invokeMemberExpressionAst.Static);
    }

    public virtual object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst) {
        var newStatementBlock = VisitElement(arrayExpressionAst.SubExpression);
        return new ArrayExpressionAst(arrayExpressionAst.Extent, newStatementBlock);
    }

    public virtual object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst) {
        var newArrayElements = VisitElements(arrayLiteralAst.Elements);
        return new ArrayLiteralAst(arrayLiteralAst.Extent, newArrayElements);
    }

    public virtual object VisitHashtable(HashtableAst hashtableAst) {
        var newKeyValuePairs = new List<Tuple<ExpressionAst, StatementAst>>();
        foreach (var keyValuePair in hashtableAst.KeyValuePairs) {
            var newKey = VisitElement(keyValuePair.Item1);
            var newValue = VisitElement(keyValuePair.Item2);
            newKeyValuePairs.Add(Tuple.Create(newKey, newValue));
        }
        return new HashtableAst(hashtableAst.Extent, newKeyValuePairs);
    }

    public virtual object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst) {
        var newScriptBlock = VisitElement(scriptBlockExpressionAst.ScriptBlock);
        return new ScriptBlockExpressionAst(scriptBlockExpressionAst.Extent, newScriptBlock);
    }

    public virtual object VisitParenExpression(ParenExpressionAst parenExpressionAst) {
        var newPipeline = VisitElement(parenExpressionAst.Pipeline);
        return new ParenExpressionAst(parenExpressionAst.Extent, newPipeline);
    }

    public virtual object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst) {
        return new ExpandableStringExpressionAst(expandableStringExpressionAst.Extent,
                                                 expandableStringExpressionAst.Value,
                                                 expandableStringExpressionAst.StringConstantType);
    }

    public virtual object VisitIndexExpression(IndexExpressionAst indexExpressionAst) {
        var newTargetExpression = VisitElement(indexExpressionAst.Target);
        var newIndexExpression = VisitElement(indexExpressionAst.Index);
        return new IndexExpressionAst(indexExpressionAst.Extent, newTargetExpression, newIndexExpression);
    }

    public virtual object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst) {
        var newAttribute = VisitElement(attributedExpressionAst.Attribute);
        var newChild = VisitElement(attributedExpressionAst.Child);
        return new AttributedExpressionAst(attributedExpressionAst.Extent, newAttribute, newChild);
    }

    public virtual object VisitBlockStatement(BlockStatementAst blockStatementAst) {
        var newBody = VisitElement(blockStatementAst.Body);
        return new BlockStatementAst(blockStatementAst.Extent, blockStatementAst.Kind, newBody);
    }
}
