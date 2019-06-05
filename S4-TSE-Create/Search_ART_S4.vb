Public Class Search_ART_S4
    Private Sub Search_ART_S4_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Form1.ITS4App.SetActiveArticle(TextBox1.Text)
    End Sub

    Private Sub Search_ART_S4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        button_enable()
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        button_enable()
    End Sub
    Sub button_enable()
        If TextBox1.Text = "" Then
            Button2.Enabled = False
        Else
            Button2.Enabled = True
        End If
    End Sub
End Class