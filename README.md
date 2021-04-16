# Networking
High performance backend for fully encrypted message based TCP networking.

Usage:
 + Refrence Networking.dll in your project.
 + Inherit from Networking.ClientBase for a client, or Networking.ServerBase for a server.
 + Override Sub Run(Client as Networking.TCPClient).
 + Networking.ThreadLimiter instances can be used as handy loop governors.
 + Add your code!
