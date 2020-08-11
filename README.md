# NetSyncLib
NOT TESTED AND NOT WORKING!!!
Please do NOT use this!!
I never had the time to fix this mess of a codebase.
## Usage
A property with the [NetSynchronize] attribute that is part of an object that has been registered will automatically duplicate itself to all clients and update itself on all clients.
[NetSynchronize(0, NetSynchronizeDeliveryMethod.Unreliable)] where the 0 here is a time in ms. If the last check was less ms ago than the given time here than the update will not happen.

Static methods with the [NetValueHandler] attribute will be scanned for and run before the actual program starts. They should contain all possible values the Lib should be able to duplicate. If the value is a netobject the references will be duplicated on the client like they are on the server. If there is an object that's not a netobject and that's type has not been handled by a handler an exception is thrown. 

Refer to the NetSyncLib.Tests or the NetSyncConsoleTester as examples
