// Copyright (c) 2019 Maxime Raynaud. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using NDesk.Options;

namespace PowershellAST {

    class Program {

        static ScriptBlockAst Obfuscate(string script, int seed) {
            Token[] tokens;
            ParseError[] errors;

            // Initial parsing
            var ast = Parser.ParseInput(script, out tokens, out errors);
            if (errors.Length != 0) {
                Console.WriteLine("[!] Errors: {0}", errors.Length);
                foreach (var error in errors) {
                    Console.WriteLine(error);
                }
                throw new Exception("Parsing exception");
            }

            ScriptBlockAst output;
            string alphabet = "abcdefghijklmnopqrstuvwxyz";

            // Obfuscating variables
            var varObfuscator = new Obf.RandomVariableName(alphabet);
            output = varObfuscator.Obfuscate(seed, ast);

            // Obfuscating function names
            var fnObfuscator = new Obf.RandomFunctionName(alphabet);
            output = fnObfuscator.Obfuscate(seed, output);

            return output;
        }

        static void Main(string[] args) {
            string inputFile = null;
            string outputFile = null;
            bool showHelp = false;
            bool hasSeed = false;
            int seed = 0;

            OptionSet opts = new OptionSet()
            {
                {"o|output=", "Specify output file (default is stdout).", v => outputFile = v },
                {"s|seed=", "Specify the random seed (generated if not specified).", (int v) => { seed = v; hasSeed = true; } },
                {"h|help", "Show this message and exit.", v => showHelp = v != null },
            };

            List<string> extra = opts.Parse(args);

            if (extra.Count == 0 || showHelp) {
                Console.Error.WriteLine("usage: PowershellAST.exe [options] <input file>");
                opts.WriteOptionDescriptions(Console.Error);
                Environment.Exit(1);
            }

            inputFile = extra[0];

            System.IO.StreamReader stream = new System.IO.StreamReader(inputFile);

            string content = stream.ReadToEnd();
            ScriptBlockAst obfuscatedAst;
            if (hasSeed) {
                Console.Error.WriteLine(String.Format("[+] Using seed: {0}", seed));
                obfuscatedAst = Obfuscate(content, seed);
            } else {
                var rng = new CryptoRandom();
                seed = rng.Next();
                Console.Error.WriteLine(String.Format("[+] Generated seed: {0}", seed));
                obfuscatedAst = Obfuscate(content, seed);
            }

            if (!String.IsNullOrEmpty(outputFile)) {
                Out.MinPowershell.AstToFile(obfuscatedAst, outputFile);
            } else {
                Out.MinPowershell.AstToConsole(obfuscatedAst);
            }

            Console.Error.WriteLine("[+] Done.");
        }
    }
}
