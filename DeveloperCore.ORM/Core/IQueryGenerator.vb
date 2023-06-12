Namespace Core
    Public Interface IQueryGenerator
        Function From(table As String) As IQueryGenerator
        Function [Select](col As String) As IQueryGenerator
        Function [Select](ParamArray cols As String()) As IQueryGenerator
        Function SelectAll() As IQueryGenerator
        Function Filter(col As String, op As String, val As String) As IQueryGenerator
        Function Filter(col As String, op As String, paramName As String, paramValue As String) As IQueryGenerator
        Function [And]() As IQueryGenerator
        Function [Or]() As IQueryGenerator
        Function [Not]() As IQueryGenerator
        Function OrderBy(col As String) As IQueryGenerator
        Function OrderByDesc(col As String) As IQueryGenerator
        Function GetCommand() As ICommand
    End Interface
End Namespace