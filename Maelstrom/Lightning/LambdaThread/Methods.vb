Imports System.ComponentModel

Namespace Lightning

    Friend Partial Class LambdaThread
    
        Public Sub New(ThreadStart As ThreadStart, Optional Name As String = "New LambdaThread Instance")
            Me.Name = Name
            Workload = ThreadStart
            Worker = New BackgroundWorker()
            AddHandler Worker.DoWork, AddressOf WorkThread
        End Sub

        Private Sub WorkThread(Sender As Object, E As DoWorkEventArgs)
            Threading.Thread.CurrentThread.Name = Name
            Workload.Invoke()
        End Sub

        Public Sub Start()
            Worker.RunWorkerAsync()
        End Sub
    
        Public Shared Function Start(ThreadStart As ThreadStart, Optional Name As String = "New LambdaThread Instance") As LambdaThread
            Dim NewThread As New LambdaThread(ThreadStart, Name)
            NewThread.Start() : Return NewThread
        End Function
    
        Public Shared Sub Yield()
            Threading.Thread.Yield()
        End Sub
    
        Public Shared Sub Sleep(Duration As TimeSpan)
            Threading.Thread.Sleep(Duration)
        End Sub
    End Class

    Friend Partial Class LambdaThread(Of T1)
        Public Sub New(ThreadStart As ThreadStart, Parameter1 As T1, Optional Name As String = "New LambdaThread Instance")
            Me.Name = Name
            P1 = Parameter1
            Workload = ThreadStart
            Worker = New BackgroundWorker()
            AddHandler Worker.DoWork, AddressOf WorkThread
        End Sub

        Private Sub WorkThread(Sender As Object, E As DoWorkEventArgs)
            Threading.Thread.CurrentThread.Name = Name
            Workload.Invoke(P1)
        End Sub
    
        Public Sub Start()
            Worker.RunWorkerAsync()
        End Sub
    
        Public Shared Function Start(ThreadStart As ThreadStart, Parameter1 As T1, Optional Name As String = "New LambdaThread Instance") As LambdaThread(Of T1)
            Dim NewThread As New LambdaThread(Of T1)(ThreadStart, Parameter1, Name)
            NewThread.Start() : Return NewThread
        End Function
    
    End Class

    Friend Partial Class LambdaThread(Of T1, T2)
        Public Sub New(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Optional Name As String = "New LambdaThread Instance")
            Me.Name = Name
            P1 = Parameter1
            P2 = Parameter2
            Workload = ThreadStart
            Worker = New BackgroundWorker()
            AddHandler Worker.DoWork, AddressOf WorkThread
        End Sub

        Private Sub WorkThread(Sender As Object, E As DoWorkEventArgs)
            Threading.Thread.CurrentThread.Name = Name
            Workload.Invoke(P1, P2)
        End Sub
    
        Public Sub Start()
            Worker.RunWorkerAsync()
        End Sub
    
        Public Shared Function Start(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Optional Name As String = "New LambdaThread Instance") As LambdaThread(Of T1, T2)
            Dim NewThread As New LambdaThread(Of T1, T2)(ThreadStart, Parameter1, Parameter2, Name)
            NewThread.Start() : Return NewThread
        End Function
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3)
        Public Sub New(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Optional Name As String = "New LambdaThread Instance")
            Me.Name = Name
            P1 = Parameter1
            P2 = Parameter2
            P3 = Parameter3
            Workload = ThreadStart
            Worker = New BackgroundWorker()
            AddHandler Worker.DoWork, AddressOf WorkThread
        End Sub

        Private Sub WorkThread(Sender As Object, E As DoWorkEventArgs)
            Threading.Thread.CurrentThread.Name = Name
            Workload.Invoke(P1, P2, P3)
        End Sub
    
        Public Sub Start()
            Worker.RunWorkerAsync()
        End Sub
    
        Public Shared Function Start(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Optional Name As String = "New LambdaThread Instance") As LambdaThread(Of T1, T2, T3)
            Dim NewThread As New LambdaThread(Of T1, T2, T3)(ThreadStart, Parameter1, Parameter2, Parameter3, Name)
            NewThread.Start() : Return NewThread
        End Function
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3, T4)
        Public Sub New(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Optional Name As String = "New LambdaThread Instance")
            Me.Name = Name
            P1 = Parameter1
            P2 = Parameter2
            P3 = Parameter3
            P4 = Parameter4
            Workload = ThreadStart
            Worker = New BackgroundWorker()
            AddHandler Worker.DoWork, AddressOf WorkThread
        End Sub

        Private Sub WorkThread(Sender As Object, E As DoWorkEventArgs)
            Threading.Thread.CurrentThread.Name = Name
            Workload.Invoke(P1, P2, P3, P4)
        End Sub
    
        Public Sub Start()
            Worker.RunWorkerAsync()
        End Sub
    
        Public Shared Function Start(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Optional Name As String = "New LambdaThread Instance") As LambdaThread(Of T1, T2, T3, T4)
            Dim NewThread As New LambdaThread(Of T1, T2, T3, T4)(ThreadStart, Parameter1, Parameter2, Parameter3, Parameter4, Name)
            NewThread.Start() : Return NewThread
        End Function
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3, T4, T5)
        Public Sub New(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Parameter5 As T5, Optional Name As String = "New LambdaThread Instance")
            Me.Name = Name
            P1 = Parameter1
            P2 = Parameter2
            P3 = Parameter3
            P4 = Parameter4
            P5 = Parameter5
            Workload = ThreadStart
            Worker = New BackgroundWorker()
            AddHandler Worker.DoWork, AddressOf WorkThread
        End Sub

        Private Sub WorkThread(Sender As Object, E As DoWorkEventArgs)
            Threading.Thread.CurrentThread.Name = Name
            Workload.Invoke(P1, P2, P3, P4, P5)
        End Sub
    
        Public Sub Start()
            Worker.RunWorkerAsync()
        End Sub
    
        Public Shared Function Start(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Parameter5 As T5, Optional Name As String = "New LambdaThread Instance") As LambdaThread(Of T1, T2, T3, T4, T5)
            Dim NewThread As New LambdaThread(Of T1, T2, T3, T4, T5)(ThreadStart, Parameter1, Parameter2, Parameter3, Parameter4, Parameter5, Name)
            NewThread.Start() : Return NewThread
        End Function
    End Class

    Friend Partial Class LambdaThread(Of T1, T2, T3, T4, T5, T6)
        Public Sub New(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Parameter5 As T5, Parameter6 As T6, Optional Name As String = "New LambdaThread Instance")
            Me.Name = Name
            P1 = Parameter1
            P2 = Parameter2
            P3 = Parameter3
            P4 = Parameter4
            P5 = Parameter5
            P6 = Parameter6
            Workload = ThreadStart
            Worker = New BackgroundWorker()
            AddHandler Worker.DoWork, AddressOf WorkThread
        End Sub

        Private Sub WorkThread(Sender As Object, E As DoWorkEventArgs)
            Threading.Thread.CurrentThread.Name = Name
            Workload.Invoke(P1, P2, P3, P4, P5, P6)
        End Sub
    
        Public Sub Start()
            Worker.RunWorkerAsync()
        End Sub
    
        Public Shared Function Start(ThreadStart As ThreadStart, Parameter1 As T1, Parameter2 As T2, Parameter3 As T3, Parameter4 As T4, Parameter5 As T5, Parameter6 As T6, Optional Name As String = "New LambdaThread Instance") As LambdaThread(Of T1, T2, T3, T4, T5, T6)
            Dim NewThread As New LambdaThread(Of T1, T2, T3, T4, T5, T6)(ThreadStart, Parameter1, Parameter2, Parameter3, Parameter4, Parameter5, Parameter6, Name)
            NewThread.Start() : Return NewThread
        End Function
    End Class
End NameSpace