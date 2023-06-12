Imports DeveloperCore.ORM.Core

Public Class MySqlCommand
    Implements ICommand
    Public Property Connection As IConnection Implements ICommand.Connection
    Public Property Transaction As ITransaction Implements ICommand.Transaction
    Public ReadOnly Property Parameters As New Dictionary(Of String, Object) Implements ICommand.Parameters
    Public Property CommandText As String Implements ICommand.CommandText
        
    Public Function Execute() As Integer Implements ICommand.Execute
        Return CreateCommand().ExecuteNonQuery()
    End Function
        
    Public Function Query() As IReader Implements ICommand.Query
        Return New MySqlReader(CreateCommand().ExecuteReader())
    End Function

    Private Function CreateCommand() As MySqlConnector.MySqlCommand
        Dim command As New MySqlConnector.MySqlCommand(CommandText)
        For Each kvp As KeyValuePair(Of String, Object) In Parameters
            command.Parameters.AddWithValue(kvp.Key, kvp.Value)
        Next
        If Connection IsNot Nothing Then command.Connection = Connection.Connection
        If Transaction IsNot Nothing Then command.Transaction = Transaction.Transaction
        Return command
    End Function
End Class