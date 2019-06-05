Public Class Options_TSE
    Public txt_After_Oboz = get_reesrt_value("txt_After_Oboz", "_1^1")
    Public txt_After_Oboz_TP = get_reesrt_value("txt_After_Oboz_TP", "ТП")
    Public Arch_Name As String = "Архив в разработке"
    Public Doc_type_Name As String = "Лекало/Техпроцесс (PDF)"
    Public ArtSectionName As String = "Деталь"
    Public Razdel_SP_name As String = "Деталь"
    Public sv9z As String = "Технологическая"
    Public default_folder_path As String = get_reesrt_value("default_folder_path", "C:\IM\IMWork")
    Public DocNotification As String = get_reesrt_value("DocNotification", Convert.ToString(True))
    Private Sub Options_TSE_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TextBox1.Text = txt_After_Oboz
        TextBox2.Text = txt_After_Oboz_TP

        ComboBox1.Items.Add(Arch_Name)
        ComboBox1.Text = Arch_Name

        ComboBox2.Items.Add(Doc_type_Name)
        ComboBox2.Text = Doc_type_Name

        ComboBox3.Items.Add(ArtSectionName)
        ComboBox3.Text = ArtSectionName

        ComboBox4.Items.Add(Razdel_SP_name)
        ComboBox4.Text = Razdel_SP_name

        ComboBox5.Items.Add(sv9z)
        ComboBox5.Text = sv9z

        TextBox5.Text = default_folder_path

        CheckBox1.Checked = Convert.ToBoolean(DocNotification)
    End Sub

    Private Sub Options_TSE_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        save_options()
        Me.Close()
    End Sub
    Sub save_options()
        set_reesrt_value("txt_After_Oboz", TextBox1.Text)
        set_reesrt_value("txt_After_Oboz_TP", TextBox1.Text)
        set_reesrt_value("default_folder_path", TextBox5.Text)
        set_reesrt_value("DocNotification", Convert.ToString(CheckBox1.Checked))

        Form1.txt_After_Oboz = TextBox1.Text
        Form1.txt_After_Oboz_TP = TextBox2.Text
        Form1.default_folder_path = TextBox5.Text
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        select_default_folder_path()
    End Sub
    Sub select_default_folder_path()
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            TextBox5.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub
End Class