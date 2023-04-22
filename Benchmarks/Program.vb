Imports BenchmarkDotNet.Running
Imports Bogus

Module Program
    Sub Main(args As String())
        'Dim faker As New Faker(Of ORMUser)
        'faker.RuleFor(Function(x) x.Name, Function(f) f.Name.FullName())
        'Dim res = faker.Generate(10000)
        'Dim dc = New DeveloperCore.ORM.DataContext("Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True")
        'Dim i As Integer = 0
        'For Each item In res
        '    dc.Insert(item)
        '    i += 1
        '    Console.WriteLine(i)
        'Next
        BenchmarkRunner.Run(Of QueryBenchmark)()
    End Sub
End Module