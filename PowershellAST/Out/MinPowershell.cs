// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using System.Management.Automation.Language;

namespace Out {

    public partial class MinPowershell {

        static public void AstToFile(ScriptBlockAst ast, string path) {
            using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(false))) {
                var visitor = new _TextWriterVisitor(sw);
                ast.Visit(visitor);
            }
        }

        static public void AstToConsole(ScriptBlockAst ast) {
            var writer = new StringWriter();
            var visitor = new _TextWriterVisitor(writer);
            ast.Visit(visitor);
            System.Console.WriteLine(writer);
        }

        private partial class _TextWriterVisitor { }
    }
}