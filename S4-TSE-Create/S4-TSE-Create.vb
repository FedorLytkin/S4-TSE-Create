Imports System
Imports System.IO
Imports System.Text
Public Class Form1
    Public ITS4App As S4.TS4App
    Dim search_API As New search_API
    Public DocID, ArtID, oldArtID As Integer
    Public txt_After_Oboz = Options_TSE.txt_After_Oboz  '"_1^1"
    Public txt_After_Oboz_TP = Options_TSE.txt_After_Oboz_TP  '"_1^1"
    Public default_folder_path = Options_TSE.default_folder_path

    Dim doc_type As Integer = 1000057 'типа докуменат Лекало  (1000063 - для техпроцесса PDF)
    Dim Arch_ID As Integer = 7 'архив В разработке
    Dim SectionID As Integer = 4 'тип объекта Детали
    Dim razdel As Integer = 4 'Раздел специи Детали
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tempform = Me

        Chek_licenc()

        ITS4App = CreateObject("S4.TS4App")
        ITS4App.Login()
        Timer1.Enabled = False
        ToolStripComboBox1.Text = "Лекало"
        ToolStripComboBox2.Text = "Лекало"

        ToolStripLabel1.Text = ITS4App.GetUserFullName_ByUserID(ITS4App.GetUserID)
        'search_API.Report_txt("S4_DocEdit_report")
        If ArtID = 0 Or ArtID = Nothing Or TextBox5.Text = Nothing Or TextBox7.Text = Nothing Then
            Button1.Enabled = False
        Else
            Button1.Enabled = True
        End If
        'ButtonEnable()
        ToolStripStatusLabel1.Text = ""
        Timer1.Interval = 100
        'search_API.Report_txt("S4_TSE_Create_report")
    End Sub

    Private Sub ВыбратьОбъектToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ВыбратьОбъектToolStripMenuItem.Click
        import_articl()
    End Sub
    Sub import_articl()
        SelectArticl_procedure()
        artic_param_copy()
        ButtonEnable()
        cellsAdd()
        'ArtId_Chanched()
    End Sub
    Sub cellsAdd()
        Dim oboz_imp, Naim_imp, Mass_imp, Mater_imp As String
        With ITS4App
            .OpenArticle(ArtID)
            oboz_imp = .GetFieldValue_Articles("Обозначение")
            Naim_imp = .GetFieldValue_Articles("Наименование")
            Mater_imp = .GetFieldValue_Articles("Материал")
            Mass_imp = .GetFieldValue_Articles("Масса")
            .CloseArticle()
        End With

        TextBox13.Text = ArtID
        TextBox12.Text = oboz_imp
        TextBox11.Text = Naim_imp
        TextBox10.Text = Mater_imp
        TextBox9.Text = Mass_imp
    End Sub
    Sub ArtId_Chanched()
        If ArtID = 0 Or ArtID = Nothing Then
            ToolStripStatusLabel1.Text = "Выберите объект из S4"
            'Button1.Enabled = False
        Else
            ToolStripStatusLabel1.Text = "Укажите ссылку на создаваемое ТСЕ"
            'Button1.Enabled = True
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
        add_TSE_DOC()
    End Sub
    Sub add_TSE_DOC()
        Dim add_ID As Integer = add_documentLecalo()
        If add_ID > 0 Then
            ToolStripStatusLabel1.Text = "Cоздан Документ DocID: " & add_ID & " ( " & TextBox1.Text & " " & TextBox2.Text & " )"
        End If
        all_clear()
        ButtonEnable()
    End Sub
    Function add_documentLecalo() As Integer
        Dim designation As String = TextBox1.Text
        Dim Naim As String = TextBox2.Text
        Dim doc_filename As String = filerename()


        Dim material As String = TextBox3.Text
        Dim mass As String = TextBox4.Text
        Dim new_ArtID As Integer
        Try
            add_documentLecalo = ITS4App.CreateFileDocumentWithDocType1(doc_filename, doc_type, Arch_ID, designation, Naim)
            new_ArtID = ITS4App.GetArtID_ByDocID(add_documentLecalo)
            ITS4App.OpenArticle(new_ArtID)
            Call ITS4App.SetFieldValue_Articles("Материал", material)
            Call ITS4App.SetFieldValue_Articles("Масса", mass)
        Catch ex As Exception
            MsgBox("Ошибка при создании объекта" & vbNewLine & ex.Message)
        End Try
        'добавляем его в состав выделенного объекта AddBOMItem2, параметр CtxID = 2  
        Try
            Dim newBOMItem As Integer = ITS4App.AddBOMItem2(ArtID, -1, new_ArtID, -1, CInt(TextBox8.Text), 0, razdel, CInt(TextBox7.Text), CStr(TextBox6.Text), 2)
            If Convert.ToBoolean(Options_TSE.DocNotification) Then
                If Docnotifocation(ArtID) Then
                    MsgBox("Вы подписались на уведомление 'О сохранении изменений'")
                End If
            End If
            MsgBox("ТСЕ успешно создана")
        Catch ex As Exception
            MsgBox("Ошибка при создании технологической связи" & vbNewLine & ex.Message)
        End Try
        Return new_ArtID
    End Function
    Function Docnotifocation(ArtId As Integer) As Boolean
        Try
            With ITS4App
                Dim DocId As Integer = .GetDocID_ByArtID(ArtId)
                Dim IserID As Integer = .GetUserID
                Dim notification_type As Integer = 2
                If DocId > 0 Then
                    .AddDocNotification(DocId, IserID, notification_type)
                    Return True
                Else
                    Return False
                End If
            End With
        Catch ex As Exception
            Return False
        End Try
    End Function
    Function filerename() As String
        Dim doc_filename As String = TextBox5.Text
        Dim doc_path As String = doc_filename.Substring(0, doc_filename.LastIndexOf("\"))
        Dim onlyfilename As String = doc_filename.Substring(doc_filename.LastIndexOf("\"))
        'Dim default_folder_path As String = "C:\IM\IMWork"
        If doc_path <> default_folder_path Then
            Dim newfilename As String = default_folder_path & onlyfilename
            If File.Exists(newfilename) Then
                File.Delete(newfilename)
            End If
            File.Copy(doc_filename, default_folder_path & onlyfilename)
            Return newfilename
        Else
            Return doc_filename
        End If
    End Function
    Sub add_art_andDoc_OLD()
        Dim new_ArtID As Integer

        Dim oboz As String = TextBox1.Text
        Dim naim As String = TextBox2.Text
        Dim SectionID As Integer = 4
        Dim material As String = TextBox3.Text
        Dim mass As String = TextBox4.Text
        Try
            Try
                new_ArtID = ITS4App.AddNewArticle2(oboz, "", naim, SectionID)
                MsgBox("Объект c атрибутами " & oboz & " - " & naim & " успешно создан")
                If new_ArtID = -1 Then
                    MsgBox("Не удалось создать Объект." & vbNewLine & "Проверьте уникальность Обозначения в архиве S4")
                    Exit Sub
                End If
            Catch ex As Exception
                MsgBox(ITS4App.ErrorMessage)
            End Try
            ITS4App.OpenArticle(new_ArtID)
            Call ITS4App.SetFieldValue_Articles("Материал", material)
            Call ITS4App.SetFieldValue_Articles("Масса", mass)
            Dim razdel As Integer
            razdel = 4
            'добавляем его в состав выделенного объекта AddBOMItem2, параметр CtxID = 2  
            Dim newBOMItem As Integer = ITS4App.AddBOMItem2(ArtID, -1, new_ArtID, -1, CInt(TextBox8.Text), 0, razdel, CInt(TextBox7.Text), CStr(TextBox6.Text), 2)
            'MsgBox("Объект добавлен в состав")
            Dim new_DocID As Integer = add_documentLecalo()
            If new_DocID > 0 Then
                ToolStripStatusLabel1.Text = "Cоздан Документ DocID: " & new_DocID & " ( " & oboz & " " & naim & " )"
            End If
        Catch ex As Exception
            MsgBox(ITS4App.ErrorMessage)
        End Try

        all_clear()
        ToolStripStatusLabel1.Text = "Cоздан объект ArtID: " & new_ArtID & " ( " & oboz & " " & naim & " )" & " Входит в состав объекта с ArtID: " & ArtID
        'search_API.Write_Report_txt(cmd)
    End Sub
    Function add_documentLecalo2() As Integer
        Dim designation As String = TextBox1.Text
        Dim Naim As String = TextBox2.Text
        Dim doc_filename As String = TextBox5.Text

        Dim doc_type As Integer = 1000057
        Dim Arch_ID As Integer = 7 'архив В разработке
        add_documentLecalo2 = ITS4App.CreateFileDocumentWithDocType2(doc_filename, doc_type, Arch_ID, designation, Naim, 4)
    End Function
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        all_clear()
    End Sub
    Sub all_clear()
        oldArtID = ArtID
        ArtID = 0
        DocID = 0
        TextBox1.Text = Nothing
        TextBox2.Text = Nothing
        TextBox3.Text = Nothing
        TextBox4.Text = Nothing
        TextBox5.Text = Nothing
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
        ElseIf e.Control And e.Shift And e.KeyCode.ToString = "L" Then
            licCFG()
        End If
        Select Case e.KeyCode
            Case Keys.F1
                Me.KeyPreview = False
                spravka()
            Case Keys.F2
                Me.KeyPreview = False
                App_info.ShowDialog()
            Case Keys.Escape
                cancelled = True
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

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        import_articl()
    End Sub

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        spravka()
    End Sub

    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        App_info.ShowDialog()
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        ButtonEnable()
    End Sub
    Function check_exist_Articke_inS4()
        Dim check_oboz As String = TextBox1.Text
        Dim check_ArtID As Integer = ITS4App.GetArtID_ByDesignation(check_oboz)
        If check_ArtID > 0 Then
            ToolStripStatusLabel1.Text = "В архиве уже есть объект с таким Обозначением ( " & check_oboz & ")!" & vbNewLine & check_oboz & "  у объекта с ArtID :" & check_ArtID
        Else
            ToolStripStatusLabel1.Text = ""
        End If
        Return check_ArtID
    End Function
    Sub ButtonEnable()
        PositioChange()
        Dim check_DocID As Integer = ITS4App.GetDocID_ByFilename(TextBox5.Text)
        Dim filename As String = TextBox5.Text.Substring(TextBox5.Text.LastIndexOf("\") + 1)
        If ArtID > 0 And check_exist_Articke_inS4() < 0 And TextBox1.Text IsNot "" And TextBox5.Text IsNot "" And check_DocID <= 0 Then
            Button1.Enabled = True
            СоздатьТСЕToolStripMenuItem.Enabled = True
            ToolStripButton6.Enabled = True
            ToolStripStatusLabel1.Text = "Готово к созданию ТСЕ. Проверьте параметы и Поехали!"
        Else
            If check_DocID > 0 Then
                ToolStripStatusLabel1.Text = "Документ с файлом ( " & filename & " ) уже существует в S4!" & vbNewLine & "Выберите другой файл!"
            End If
            If DocID <= 0 Then
                ToolStripStatusLabel1.Text = "Выберите объект из S4!"
            End If
            Button1.Enabled = False
            СоздатьТСЕToolStripMenuItem.Enabled = False
            ToolStripButton6.Enabled = False
        End If
        If oldArtID > 0 Then
            ПоказатьСToolStripMenuItem.Enabled = True
            ToolStripButton5.Enabled = True
            ToolStripButton8.Enabled = True
        Else
            ПоказатьСToolStripMenuItem.Enabled = False
            ToolStripButton5.Enabled = False
            ToolStripButton8.Enabled = False
        End If
    End Sub

    Private Sub TextBox5_TextChanged(sender As Object, e As EventArgs) Handles TextBox5.TextChanged

        ButtonEnable()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        InitializeOpenFileDialog()
    End Sub
    Public Sub InitializeOpenFileDialog()
        ' Set the file dialog to filter for graphics files.
        Me.OpenFileDialog1.Filter = "Файлы(*.DXF)|"


        ' Allow the user to select multiple images.
        Me.OpenFileDialog1.Multiselect = False
        Me.OpenFileDialog1.Title = "Укажите ссылку на файл DXF"
        Dim dr As DialogResult = Me.OpenFileDialog1.ShowDialog()
        If (dr = System.Windows.Forms.DialogResult.OK) Then
            ' Read the files
            Dim file, fileformat As String
            file = OpenFileDialog1.FileName
            fileformat = file.Substring(file.LastIndexOf(".") + 1)
            If doc_type = 1000057 Then
                If UCase(fileformat) = UCase("DXF") Then
                    TextBox5.Text = file
                Else
                    MsgBox("Файл должен быть в формате .DXF")
                End If
            ElseIf doc_type = 1000063 Then
                If UCase(fileformat) = UCase("PDF") Then
                    TextBox5.Text = file
                Else
                    MsgBox("Файл должен быть в формате .PDF")
                End If
            End If
        End If
    End Sub

    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.All
        End If
    End Sub

    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop
        Dim file, fileformat As String
        For Each file In e.Data.GetData(DataFormats.FileDrop)
            fileformat = file.Substring(file.LastIndexOf(".") + 1)
            If doc_type = 1000057 Then
                If UCase(fileformat) = "DXF" Then
                    TextBox5.Text = file
                    Exit For
                Else
                    MsgBox("Для данного Типа документа необходимо выбрать файл типа DXF")
                End If
            ElseIf doc_type = 1000063 Then
                If UCase(fileformat) = "PDF" Then
                    TextBox5.Text = file
                    Exit For
                Else
                    MsgBox("Для данного Типа документа необходимо выбрать файл типа PDF")
                End If
            End If
        Next
    End Sub


    Private Sub ToolStripButton4_Click_1(sender As Object, e As EventArgs) Handles ToolStripButton4.Click
        InitializeOpenFileDialog()
    End Sub

    Private Sub НастройкиToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles НастройкиToolStripMenuItem.Click
        Options_TSE.ShowDialog()
    End Sub


    Private Sub ToolStripButton5_Click(sender As Object, e As EventArgs) Handles ToolStripButton5.Click
        search()
    End Sub

    Private Sub ПоказатьСToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ПоказатьСToolStripMenuItem.Click
        search()
    End Sub

    Private Sub ToolStripButton6_Click(sender As Object, e As EventArgs) Handles ToolStripButton6.Click
        add_TSE_DOC()
    End Sub

    Private Sub ContextMenuStrip1_Opening(sender As Object, e As ComponentModel.CancelEventArgs) Handles ContextMenuStrip1.Opening
        If TextBox5.Text = "" Then
            ОткрытьDXFToolStripMenuItem.Enabled = False
        Else
            ОткрытьDXFToolStripMenuItem.Enabled = True
        End If
    End Sub

    Private Sub ОткрытьDXFToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ОткрытьDXFToolStripMenuItem.Click
        Process.Start(TextBox5.Text)
    End Sub

    Private Sub СкачатьОбъектыИзПроектаToolStripMenuItem_Click(sender As Object, e As EventArgs)
        ITS4App.savef
    End Sub

    Private Sub ToolStripButton7_Click(sender As Object, e As EventArgs)
        filerename()
    End Sub

    Private Sub СохранитьDXFИзСоставаToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles СохранитьDXFИзСоставаToolStripMenuItem.Click
        save_Copy_dxf_for_Derevo_sostava()
    End Sub
    Sub save_Copy_dxf_for_Derevo_sostava()
        Dim Copy_directory As String
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            Copy_directory = FolderBrowserDialog1.SelectedPath
        Else
            Exit Sub
        End If
        With ITS4App
            SelectArticl_procedure()
            If ArtID <= 0 Then Exit Sub

            '.OpenArticleStructure2(ArtID, -1, 2)
            .OpenArticleStructureExpanded2(ArtID, -1, 2)
            .asFirst()
            While .asEof = 0
                If .asGetArtKind = SectionID Then
                    Dim tmp_artID As Integer = .asGetArtID
                    Dim tmp_Doc_ID As Integer = .GetDocID_ByArtID(tmp_artID)
                    .OpenDocument(tmp_Doc_ID)
                    If .GetDocType = doc_type Then
                        .CopyToDir(Copy_directory)
                    End If
                    .CloseDocument()
                End If
                .asNext()
            End While
        End With
    End Sub

    Private Sub НайтиОбъектToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles НайтиОбъектToolStripMenuItem.Click
        Search_ART_S4.ShowDialog()
    End Sub

    Sub search()
        ITS4App.SetActiveArticle(oldArtID)
    End Sub

    Private Sub ВыходToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ВыходToolStripMenuItem.Click
        Me.Close()
    End Sub
    Private Sub ToolStripButton7_Click_1(sender As Object, e As EventArgs) Handles ToolStripButton7.Click
        Search_ART_S4.ShowDialog()
    End Sub
    Private Sub ToolStripComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ToolStripComboBox1.SelectedIndexChanged
        ToolStripComboBox2.Text = ToolStripComboBox1.Text
        If ToolStripComboBox1.Text = "Лекало" Then
            doc_type = 1000057
            razdel = 4
        Else
            doc_type = 1000063
            razdel = 1
        End If
    End Sub

    Private Sub ToolStripComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ToolStripComboBox2.SelectedIndexChanged
        ToolStripComboBox1.Text = ToolStripComboBox2.Text
        If ToolStripComboBox1.Text = "Лекало" Then
            doc_type = 1000057
            razdel = 4
        Else
            doc_type = 1000063
            razdel = 1
        End If
    End Sub

    Private Sub WhatsNewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles WhatsNewToolStripMenuItem.Click
        Process.Start("http://www.evernote.com/l/AjK4nSp39FxE_4hogonTePu9zxTdojXM0mQ/")
    End Sub

    Private Sub ToolStripButton8_Click(sender As Object, e As EventArgs) Handles ToolStripButton8.Click
        ITS4App.SelectArticleFromTree(oldArtID)
    End Sub
    Private Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Long) As Integer
    Private cancelled As Boolean
    Private Sub Test1ToolStripMenuItem_Click(sender As Object, e As EventArgs) 
        Docnotifocation(7729)
    End Sub

    Private Sub Timer1_Timer()
        If GetAsyncKeyState(Keys.Escape) <> 0 Then
            cancelled = True
            Timer1.Enabled = False
        End If
    End Sub

    Private Sub СоздатьТСЕToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles СоздатьТСЕToolStripMenuItem.Click
        add_TSE_DOC()
    End Sub

    Private Sub ToolStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles ToolStrip1.ItemClicked
        add_TSE_DOC()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        import_articl()
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If TextBox13.Text <> "" Then
            Try
                ITS4App.SetActiveArticle(TextBox13.Text)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub TextBox7_TextChanged(sender As Object, e As EventArgs) Handles TextBox7.TextChanged
        PositioChange()
    End Sub
    Function PositioChange() As Boolean
        If TextBox7.Text Is Nothing Or TextBox7.Text.Trim(" ") = "" Then
            Button1.Enabled = Not True
        Else
            With ITS4App
                .OpenArticleStructure2(ArtID, -1, 2)
                .asFirst()
                If TextBox7.Text = .asGetPosition Then
                    Button1.Enabled = Not True
                Else
                    Button1.Enabled = Not False
                End If

                While .asEof <> 1
                    If TextBox7.Text = .asGetPosition Then
                        Button1.Enabled = Not True
                    Else
                        Button1.Enabled = Not False
                    End If
                    .asNext()
                End While
                .CloseArticleStructure()
            End With
            If Not Button1.Enabled Then
                ToolStripStatusLabel1.Text = "Позиция заполнена не корректно"
            Else
                ToolStripStatusLabel1.Text = ""
            End If
            Return Not Button1.Enabled
        End If
    End Function
    Sub artic_param_copy()
        ITS4App.OpenArticle(ArtID)
        'DocID = ITS4App.GetDocID_ByArtID(ArtID)
        'ITS4App.OpenDocument(DocID)
        ITS4App.ReturnFieldValueWithImbaseKey = 1
        Dim oboz As String = ITS4App.GetFieldValue_Articles("Обозначение")
        Dim naim As String = ITS4App.GetFieldValue_Articles("Наименование")
        Dim Old_mater As String = ITS4App.GetFieldValue_Articles("Материал")
        Dim Old_mass As String = ITS4App.GetFieldValue_Articles("Масса")
        ITS4App.CloseDocument()
        ITS4App.CloseArticle()
        If ToolStripComboBox1.Text = "Лекало" Then
            TextBox1.Text = oboz & txt_After_Oboz
        Else
            TextBox1.Text = oboz & txt_After_Oboz_TP
        End If
        TextBox2.Text = naim
        TextBox3.Text = Old_mater
        TextBox4.Text = Old_mass
    End Sub
    Sub artic_param_copy2()
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
        TextBox1.Text = oboz & txt_After_Oboz
        TextBox2.Text = naim
        TextBox3.Text = Old_mater
        TextBox4.Text = Old_mass
    End Sub
End Class
