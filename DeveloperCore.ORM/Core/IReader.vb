Imports System.Data

Namespace Core
    Public interface IReader
        Function Read() As Boolean
        Function GetRecord() As IDataRecord
    end interface
End NameSpace