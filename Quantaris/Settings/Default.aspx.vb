Imports System
Imports System.IO
Imports System.Web

Public Class Settings_Default
    Inherits BasePage

    Private Shared ReadOnly KeyFilePath As String = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "openai_key.txt")

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ShowStatus()
        End If
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim key = txtApiKey.Text.Trim()
        If String.IsNullOrEmpty(key) Then
            lblStatus.ForeColor = Drawing.Color.Red
            lblStatus.Text = "Please enter an API key."
            Return
        End If

        Try
            Dim dir = Path.GetDirectoryName(KeyFilePath)
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
            File.WriteAllText(KeyFilePath, key)
            lblStatus.ForeColor = Drawing.Color.FromArgb(0, 214, 143)
            lblStatus.Text = "API key saved successfully."
            txtApiKey.Text = ""
        Catch ex As Exception
            lblStatus.ForeColor = Drawing.Color.Red
            lblStatus.Text = "Error saving key: " & ex.Message
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        Try
            If File.Exists(KeyFilePath) Then File.Delete(KeyFilePath)
            lblStatus.ForeColor = Drawing.Color.FromArgb(0, 214, 143)
            lblStatus.Text = "API key removed."
        Catch ex As Exception
            lblStatus.ForeColor = Drawing.Color.Red
            lblStatus.Text = "Error removing key: " & ex.Message
        End Try
    End Sub

    Private Sub ShowStatus()
        Dim configured = Not String.IsNullOrEmpty(GetApiKey())
        If configured Then
            lblStatus.ForeColor = Drawing.Color.FromArgb(0, 214, 143)
            lblStatus.Text = "<i class='bi bi-check-circle-fill'></i> API key is configured. AI rephrasing is enabled."
        Else
            lblStatus.ForeColor = Drawing.Color.FromArgb(139, 139, 163)
            lblStatus.Text = "<i class='bi bi-info-circle'></i> No API key set. Only local grammar rules are active."
        End If
    End Sub

    Public Shared Function GetApiKey() As String
        Try
            If File.Exists(KeyFilePath) Then
                Dim key = File.ReadAllText(KeyFilePath).Trim()
                If Not String.IsNullOrEmpty(key) Then Return key
            End If
        Catch
        End Try
        Return ""
    End Function
End Class
