Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL
    Public Class MSSQLCommand
        Implements ICommand
        Private _command As SqlCommand
        Public Property Connection As IConnection Implements ICommand.Connection
        Public Property Transaction As ITransaction Implements ICommand.Transaction
        Public ReadOnly Property Parameters As New Dictionary(Of String, Object) Implements ICommand.Parameters
        Public Property CommandText As String Implements ICommand.CommandText
        
        Public Function Execute() As Integer Implements ICommand.Execute
            CreateCommand()
            If Connection IsNot Nothing Then _command.Connection = Connection.Connection
            If Transaction IsNot Nothing Then _command.Transaction = Transaction.Transaction
            Return _command.ExecuteNonQuery()
        End Function

        Private Sub CreateCommand()
            _command = New SqlCommand(CommandText)
            For Each kvp As KeyValuePair(Of String, Object) In Parameters
                _command.Parameters.AddWithValue(kvp.Key, kvp.Value)
            Next
        End Sub
    End Class
End Namespace