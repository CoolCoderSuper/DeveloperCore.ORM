Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Public Class SqlServerConnection
    Implements IConnection
    Private ReadOnly _conn As SqlConnection
        
    Public Sub New(connectionString As String)
        _conn = New SqlConnection(connectionString)
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
        Return New SqlServerInsert()
    End Function
        
    Public Function Update() As IUpdate Implements IConnection.Update
        Return New SqlServerUpdate()
    End Function
        
    Public Function Delete() As IDelete Implements IConnection.Delete
        Return New SqlServerDelete()
    End Function
        
    Public Function Transaction() As ITransaction Implements IConnection.Transaction
        Return New SqlServerTransaction(_conn.BeginTransaction())
    End Function
        
    Public Function Command() As ICommand Implements IConnection.Command
        Return New SqlServerCommand() With {.Connection = Me}
    End Function
        
    Public Function Generate() As IQueryGenerator Implements IConnection.Generate
        Return New SqlServerQueryGenerator(Me)
    End Function
End Class