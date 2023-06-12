Imports DeveloperCore.ORM.Core
Imports Microsoft.Data.SqlClient

Public Class SqlServerReader
    Implements IReader
    Private ReadOnly _reader As SqlDataReader

    Public Sub New(reader As SqlDataReader)
        _reader = reader
    End Sub
        
    Public Function Read() As Boolean Implements IReader.Read
        Return _reader.Read()
    End Function
        
    Public Function GetRecord() As IRecord Implements IReader.GetRecord
        Return New SqlServerRecord(_reader)
    End Function
End Class