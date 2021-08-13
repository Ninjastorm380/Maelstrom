Imports System.Drawing
Imports System.Windows.Forms
Friend Module Loader
    Public Sub Main(Args As String())
        Dim NF As New MainForm
        Application.Run(NF)
    End sub
End Module

Friend Class MainForm
    Inherits Form
    
    Private WithEvents LBServerAddress As Label
    Private WithEvents LBServerPort As Label
    Private WithEvents TXServerAddress As TextBox
    Private WithEvents TXServerPort As TextBox
    Private WithEvents BTStartServer as button
    Private WithEvents BTStopServer As Button

    Private WithEvents LBClientAddress As Label
    Private WithEvents LBClientPort As Label
    Private WithEvents TXClientAddress As TextBox
    Private WithEvents TXClientPort As TextBox
    Private WithEvents BTConnectClient As Button
    Private WithEvents BTDisconnectClient As Button
    Private WithEvents BTMessageTest As Button
    Private WithEvents BTBeginEnduranceIntegrityTest As Button
    Private WithEvents BTEndEnduranceIntegrityTest As Button
    
    Private ReadOnly Client As New ClientServerTestLib.TestClient
    Private ReadOnly Server As New ClientServerTestLib.TestServer



    Friend Sub New()
        SuspendLayout()
        Size = New Size(440,256)
        Text = "Networking library tester"
        MaximumSize = Size
        
        LBServerAddress = New Label With {
            .Location = New Point(1,3),
            .Size = New Size(100,24),
            .Text = "Server address:"}
        
        TXServerAddress = New TextBox With {
            .Location = New Point(101,1),
            .Size = New Size(100,24),
            .Text = "127.0.0.1"}
        
        LBServerPort = New Label With {
            .Location = New Point(1,28),
            .Size = New Size(100,24),
            .Text = "Server port:"}
        
        TXServerPort = New TextBox With {
            .Location = New Point(101,26),
            .Size = New Size(100,24),
            .Text = "443"}
        
        BTStartServer = new button With {
            .Location = New Point(1,52),
            .Size = New Size(100,24),
            .Text = "Start server"}
        
        BTStopServer = new button With {
            .Location = New Point(102,52),
            .Size = New Size(100,24),
            .Text = "Stop server"}
        
        LBClientAddress = new Label With {
            .Location = New Point(230,3),
            .Size = New Size(100,24),
            .Text = "Client address:"}
        
        TXClientAddress = New TextBox With {
            .Location = New Point(330,1),
            .Size = New Size(100,24),
            .Text = "127.0.0.1"}
        
        LBClientPort = new Label With {
            .Location = New Point(230,28),
            .Size = New Size(100,24),
            .Text = "Client port:"}
        
        TXClientPort = new TextBox With {
            .Location = New Point(330,26),
            .Size = New Size(100,24),
            .Text = "443"}
        
        BTConnectClient = new button With {
            .Location = New Point(230,52),
            .Size = New Size(100,24),
            .Text = "Start client"}
        
        BTDisconnectClient = new button With {
            .Location = New Point(331,52),
            .Size = New Size(100,24),
            .Text = "Stop client"}
        
        BTMessageTest = New Button With {
            .Location = New Point(230,77),
            .Size = New Size(201,24),
            .Text = "Send test message"}
        
        BTBeginEnduranceIntegrityTest = New Button With {
            .Location = New Point(230,102),
            .Size = New Size(201,24),
            .Text = "Begin connection integrity test"}
        
        BTEndEnduranceIntegrityTest = New Button With {
            .Location = New Point(230,127),
            .Size = New Size(201,24),
            .Text = "End connection integrity test"}
        
        Controls.Add(LBServerAddress)
        Controls.Add(TXServerAddress)
        Controls.Add(LBServerPort)
        Controls.Add(TXServerPort)
        Controls.Add(BTStartServer)
        Controls.Add(BTStopServer)
        Controls.Add(LBClientAddress)
        Controls.Add(TXClientAddress)
        Controls.Add(LBClientPort)
        Controls.Add(TXClientPort)
        Controls.Add(BTConnectClient)
        Controls.Add(BTDisconnectClient)
        Controls.Add(BTMessageTest)
        Controls.Add(BTBeginEnduranceIntegrityTest)
        Controls.Add(BTEndEnduranceIntegrityTest)
        ResumeLayout()
    End Sub
    
    Private Sub EVStartServer() Handles BTStartServer.Click
        Dim Endpoint As New Net.IPEndPoint(net.IPAddress.Parse(TXServerAddress.Text),Convert.ToInt32(TXServerPort.Text))
        Server.Start(Endpoint)
    End sub
    
    Private Sub EVStopServer() Handles BTStopServer.Click
        Server.Stop()
    End Sub
    
    Private Sub EVConnectClient() Handles BTConnectClient.Click
        Dim Endpoint As New Net.IPEndPoint(net.IPAddress.Parse(TXClientAddress.Text),Convert.ToInt32(TXClientPort.Text))
        Client.Connect(Endpoint)
    End Sub
    
    Private Sub EVDisconnectClient() Handles BTDisconnectClient.Click
        Client.Disconnect()
    End Sub
    
    Private Sub EVSendTestMessage() Handles  BTMessageTest.Click
       Console.WriteLine(Client.CommandTest())
    End Sub
    
    Private sub EVBeginIntegrityTest() Handles  BTBeginEnduranceIntegrityTest.Click
        Console.WriteLine(Client.IntegrityTestBegin())
    End sub
    Private sub EVEndIntegrityTest() Handles BTEndEnduranceIntegrityTest.Click
        Console.WriteLine(Client.IntegrityTestEnd())
    End sub
End Class