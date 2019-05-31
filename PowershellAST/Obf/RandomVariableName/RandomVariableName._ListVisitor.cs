// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Obf {

    public partial class RandomVariableName {

        private partial class _ListVisitor : BasePassthroughCustomAstVisitor {
            private HashSet<string> hashset_;

            public _ListVisitor() {
                hashset_ = new HashSet<string>();
            }

            public List<string> GetVariableNameList() {
                return hashset_.ToList();
            }

            public override object VisitVariableExpression(VariableExpressionAst variableExpressionAst) {
                if (!variableExpressionAst.IsConstantVariable() &&
                    !hashset_.Contains(variableExpressionAst.VariablePath.UserPath)) {
                    hashset_.Add(variableExpressionAst.VariablePath.UserPath);
                }
                return variableExpressionAst;
            }
        }
    }
}