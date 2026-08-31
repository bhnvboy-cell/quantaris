Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Text

Public Partial Class Tools_HoshinKanri
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlHK.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlHK.Visible = False
                pnlNoProject.Visible = True
            End If
        End If
    End Sub

    Private Sub LoadProjects()
        Using cmd As New MySqlCommand(
            "SELECT Id, ProjectCode, ProjectName FROM QuantarisProjects WHERE TenantId = @TenantId ORDER BY ProjectName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            ddlProject.DataSource = dt
            ddlProject.DataTextField = "ProjectName"
            ddlProject.DataValueField = "Id"
            ddlProject.DataBind()
            ddlProject.Items.Insert(0, New ListItem("-- Select Project --", ""))
        End Using
    End Sub

    Private Function GetSelectedProjectId() As Integer
        Dim pid As Integer = 0
        If ddlProject.SelectedValue <> "" Then Integer.TryParse(ddlProject.SelectedValue, pid)
        Return pid
    End Function

    Private Sub LoadHistory()
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Exit Sub
        Using cmd As New MySqlCommand(
            "SELECT Id, ModelName, FiscalYear, CreatedOn FROM HoshinKanriModels " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataSource = dt
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlHK.Visible = True
            pnlNoProject.Visible = False
            pnlMatrix.Visible = False
            pnlNoMatrix.Visible = True
            LoadHistory()
        Else
            pnlHK.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then LoadHistory()
    End Sub

    Protected Sub btnAdd_Click(sender As Object, e As EventArgs)
        Dim items = GetSessionItems()
        Dim level = ddlLevel.SelectedValue
        Dim name = txtItemName.Text.Trim()
        If name = "" Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter item name.</div>" : Return
        End If
        Dim owner = txtOwner.Text.Trim()
        Dim targetDate = txtTargetDate.Text.Trim()
        Dim metrics = txtMetrics.Text.Trim()

        items.Add(New HoshinItem() With {
            .ItemType = level,
            .ItemName = name,
            .Owner = owner,
            .TargetDate = targetDate,
            .Metrics = metrics,
            .Progress = 0,
            .Status = "NotStarted"
        })
        SaveSessionItems(items)
        RenderMatrix()
        litMessage.Text = "<div class='alert alert-success py-1 small'>Item added.</div>"
        txtItemName.Text = "" : txtOwner.Text = "" : txtTargetDate.Text = "" : txtMetrics.Text = ""
    End Sub

    Private Class HoshinItem
        Public Property ItemType As String
        Public Property ItemName As String
        Public Property Owner As String
        Public Property TargetDate As String
        Public Property Metrics As String
        Public Property Progress As Double
        Public Property Status As String
    End Class

    Private Function GetSessionItems() As List(Of HoshinItem)
        Dim items = TryCast(ViewState("HoshinItems"), List(Of HoshinItem))
        If items Is Nothing Then items = New List(Of HoshinItem)()
        Return items
    End Function

    Private Sub SaveSessionItems(items As List(Of HoshinItem))
        ViewState("HoshinItems") = items
        ViewState("ItemCount") = items.Count
        RenderMatrix()
    End Sub

    Private Sub RenderMatrix()
        Dim items = GetSessionItems()
        If items.Count = 0 Then
            pnlMatrix.Visible = False
            pnlNoMatrix.Visible = True
            btnSave.Visible = False
            Return
        End If

        Dim strategic = items.Where(Function(i) i.ItemType = "Strategic").ToList()
        Dim tactical = items.Where(Function(i) i.ItemType = "Tactical").ToList()
        Dim operational = items.Where(Function(i) i.ItemType = "Operational").ToList()

        Dim sb As New StringBuilder()
        sb.Append("<table class='table table-bordered hk-matrix'>")
        sb.Append("<thead class='table-dark'><tr><th style='width:40%;'>Strategic Objectives</th><th style='width:15%;'>Owner</th><th style='width:10%;'>Target</th><th style='width:25%;'>Metrics</th><th style='width:10%;'>Progress</th></tr></thead><tbody>")

        For Each s In strategic
            sb.Append("<tr class='hk-strategic'>")
            sb.Append("<td class='fw-bold'>" & HttpUtility.HtmlEncode(s.ItemName) & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(s.Owner) & "</td>")
            sb.Append("<td>" & s.TargetDate & "</td>")
            sb.Append("<td>" & HttpUtility.HtmlEncode(s.Metrics) & "</td>")
            sb.Append("<td>" & RenderProgress(s.Progress) & "</td>")
            sb.Append("</tr>")

            ' Tactical items under this strategic
            Dim relatedTactical = tactical.Where(Function(t) True).ToList()
            For Each t In relatedTactical
                sb.Append("<tr class='hk-tactical'>")
                sb.Append("<td style='padding-left:24px;'>â†’ " & HttpUtility.HtmlEncode(t.ItemName) & "</td>")
                sb.Append("<td>" & HttpUtility.HtmlEncode(t.Owner) & "</td>")
                sb.Append("<td>" & t.TargetDate & "</td>")
                sb.Append("<td>" & HttpUtility.HtmlEncode(t.Metrics) & "</td>")
                sb.Append("<td>" & RenderProgress(t.Progress) & "</td>")
                sb.Append("</tr>")

                ' Operational items under this tactical
                For Each o In operational
                    sb.Append("<tr class='hk-operational'>")
                    sb.Append("<td style='padding-left:48px;'>  âˆ˜ " & HttpUtility.HtmlEncode(o.ItemName) & "</td>")
                    sb.Append("<td>" & HttpUtility.HtmlEncode(o.Owner) & "</td>")
                    sb.Append("<td>" & o.TargetDate & "</td>")
                    sb.Append("<td>" & HttpUtility.HtmlEncode(o.Metrics) & "</td>")
                    sb.Append("<td>" & RenderProgress(o.Progress) & "</td>")
                    sb.Append("</tr>")
                Next
                operational.Clear()
            Next
            tactical = tactical.Except(relatedTactical).ToList()
        Next

        sb.Append("</tbody></table>")
        litMatrix.Text = sb.ToString()
        pnlMatrix.Visible = True
        pnlNoMatrix.Visible = False
        btnSave.Visible = True
    End Sub

    Private Function RenderProgress(pct As Double) As String
        Dim color = If(pct >= 75, "bg-success", If(pct >= 50, "bg-warning", If(pct >= 25, "bg-info", "bg-danger")))
        Return "<div class='hk-progress bg-light'><div class='hk-progress-bar " & color & "' style='width:" & pct.ToString("F0") & "%'></div></div><small>" & pct.ToString("F0") & "%</small>"
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Return
        Dim modelName = txtModelName.Text.Trim()
        If modelName = "" Then modelName = "Hoshin - " & DateTime.Today.ToString("MMM dd, yyyy")
        Dim fiscalYear As Integer
        Integer.TryParse(txtFiscalYear.Text.Trim(), fiscalYear)
        If fiscalYear < 2000 Then fiscalYear = DateTime.Today.Year

        Dim items = GetSessionItems()
        If items.Count = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>No items to save.</div>" : Return
        End If

        Dim conn = DbHelper.GetConnection()
        Try
            conn.Open()
            Using tx = conn.BeginTransaction()
                Using cmd As New MySqlCommand(
                    "INSERT INTO HoshinKanriModels (TenantId, ProjectId, ModelName, FiscalYear, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Name, @FY, @UID)", conn, tx)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", pid)
                    cmd.Parameters.AddWithValue("@Name", modelName)
                    cmd.Parameters.AddWithValue("@FY", fiscalYear)
                    cmd.Parameters.AddWithValue("@UID", CurrentUserId)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim modelId = CInt(cmd.ExecuteScalar())

                    For i = 0 To items.Count - 1
                        Dim it = items(i)
                        Using icmd As New MySqlCommand(
                            "INSERT INTO HoshinKanriItems (ModelId, ItemType, ItemIndex, ItemName, Owner, TargetDate, Metrics, Progress, Status) " &
                            "VALUES (@ModelId, @Type, @Idx, @Name, @Owner, @Date, @Metrics, @Prog, @Status)", conn, tx)
                            icmd.Parameters.AddWithValue("@ModelId", modelId)
                            icmd.Parameters.AddWithValue("@Type", it.ItemType)
                            icmd.Parameters.AddWithValue("@Idx", i)
                            icmd.Parameters.AddWithValue("@Name", it.ItemName)
                            icmd.Parameters.AddWithValue("@Owner", If(it.Owner <> "", it.Owner, CObj(DBNull.Value)))
                            Dim dt As DateTime
                            icmd.Parameters.AddWithValue("@Date", If(DateTime.TryParse(it.TargetDate, dt), dt, CObj(DBNull.Value)))
                            icmd.Parameters.AddWithValue("@Metrics", If(it.Metrics <> "", it.Metrics, CObj(DBNull.Value)))
                            icmd.Parameters.AddWithValue("@Prog", it.Progress)
                            icmd.Parameters.AddWithValue("@Status", it.Status)
                            icmd.ExecuteNonQuery()
                        End Using
                    Next
                    tx.Commit()
                End Using
            End Using
            LogAudit("Create", "HoshinKanriModel", Nothing, "Saved Hoshin: " & modelName)
            litMessage.Text = "<div class='alert alert-success py-1 small'>Model saved.</div>"
            LoadHistory()
        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger py-1 small'>Error: " & ex.Message & "</div>"
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "DeleteModel" Then
            Using cmd As New MySqlCommand("DELETE FROM HoshinKanriModels WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "HoshinKanriModel", id, "Deleted Hoshin model")
            LoadHistory()
        End If
    End Sub
End Class
