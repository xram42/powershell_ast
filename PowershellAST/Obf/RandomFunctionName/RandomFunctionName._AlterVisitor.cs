// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Obf {

    public partial class RandomFunctionName {

        private partial class _AlterVisitor : BaseCopyCustomAstVisitor {
            private Dictionary<string, string> dict_;

            public _AlterVisitor(int seed, List<string> functionList, string alphabet, int minLength = 1) {
                dict_ = new Dictionary<string, string>();
                var rng = new Random(seed);

                var hashsets = new List<HashSet<string>>();
                hashsets.Add(new HashSet<string>());
                int level = 0;

                var shuffledFuncList = new List<string>(functionList);
                shuffledFuncList.Shuffle(rng);

                foreach (var funcName in shuffledFuncList) {
                    string newFuncName = "";
                    do {
                        StringBuilder sb = new StringBuilder();

                        foreach (var idx in Enumerable.Range(0, minLength + level)) {
                            int num = rng.Next(0, alphabet.Count());
                            sb.Append(alphabet[num]);
                        }

                        newFuncName = sb.ToString();
                    } while (hashsets[level].Contains(newFuncName));

                    dict_.Add(funcName.ToLower(), newFuncName);

                    if (hashsets[level].Count >= (alphabet.Count() * level)) {
                        ++level;
                        hashsets.Add(new HashSet<string>());
                    }
                }
            }

            public override object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) {
                if (!dict_.ContainsKey(functionDefinitionAst.Name.ToLower())) {
                    throw new Exception("Function name was not previously generated");
                }
                var newBody = VisitElement(functionDefinitionAst.Body);

                var newName = dict_[functionDefinitionAst.Name.ToLower()];
                if (functionDefinitionAst.Name.StartsWith("global:")) {
                    newName = "global:" + newName;
                }

                return new FunctionDefinitionAst(functionDefinitionAst.Extent, functionDefinitionAst.IsFilter,
                                                 functionDefinitionAst.IsWorkflow, newName,
                                                 VisitElements(functionDefinitionAst.Parameters), newBody);
            }


            public override object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst) {
                if (dict_.ContainsKey(stringConstantExpressionAst.Value.ToLower())) {
                    var newName = dict_[stringConstantExpressionAst.Value.ToLower()];
                    if (stringConstantExpressionAst.Value.StartsWith("global:")) {
                        newName = "global:" + newName;
                    }

                    return new StringConstantExpressionAst(stringConstantExpressionAst.Extent, newName,
                                                       stringConstantExpressionAst.StringConstantType);
                }
                return new StringConstantExpressionAst(stringConstantExpressionAst.Extent, stringConstantExpressionAst.Value,
                                                       stringConstantExpressionAst.StringConstantType);
            }

        }
    }
}
