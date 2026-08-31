Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Web

Public NotInheritable Class ExportHelper
    Private Sub New()
    End Sub

    Public Shared Sub ExportToCsv(dt As DataTable, fileName As String)
        Dim sb As New StringBuilder()
        Dim headers(dt.Columns.Count - 1) As String
        For i = 0 To dt.Columns.Count - 1
            headers(i) = CsvEscape(dt.Columns(i).ColumnName)
        Next
        sb.AppendLine(String.Join(",", headers))
        For Each row As DataRow In dt.Rows
            Dim cells(dt.Columns.Count - 1) As String
            For i = 0 To dt.Columns.Count - 1
                cells(i) = CsvEscape(If(row(i) Is DBNull.Value, "", row(i).ToString()))
            Next
            sb.AppendLine(String.Join(",", cells))
        Next
        Dim bytes = Encoding.UTF8.GetBytes(sb.ToString())
        HttpContext.Current.Response.Clear()
        HttpContext.Current.Response.ContentType = "text/csv"
        HttpContext.Current.Response.HeaderEncoding = Encoding.UTF8
        HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" & fileName & ".csv")
        HttpContext.Current.Response.BinaryWrite(bytes)
        HttpContext.Current.Response.End()
    End Sub

    Public Shared Sub ExportToCsv(rows As List(Of String()), headers As String(), fileName As String)
        Dim sb As New StringBuilder()
        Dim headerArr(headers.Length - 1) As String
        For i = 0 To headers.Length - 1
            headerArr(i) = CsvEscape(headers(i))
        Next
        sb.AppendLine(String.Join(",", headerArr))
        For Each row In rows
            Dim cells(row.Length - 1) As String
            For i = 0 To row.Length - 1
                cells(i) = CsvEscape(If(row(i) Is Nothing, "", row(i)))
            Next
            sb.AppendLine(String.Join(",", cells))
        Next
        Dim bytes = Encoding.UTF8.GetBytes(sb.ToString())
        HttpContext.Current.Response.Clear()
        HttpContext.Current.Response.ContentType = "text/csv"
        HttpContext.Current.Response.HeaderEncoding = Encoding.UTF8
        HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" & fileName & ".csv")
        HttpContext.Current.Response.BinaryWrite(bytes)
        HttpContext.Current.Response.End()
    End Sub

    Private Shared Function CsvEscape(value As String) As String
        If value.Contains(",") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
            Return """" & value.Replace("""", """""") & """"
        End If
        Return value
    End Function
End Class
