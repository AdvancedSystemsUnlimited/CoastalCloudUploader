
' Script is designed to be run with cscript.exe
Function FileExists(FilePath)
  Set fso = CreateObject("Scripting.FileSystemObject")
  If fso.FileExists(FilePath) Then
    FileExists=CBool(1)
  Else
    FileExists=CBool(0)
  End If
End Function
Dim objFSO, objFSOText, objFolder, objFile, LoadFilePath, inputFile, outputFile, arr,sLine, json, inputData, ErrorLog, errLog
Dim strDirectory, OutputSuccessLog
Set args = Wscript.Arguments

For Each arg In args
  Wscript.Echo arg
Next
'Replace ".\Users\Angela Carr\Downloads\" with a local file path for your load file, success and error logs.
strDirectory = ".\temp\"
OutputSuccessLog = year(date) & "-" & month(date) & "-" & day(date) & " Success Log.txt"
errLog = year(date) & "-" & month(date) & "-" & day(date) & " Error Log.txt"

Const ForReading = 1, ForWriting = 2, ForAppending = 8 
Set objFSO = CreateObject("Scripting.FileSystemObject")

' Name of CSV file to read values from.
' Column 0 - Controlling Value
' Column 1 - Child Value
LoadFilePath = strDirectory & "\Prod_Final_Attachments_Wave-05_01.txt"

Wscript.Echo  strDirectory & OutputSuccessLog
Set outputFile = objFSO.OpenTextFile(strDirectory & OutputSuccessLog, ForWriting, True)
Set outputErrorLog = objFSO.OpenTextFile(strDirectory & errLog, ForWriting, True)
OutputErrorLog.WriteLine LoadFilePath
Set inputFile = objFSO.OpenTextFile(LoadFilePath, ForReading)

OutputErrorLog.WriteLine "************************* START TEST " & now & " *********************************"
Dim Fso
      Set Fso = WScript.CreateObject("Scripting.FileSystemObject")
  Do While inputFile.AtEndOfStream <> True
      SLine = inputFile.ReadLine
      arr = Split(sLine, "|")
      FNameSplit = Split(arr(3),".")
      OutputErrorLog.WriteLine "38 ZD TEMP NAME --- .\temp\Attachments\" & ARR(5)
      If FileExists(".\temp\Attachments\" & ARR(3)) Then
         'Read file and convert to JSON  
                 OutputErrorLog.WriteLine "41 FILE EXISTS --- .\temp\Attachments\" & ARR(3)
                 i = 1
                 x = 20
                 StopLoop = False
                 Do While i < x and StopLoop <> True
                                If FileExists(".\temp\Attachments\" & FNameSplit(0) & "-" & i & "." & FNameSplit(1)) Then
                                 i = i + 1
                                Else
                                 StopLoop = TRUE
                                 OutputErrorLog.WriteLine " 49 NEW NAME .\temp\Attachments\" & FNameSplit(0) & "-" & i & "." & FNameSplit(1)
                                 Fso.MoveFile ".\temp\Attachments\" & ARR(5), ".\temp\Attachments\" & FNameSplit(0) & "-" & i & "." &  FNameSplit(1)
                                 outputFile.WriteLine Arr(0) & "|" & Arr(1) & "|" & Arr(2) & "|.\temp\Attachments\" & FNameSplit(0) & "-" & i & "." &  FNameSplit(1) & "|" & Arr(4) & "|" & Arr(5) 
                                End if                         
                 Loop
      Else
          OutputErrorLog.WriteLine "54 NEW NAME Line .\temp\Attachments\" & ARR(3)
          Fso.MoveFile ".\temp\Attachments\" & ARR(5), ".\temp\Attachments\" & ARR(3)
          outputFile.WriteLine  Arr(0) & "|" & Arr(1) & "|" & Arr(2) & "|.\temp\Attachments\" & ARR(3) & "|" & Arr(4) & "|" & Arr(5) 
      End If
  Loop
  OutputErrorLog.WriteLine "Variable name: inputData Value:" & inputData
  ' Output error message (File: outputFile.WriteLine  Screen: stderr)
