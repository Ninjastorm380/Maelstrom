Imports System
Imports System.Net

Module Program
    Private ReadOnly TC as new TestClient
    Private ReadOnly TS as new TestServer
    Private EP as IPEndPoint
    Sub Main(args As String())
        Console.Write("Mode: ")
        Dim Mode as String = Console.ReadLine()
        Console.Write("IP: ")
        Dim IP as String = Console.ReadLine()
        Console.Write("Port: ")
        Dim Port as Int32 = Console.ReadLine()
        Console.Write("Rate: ")
        Dim Rate as Double = Console.ReadLine()
        Console.Write("Instances: ")
        Dim Instances as Uint32 = Console.ReadLine()
        
        EP = New IPEndPoint(IPAddress.Parse(IP), Port)
        If Mode.ToLower() = "client"
            TC.SetSpeed(Rate)
            TC.SetInstances(Instances)
            TC.Connect(EP)
            Console.WriteLine("Client connected. Press any key to disconnect and close.")
            Console.Read()
            TC.Disconnect()
        ElseIf Mode.ToLower() = "server"
            TS.SetSpeed(Rate)
            TS.SetInstances(Instances)
            TS.Listen(EP)
            Console.WriteLine("Server online. Press any key to disconnect and close.")
            Console.Read()
            TS.Deafen()
        ElseIf Mode.ToLower() = "both"
            TS.SetSpeed(Rate)
            TS.SetInstances(Instances)
            TS.Listen(EP)
            Threading.Thread.Sleep(300)
            TC.SetSpeed(Rate)
            TC.SetInstances(Instances)
            TC.Connect(EP)
            Console.WriteLine("Server started and client connected. Press any key to disconnect, shutdown, and close.")
            Console.Read()
            TC.Disconnect()
            TS.Deafen()
        End If
    End Sub
End Module
