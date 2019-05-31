# Research project on Powershell AST obfuscation

### Requirements

Requirements are in the form of NuGet packages :
- NDesk.Options (0.2.1)
- System.Management.Automation 5 reference assemblies (1.1.0)

### Example usage
 
```
C:\>PowershellAST.exe -h
usage: PowershellAST.exe [options] <input file>
  -o, --output=VALUE         Specify output file (default is stdout).
  -s, --seed=VALUE           Specify the random seed (generated if not
                               specified).
  -h, --help                 Show this message and exit.

C:\>PowershellAST.exe sample.ps1 -o obfuscated_sample.ps1
Generated seed: 1040295388
```
