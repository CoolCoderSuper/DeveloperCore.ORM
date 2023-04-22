Imports BenchmarkDotNet.Attributes
Imports DeveloperCore.ORM
Imports Microsoft.Data.SqlClient
Imports NPoco

<MemoryDiagnoser>
Public Class QueryBenchmark

    Dim _dc As DataContext
    Dim _db As Database
    Dim conn As SqlConnection

    <GlobalSetup>
    Public Sub Setup()
        _dc = New DataContext("Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True")
        conn = New SqlConnection("Server=cch\codingcool;Database=SampleDB;Integrated Security=True;TrustServerCertificate=True")
        _db = New Database(conn)
    End Sub

    <Benchmark>
    Public Sub QueryORM()
        Dim sql = "SELECT * FROM OtherUser"
        Dim result = _dc.Fetch(Of ORMUser)(sql)
    End Sub

    <Benchmark>
    Public Sub QueryNPoco()
        conn.Open()
        Dim sql = "SELECT * FROM OtherUser"
        Dim result = _db.Fetch(Of ORMUser)(sql)
        conn.Close()
    End Sub
End Class