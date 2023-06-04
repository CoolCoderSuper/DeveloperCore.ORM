Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Namespace MSSQL
    Public Class MSSQLCommand
        Implements ICommand
        Private ReadOnly _command As SqlCommand
        Public Property Connection As IProvider Implements ICommand.Connection
        Public Property Transaction As ITransaction Implements ICommand.Transaction

        Public Sub New(command As SqlCommand)
            _command = command
        End Sub
        
        Public Function Execute() As Integer Implements ICommand.Execute
            If Connection IsNot Nothing Then _command.Connection = Connection.Connection
            If Transaction IsNot Nothing Then _command.Transaction = Transaction.Transaction
            Return _command.ExecuteNonQuery()
        End Function
    End Class
End NameSpace