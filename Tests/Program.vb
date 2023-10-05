Imports System

Module Program
    Private ReadOnly TestClient as new TestClient
    Private ReadOnly TestServer as new TestServer
    Private ReadOnly ModeList As String() = {"client", "server", "both"}
    
    Private TestMode as String = Nothing
    Private TestEndpoint As Net.IPEndPoint = Nothing
    Private TestWorkerCount As UInt32 = Nothing
    Private TestWorkerFrequency As Double = Nothing
    Private TestServerID As String
    Private TestServerIDRaw As Byte()
    Sub Main(Args As String())
        If Args.Length >= 4 Then
            Dim Valid As Boolean = True
            TestMode = Args(0) : If ModeList.Contains(TestMode) = False Then Valid = False
            If Net.IPEndPoint.TryParse(Args(1), TestEndpoint) = False Then Valid = False
            If UInt32.TryParse(Args(2), TestWorkerCount) = False Then Valid = False
            If Double.TryParse(Args(3), TestWorkerFrequency) = False Then Valid = False
            If TestMode = "client"
                If Args.Length >= 5
                    TestServerID = Args(4)
                    TestServerIDRaw = Convert.FromBase64String(TestServerID)
                Else 
                    TestServerID = ""
                End If
            End If
            If Valid = False Then
                Console.WriteLine("Invalid parameters were provided, switching to interactive mode...")
                DoInteractiveInputs()
            Else
                Console.WriteLine("Running tests with parameters: { Mode: " & Args(0) & ", Endpoint: " & Args(1) & ", Worker Count: " & Args(2) & ", Worker Frequency: " & Args(3) & " }")
            End If
        Else
            DoInteractiveInputs()
        End If
        
        TestClient.WorkerCount = TestWorkerCount
        TestClient.WorkerSpeed = TestWorkerFrequency
        TestServer.WorkerCount = TestWorkerCount
        TestServer.WorkerSpeed = TestWorkerFrequency
        
        If TestMode.ToLower() = "client"
            If TestServerID <> "" Then TestClient.AddTrustedToken(TestServerIDRaw)
            TestClient.Connect(TestEndpoint)
            Console.Read()
            If TestClient.Connected = True Then TestClient.Disconnect()
        ElseIf TestMode.ToLower() = "server"
            TestServer.Listen(TestEndpoint)
            Console.Read()
            TestServer.Deafen()
        ElseIf TestMode.ToLower() = "both"
            TestClient.AddTrustedToken(TestServer.IdentificationToken)
            TestServer.Listen(TestEndpoint)
            TestClient.Connect(TestEndpoint)
            Console.Read()
            TestClient.Disconnect()
            TestServer.Deafen()
        End If
    End Sub
    
    Private Sub DoInteractiveInputs()
        Dim ModeSelected As Boolean
        Do
            Console.Write("Mode: ")
            TestMode = Console.ReadLine().ToLower()
            ModeSelected = ModeList.Contains(TestMode)
            
            If ModeSelected = False Then
                Console.WriteLine("      ^")
                Console.WriteLine("      Not a valid mode! Valid mode values: ")
                For Each Item In ModeList
                    Console.Write("'" & Item & "' ")
                Next : Console.WriteLine("")
            End If
        Loop Until ModeSelected = True
        
        Dim EndpointConversionFlag As Boolean : Do
            Console.Write("Endpoint: ")
            EndpointConversionFlag = Net.IPEndPoint.TryParse(Console.ReadLine(), TestEndpoint)
            If EndpointConversionFlag = False Then
                Console.WriteLine("          ^")
                Console.WriteLine("          Not a valid address!")
            End If
        Loop Until EndpointConversionFlag = True
        
        Dim CountConversionFlag As Boolean : Do
            Console.Write("Worker Count: ")
            CountConversionFlag = UInt32.TryParse(Console.ReadLine(), TestWorkerCount)
            If CountConversionFlag = False Then
                Console.WriteLine("              ^")
                Console.WriteLine("              Not a valid worker count!")
            End If
        Loop Until CountConversionFlag = True
        
        Dim FrequencyConversionFlag As Boolean : Do
            Console.Write("Worker Frequency: ")
            FrequencyConversionFlag = Double.TryParse(Console.ReadLine(), TestWorkerFrequency)
            If FrequencyConversionFlag = False Then
                Console.WriteLine("                  ^")
                Console.WriteLine("                  Not a valid worker frequency!")
            End If
        Loop Until FrequencyConversionFlag = True
    End Sub
End Module
