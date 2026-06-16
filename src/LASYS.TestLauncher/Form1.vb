
Imports System.IO

Public Class Form1
    Private Sub btnAutoLogin_Click(sender As Object, e As EventArgs) Handles btnAutoLogin.Click

        'Dim appPath As String = "C:\Project\TERUMO\LASYS\src\LASYS.DesktopApp\bin\x86\Debug\net8.0-windows\LASYS.DesktopApp.exe"

        Try
            Dim installDir As String = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InnovaThinkCorporation"), "current")
            Dim appPath As String = Path.Combine(installDir, "LASYS.DesktopApp.exe")
            Dim username As String = txtUsername.Text

            If Not IO.File.Exists(appPath) Then
                MessageBox.Show("LASYS Desktop App was not found." & vbCrLf & appPath,
                                "Application Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error)
                Exit Sub
            End If

            ' Check if already running
            Dim processName As String = Path.GetFileNameWithoutExtension(appPath)

            If Process.GetProcessesByName(processName).Length > 0 Then
                MessageBox.Show("LASYS Desktop App is already running.",
                                "Information",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information)
                Exit Sub
            End If

            Dim psi As New ProcessStartInfo()
            psi.FileName = appPath
            psi.Arguments = "--username=" & username
            psi.WorkingDirectory = IO.Path.GetDirectoryName(appPath)

            Process.Start(psi)
        Catch ex As Exception
            MessageBox.Show("Unable to start LASYS Desktop App." & vbCrLf & vbCrLf & ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
