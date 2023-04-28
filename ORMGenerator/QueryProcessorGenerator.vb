Imports System.Text
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<Generator(LanguageNames.VisualBasic)>
Public Class QueryProcessorGenerator
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize

    End Sub

    Public Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute
        For Each syntaxTree As SyntaxTree In context.Compilation.SyntaxTrees
            Dim model As SemanticModel = context.Compilation.GetSemanticModel(syntaxTree)
            Dim root As SyntaxNode = syntaxTree.GetRoot()
            root.DescendantNodes.OfType(Of ClassStatementSyntax)().ToList().ForEach(Sub(classStatementSyntax As ClassStatementSyntax)
                                                                                        Dim classSymbol As INamedTypeSymbol = model.GetDeclaredSymbol(classStatementSyntax)
                                                                                        If classSymbol.GetAttributes().Any(Function(attributeData As AttributeData) attributeData.AttributeClass.Name = "GeneratedQueryAttribute") Then
                                                                                            context.AddSource($"{classSymbol.Name}.Query.g.vb", SourceText.From(ProcessClass(classSymbol), Encoding.UTF8))
                                                                                        End If
                                                                                    End Sub)
        Next
    End Sub

    Private Shared Function ProcessClass(classSymbol As INamedTypeSymbol) As String
        Dim props As List(Of IPropertySymbol) = classSymbol.GetMembers.OfType(Of IPropertySymbol)().ToList()
        Dim sb As New StringBuilder(<code>
Imports System.Collections.Generic
Imports Microsoft.Data.SqlClient
Imports System.Data
                                        Public Partial Class <%= classSymbol.Name %>
                                            Public Shared Function Fetch(query As String, connectionString As String) As List(Of <%= classSymbol.Name %>)
                                                Dim result As New List(Of <%= classSymbol.Name %>)()
                                                Using connection As New SqlConnection(connectionString)
                                                    Using command As New SqlCommand(query, connection)
                                                        connection.Open()
                                                        Using reader As SqlDataReader = command.ExecuteReader()
                                                            While reader.Read()
                                                                Dim record As IDataRecord = reader                                                                
                                                                Dim item As New <%= classSymbol.Name %>()
                                                                For i As Integer = 0 To record.FieldCount - 1
                                                                    Dim name As String = record.GetName(i)
                                                                    Select Case name
                                    </code>.Value)
        For Each prop As IPropertySymbol In props
            If Not prop.GetAttributes.Any(Function(attr) attr.AttributeClass.Name = "IgnoreAttribute") Then
                Dim name As String = prop.Name
                If prop.GetAttributes.Any(Function(attr) attr.AttributeClass.Name = "ColumnNameAttribute") Then
                    name = prop.GetAttributes.First(Function(attr) attr.AttributeClass.Name = "ColumnNameAttribute").ConstructorArguments.First().Value.ToString()
                End If
                sb.AppendLine($"Case ""{name}""")
                sb.AppendLine($"item.{prop.Name} = record(i)")
            End If
        Next
        sb.Append(<code>
                                                            End Select
                                                                Next
                                                  result.Add(item)
                                              End While
                                          End Using
                                      End Using
                                  End Using
                              Return result
                          End Function
                      End Class
                  </code>.Value)
        Return SyntaxFactory.ParseCompilationUnit(sb.ToString()).NormalizeWhitespace().ToFullString()
    End Function
End Class
