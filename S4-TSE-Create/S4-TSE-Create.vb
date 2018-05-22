Imports System
Imports System.IO
Imports System.Text
Public Class Form1
    Public ITS4App As S4.TS4App
    Dim search_API As New search_API
    Public DocID, ArtID As Integer
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ITS4App = CreateObject("S4.TS4App")
        ITS4App.Login()

        Label1.Text = ITS4App.GetUserFullName_ByUserID(ITS4App.GetUserID)
        search_API.Report_txt("S4_DocEdit_report")
        ComboBox1.Text = "Деталь"
        If ArtID = 0 Or ArtID = Nothing Then
            Button1.Enabled = False
        Else
            Button1.Enabled = True
        End If
        'search_API.Report_txt("S4_TSE_Create_report")
    End Sub

    Private Sub ВыбратьОбъектToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ВыбратьОбъектToolStripMenuItem.Click
        import_articl()
    End Sub
    Sub import_articl()
        SelectArticl_procedure()
        artic_param_copy()
        ArtId_Chanched()
    End Sub
    Sub ArtId_Chanched()
        If ArtID = 0 Or ArtID = Nothing Then
            Button1.Enabled = False
        Else
            Button1.Enabled = True
        End If
    End Sub
    Public Sub SelectArticl_procedure()
        ITS4App.StartSelectArticles()
        ITS4App.SelectArticles()
        If ITS4App.SelectedArticlesCount = 0 Then
            Exit Sub
        End If
        ArtID = ITS4App.GetSelectedArticleID(0)
        ITS4App.EndSelectArticles()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim new_ArtID As Integer
        Dim cmd As String

        Try
            Try
                new_ArtID = ITS4App.AddNewArticle2(TextBox1.Text, "", TextBox2.Text, 4)
                MsgBox("Объект c атрибутами " & TextBox1.Text & " - " & TextBox2.Text, 4 & " успешно создан")
                If new_ArtID = -1 Then
                    MsgBox("Не удалось создать Объект." & vbNewLine & "Проверьте уникальность Обозначения в архиве S4")
                    Exit Sub
                End If
            Catch ex As Exception
                MsgBox(ITS4App.ErrorMessage)
            End Try
            ITS4App.OpenArticle(new_ArtID)
            Call ITS4App.SetFieldValue_Articles("Материал", TextBox3.Text)
            Call ITS4App.SetFieldValue_Articles("Масса", TextBox4.Text)
            Dim razdel As Integer
            Select Case ComboBox1.Text
                Case "Деталь"
                    razdel = 4
                Case "Сборочная единица"
                    razdel = 3
                Case "Материал"
                    razdel = 7
                Case "Документация"
                    razdel = 2
            End Select
            'добавляем его в состав выделенного объекта AddBOMItem2, параметр CtxID = 2  
            Dim newBOMItem As Integer = ITS4App.AddBOMItem2(ArtID, -1, new_ArtID, -1, CInt(TextBox8.Text), 0, razdel, CInt(TextBox7.Text), CStr(TextBox6.Text), 2)
            MsgBox("Объект добавлен в состав")
        Catch ex As Exception
            MsgBox(ITS4App.ErrorMessage)
        End Try

        cmd = Now & " - создан объект( " & ComboBox1.Text & " ) ArtID: " & new_ArtID & " Входит в состав объекта с ArtID: " & ArtID
        'search_API.Write_Report_txt(cmd)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        all_clear()
    End Sub
    Sub all_clear()
        ArtID = 0
        DocID = 0
        TextBox1.Text = Nothing
        TextBox2.Text = Nothing
        TextBox3.Text = Nothing
        TextBox4.Text = Nothing
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.Close()
    End Sub

    Private Sub ОПрограммеToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ОПрограммеToolStripMenuItem.Click
        App_info.ShowDialog()
    End Sub

    Private Sub СправкаToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles СправкаToolStripMenuItem.Click
        spravka()
    End Sub
    Public Sub spravka()
        System.Diagnostics.Process.Start("http://www.evernote.com/l/AjJ51PBwhLhKlbnpaSwjSmdH9do6Zx4RI6w/")
    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Control And e.KeyCode.ToString = "O" Then
            Me.KeyPreview = False
            import_articl()
        ElseIf e.Control And e.KeyCode.ToString = "Delete" Then
            Me.KeyPreview = False
            all_clear()
        End If
        Select Case e.KeyCode
            Case Keys.F1
                Me.KeyPreview = False
                spravka()
            Case Keys.F2
                Me.KeyPreview = False
                App_info.ShowDialog()
        End Select
        Me.KeyPreview = True
    End Sub

    Private Sub ВыбратьОтмеченныйОбъектВS4ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        select_doc_S4()
    End Sub
    Sub select_doc_S4()

        Dim SelItems As Object = ITS4App.GetSelectedItems    'S4.TS4SelectedItems
        Dim SelectCount As Integer = SelItems.SelectedCount

        Dim SelectedCount As Integer = SelItems.SelectedCount

        While SelItems.NextSelected <> 0
            Dim DocID As Integer = ITS4App.ActiveDocID
            ITS4App.OpenDocument(DocID)
        End While
        ITS4App.CloseProgressBarForm()
    End Sub

    Private Sub ОткрытьИсториюToolStripMenuItem_Click(sender As Object, e As EventArgs)
        If File.Exists(search_API.path) Then
            Process.Start(search_API.path)
        End If
    End Sub

    Sub artic_param_copy()
        ITS4App.OpenDocArticles(ArtID)
        DocID = ITS4App.GetDocID_ByArtID(ArtID)
        ITS4App.OpenDocument(DocID)
        ITS4App.ReturnFieldValueWithImbaseKey = 1
        Dim oboz As String = ITS4App.GetFieldValue("Обозначение")
        Dim naim As String = ITS4App.GetFieldValue("Наименование")
        Dim Old_mater As String = ITS4App.GetFieldValue("Материал")
        Dim Old_mass As String = ITS4App.GetFieldValue("Масса")
        ITS4App.CloseDocument()
        ITS4App.CloseArticle()
        TextBox1.Text = oboz
        TextBox2.Text = naim
        TextBox3.Text = Old_mater
        TextBox4.Text = Old_mass
    End Sub
End Class
