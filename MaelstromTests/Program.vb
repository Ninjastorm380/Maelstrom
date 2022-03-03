Imports System
Imports System.Net
Module Program
    Dim TC as new TestClient
    Dim TS as new TestServer
    Dim EP as New IPEndPoint(IPAddress.Parse("21.0.0.17"),55630)
    Sub Main(args As String())
        TS.Start(EP)
        threading.Thread.Sleep(30)
        TC.Connect(EP)
        'threading.Thread.Sleep(1000)
        Console.Read()
        TC.Disconnect()
        threading.Thread.Sleep(30)
        TS.Stop()
    End Sub
End Module
