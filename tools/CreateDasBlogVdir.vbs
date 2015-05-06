'7-27-05    Modified by Mark Michaelis (mark@michaelis.net)
'modified by Scott Hanselman scott@hanselman.com 

If WScript.Arguments.Count > 0 Then
    vDirName = WScript.Arguments(0)
Else
    vDirName = InputBox("Enter the desired virtual directory name", "Virtual Directory", "DasBlog2", False)
End If

Set shell = Wscript.CreateObject( "WScript.Shell" )

' Get the name of the current directory
Set fso = WScript.CreateObject( "Scripting.FileSystemObject" )

' Here is the name of the PHYSICAL folder just below the current directory

' *NOTE*: This is different if you are building the source yourself, or if you're just using the WebFiles

If (fso.FolderExists(".\dasblogce") = true) Then
    'USING THE WEB FILES
    vDirPath = fso.GetFolder( ".\dasblogce" ).Path
Else
    'USING THE SOURCE:
    vDirPath = fso.GetFolder( "..\source\newtelligence.DasBlog.Web" ).Path
End If

' Does this IIS application already exist in the metabase?
On Error Resume Next
Set objIIS = GetObject( "IIS://localhost/W3SVC/1/Root/" & vDirName )
If objIIS is nothing Then
  MsgBox ("Cannot create IIS Management Objects - have you enabled the IIS6 Management Compatibility in Control Panel?")
End If
If Err.Number = 0 Then
    result = shell.Popup( "A virtual directory named " & vDirName & " already exists. " & vbCrLf & vbCrLf & "Would you like it re-mapped for this sample?", 0 ,"Remap Virtual Directory?", 4 + 32 )' 4 = YesNo & 32 = Question
    If result = 6 Then ' 6 = Yes
        DeleteVirtualDirectory vDirName 
    Else
        WScript.Quit
    End If
End If

'Using IIS Administration object , turn on script/execute permissions and define the virtual directory as an 'in-process application. 
Set objIIS  = GetObject( "IIS://localhost/W3SVC/1/Root" )
If objIIS is nothing Then
  MsgBox ("Cannot create IIS Management Objects - have you enabled the IIS6 Management Compatibility in Control Panel?")
End If
Set vDirObj = objIIS.Create( "IISWebVirtualDir", vDirName )

vDirObj.Path                  = vDirPath
vDirObj.AuthNTLM              = True
vDirObj.AccessRead            = True
vDirObj.AccessWrite           = True 
vDirObj.AccessScript          = True
vDirObj.AccessExecute         = True
vDirObj.AuthAnonymous         = True
'vDirObj.AnonymousUserName     = owner
vDirObj.AnonymousPasswordSync = True
vDirObj.EnableDefaultDoc = True
vDirObj.DefaultDoc = "default.aspx"
vDirObj.AppCreate True
vDirObj.SetInfo 

If Err.Number > 0 Then
    shell.Popup Err.Description, 0, "Error", 16 ' 16 = Stop
    WScript.Quit
Else
    shell.Popup "Virtual directory created." & vbCrLf & "setting folder permissions ..." , 1, "Status", 64 ' 64 = Information
End If

' Get the name of the account for the anonymous user in IIS
owner = vDirObj.AnonymousUserName

' Change necessary folder permissions using CACLS.exe
aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & """"
aclCmd = aclCmd & " /E /G " & owner & ":C"
rtc = shell.Run( aclCmd , 0, True )

aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & """"
aclCmd = aclCmd & " /E /G ""VS Developers"":C"
rtc = shell.Run( aclCmd , 0, True )


aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & "/SiteConfig"""
aclCmd = aclCmd & " /E /T /G ""NETWORK SERVICE"":C"
rtc = shell.Run( aclCmd , 0, True )

aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & "/Content"""
aclCmd = aclCmd & " /E /T /G ""NETWORK SERVICE"":C"
rtc = shell.Run( aclCmd , 0, True )

aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & "/Logs"""
aclCmd = aclCmd & " /E /G ""NETWORK SERVICE"":C"
rtc = shell.Run( aclCmd , 0, True )



aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & "/SiteConfig"""
aclCmd = aclCmd & " /E /T /G ""ASPNET"":C"
rtc = shell.Run( aclCmd , 0, True )

aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & "/Content"""
aclCmd = aclCmd & " /E /T /G ""ASPNET"":C"
rtc = shell.Run( aclCmd , 0, True )

aclCmd = "cmd /c echo y| CACLS "
aclCmd = aclCmd & """" & vDirPath & "/Logs"""
aclCmd = aclCmd & " /E /G ""ASPNET"":C"
rtc = shell.Run( aclCmd , 0, True )


If Err.Number > 0 Then
    shell.Popup Err.Description, 0, "Error", 16 ' 16 = Stop
    WScript.Quit
Else
    res = vDirName & " has been created at" & vbCrLf & vDirPath
    shell.Popup res, 0, "All done", 64 ' 64 = Information
End If

Sub DeleteVirtualDirectory( NameOfVdir )

    Set iis = GetObject("IIS://localhost/W3SVC/1/Root")
    iis.Delete "IISWebVirtualDir", vDirName
    
    If Err.Number = 0 Then
        shell.Popup "Virtual directory deleted sucessfully", 1, "Status", 64 ' 64 = Information
    Else
        shell.Popup Err.Description, 0, "Error", 16 ' 16 = Stop
    End If

End Sub
