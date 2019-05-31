// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Obf {

    public partial class RandomFunctionName {

        private partial class _ListVisitor : BasePassthroughCustomAstVisitor {
            private HashSet<string> hashset_;

            public _ListVisitor() {
                hashset_ = new HashSet<string>();
            }

            public List<string> GetFunctionNameList() {
                return hashset_.ToList();
            }

            public override object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) {
                if (!hashset_.Contains(functionDefinitionAst.Name)) {
                    hashset_.Add(functionDefinitionAst.Name);
                }
                return functionDefinitionAst;
            }
        }
    }
}