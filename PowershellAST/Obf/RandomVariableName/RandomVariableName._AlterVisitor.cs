// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Obf {

    public partial class RandomVariableName {

        private partial class _AlterVisitor : BaseCopyCustomAstVisitor {
            private Dictionary<string, string> dict_;

            public _AlterVisitor(int seed, List<string> variableList, string alphabet, int minLength = 1) {
                dict_ = new Dictionary<string, string>();
                var rng = new Random(seed);

                var hashsets = new List<HashSet<string>>();
                hashsets.Add(new HashSet<string>());
                int level = 0;

                var shuffledVarList = new List<string>(variableList);
                shuffledVarList.Shuffle(rng);

                foreach (var varName in shuffledVarList) {
                    string newVarName = "";
                    do {
                        StringBuilder sb = new StringBuilder();

                        foreach (var idx in Enumerable.Range(0, minLength + level)) {
                            int num = rng.Next(0, alphabet.Count());
                            sb.Append(alphabet[num]);
                        }

                        newVarName = sb.ToString();
                    } while (hashsets[level].Contains(newVarName));

                    dict_.Add(varName.ToLower(), newVarName);

                    if (hashsets[level].Count >= (alphabet.Count() * level)) {
                        ++level;
                        hashsets.Add(new HashSet<string>());
                    }
                }

                // Blacklist
                // TODO: make a default static blacklist and make it upgradable
                List<string> blacklist = new List<string>(new string[] {
                    "args",
                    "true",
                    "false",
                    "ErrorActionPreference"
                });
                foreach (var blName in blacklist) {
                    dict_[blName.ToLower()] = blName;
                }
            }

            public override object VisitVariableExpression(VariableExpressionAst variableExpressionAst) {
                if (!dict_.ContainsKey(variableExpressionAst.VariablePath.UserPath.ToLower())) {
                    throw new Exception("Variable name was not previously generated");
                }

                var newVariableName = dict_[variableExpressionAst.VariablePath.UserPath.ToLower()];

                // Global variables
                if (variableExpressionAst.VariablePath.UserPath.StartsWith("global:")) {
                    // Prefix with qualifier
                    newVariableName = "global:" + newVariableName;
                }

                return new VariableExpressionAst(variableExpressionAst.Extent, newVariableName,
                                                     variableExpressionAst.Splatted);
            }
        }
    }
}
