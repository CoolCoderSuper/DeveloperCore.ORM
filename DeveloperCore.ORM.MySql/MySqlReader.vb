Imports DeveloperCore.ORM.Core
Imports MySqlConnector

Public Class MySqlReader
    Implements IReader
    Private ReadOnly _reader As MySqlDataReader

    Public Sub New(reader As MySqlDataReader)
        _reader = reader
    End Sub
        
    Public Function Read() As Boolean Implements IReader.Read
        Return _reader.Read()
    End Function
        
    Public Function GetRecord() As IRecord Implements IReader.GetRecord
        Return New MySqlRecord(_reader)
    End Function
End Class