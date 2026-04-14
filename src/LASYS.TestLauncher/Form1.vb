
Public Class Form1
    Private Sub btnAutoLogin_Click(sender As Object, e As EventArgs) Handles btnAutoLogin.Click

        Dim appPath As String = "C:\Project\TERUMO\LASYS\src\LASYS.DesktopApp\bin\x86\Debug\net8.0-windows\LASYS.DesktopApp.exe"

        Dim username As String = txtUsername.Text

        Dim psi As New ProcessStartInfo()
        psi.FileName = appPath
        psi.Arguments = "--username=" & username
        psi.WorkingDirectory = IO.Path.GetDirectoryName(appPath)

        Process.Start(psi)
        Application.Exit()
    End Sub
End Class
