# USING
using namespace System;
using namespace System.Text;
using namespace System.Diagnostics;
using namespace System.Linq;
using namespace System.Collections.Generic;

#TRAP
function TrapTest {
   trap {"Error found: $_"}
   trapunknownsymbol
}

# SWITCH
$a = 5
switch ($a) { 
        1 {"This is a test."} 
        2 {"This is a test."} 
        default {"This is a test."}
}

switch -wildcard ($a) { 
        "a*" {"This is a test."}
        "?a" {"This is a test."}
}

# DO UNTIL
Do { $val++ 
     Write-Host $val
} until($val -ne 10) 

# DO WHILE
Do { $val++ 
     Write-Host $val
} while($val -ne 10) 

# ERROR STATEMENT
Stop-Process -Name invalidprocess -ErrorVariable ProcessError;
$ProcessError;
Stop-Process -Name invalidprocess2 -ErrorVariable +ProcessError;
if ($ProcessError) {
    ######## Take administrative action on error state
}

# ERROR EXPR
($Error[0])

# CONTINUE
while ($i -lt 10) {
  $i +=1 
  if ($i -eq 5) {continue}
   Write-Host $i
}

# REDIRECTION
Write-Output "This" >test.txt
Write-Output "is" >>test.txt
Write-Output "a" *>> test.txt

# MERGE
Write-Output "test." 3>&1 2>&1 1>test.txt

# FORMAT
Get-ChildItem c:\ | ForEach-Object {'File {0} Created {1}' -f $_.fullname,$_.creationtime}

# RANGE
10..20
5..25

# GROUPING
(dir).FullName

# FOREACH OBJECT
get-childitem C:\ | foreach-object -process { $_.length / 1024 }

# SUBEXPRESSION
$(Get-WMIObject win32_Directory)

# EXIT
exit

# TODO: DATA
# DATA {    
#          "test"
# }

# DATA -supportedCommand Format-XML {    
#          "test"
# }

# $TextMsgs = DATA {
#          "test"
# }
