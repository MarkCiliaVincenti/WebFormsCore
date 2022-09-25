﻿Imports WebFormsCore
Imports WebFormsCore.UI.WebControls

Public Partial Class DefaultPage
    Inherits UI.Page

    Public Sub New()
    End Sub

    Protected Property Counter As Integer
        Get
            Return CInt(ViewState("Counter"))
        End Get
        Set
            ViewState("Counter") = Value
        End Set
    End Property

    Protected Async Function btnIncrement_OnClick(sender As Object, e As EventArgs) As Task
        Counter += 1

        rptItems.DataSource = Enumerable.Range(0, Counter)
        Await rptItems.DataBindAsync()
    End Function

    Protected Overrides Sub OnPreRender(args As EventArgs)
        litValue.Text = Counter.ToString()
    End Sub

    Protected Sub rptItems_OnItemDataBound(sender As Object, e As RepeaterItem)
        Dim lit = DirectCast(e.FindControl("litItem"), Literal)

        lit.Text = $"Item {e.ItemIndex}"
    End Sub
End Class
