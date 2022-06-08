Imports System
Imports System.Net
Module Module1
    Dim TC as new TestClient
    Dim TS as new TestServer
    Dim EP as New IPEndPoint(IPAddress.Parse("0.0.0.0"),55630)
    Sub Main(args As String())
        Console.Write("IP: ")
        Dim IP as String = Console.ReadLine()
        Console.Write("Port: ")
        Dim Port as Int32 = Console.ReadLine()
        Console.Write("Mode: ")
        Dim Mode as String = Console.ReadLine()
        
        EP = New IPEndPoint(IPAddress.Parse(IP), Port)
        If Mode.ToLower() = "client"
            TC.Connect(EP)
            Console.WriteLine("Client connected. Press any key to disconnect and close.")
            Console.Read()
            TC.Disconnect()
        ElseIf Mode.ToLower() = "server"
            TS.Start(EP)
            Console.WriteLine("Server online. Press any key to disconnect and close.")
            Console.Read()
            TS.Stop()
        End If
    End Sub
End Module
