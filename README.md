# Networking
High performance backend for fully encrypted message based TCP networking.

How to use:
 + Refrence Networking.dll in your project.
 + Inherit from Networking.ClientBase for a client, or Networking.ServerBase for a server.
 + Override Sub Run(Client as Networking.TCPClient).
 + Use Client.ReadJagged(Byref Output as byte()()) to receive data, and Client.WriteJagged(Byref Input as byte()()) to send data. Both methods are thread safe!
 + Networking.ThreadLimiter instances can be used as handy loop governors.
 + Add your code!

Features:
 + Keyless AES-256 encryption
 + Simple and efficent design
 + High performance
