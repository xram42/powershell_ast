// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Management.Automation.Language;

namespace Obf {

    public partial class RandomFunctionName {

        private string alphabet_;
        private int minLength_;

        public RandomFunctionName(string alphabet, int minLength = 1) {
            alphabet_ = alphabet;
            minLength_ = minLength;
        }

        public ScriptBlockAst Obfuscate(int seed, ScriptBlockAst ast) {
            var firstPass = new _ListVisitor();
            ast.Visit(firstPass);

            var funcList = firstPass.GetFunctionNameList();
            var secondPass = new _AlterVisitor(seed, funcList, alphabet_, minLength_);
            return (ScriptBlockAst)ast.Visit(secondPass);
        }

        private partial class _AlterVisitor { }
        private partial class _ListVisitor { }
    }
}