Imports DeveloperCore.ORM.Core

Public Class MySqlConnection
    Implements IConnection
    Private ReadOnly _conn As MySqlConnector.MySqlConnection
        
    Public Sub New(connectionString As String)
        _conn = New MySqlConnector.MySqlConnection(connectionString)
    End Sub

    Public ReadOnly Property Connection As Object Implements IConnection.Connection
        Get
            return _conn
        End Get
    End Property

    Public Sub Connect() Implements IConnection.Connect
        _conn.Open()
    End Sub
        
    Public Sub Disconnect() Implements IConnection.Disconnect
        _conn.Close()
    End Sub
        
    Public Function Insert() As IInsert Implements IConnection.Insert
        Return New MySqlInsert()
    End Function
        
    Public Function Update() As IUpdate Implements IConnection.Update
        Return New MySqlUpdate()
    End Function
        
    Public Function Delete() As IDelete Implements IConnection.Delete
        Return New MySqlDelete()
    End Function
        
    Public Function Transaction() As ITransaction Implements IConnection.Transaction
        Return New MySqlTransaction(_conn.BeginTransaction())
    End Function
        
    Public Function Command() As ICommand Implements IConnection.Command
        Return New MySqlCommand() With {.Connection = Me}
    End Function
        
    Public Function Generate() As IQueryGenerator Implements IConnection.Generate
        Return New MySqlQueryGenerator(Me)
    End Function
End Class