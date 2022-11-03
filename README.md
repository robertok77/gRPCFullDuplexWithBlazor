## gRPC full duplex with Blazor Wasm hosted
### Demo demonstrates full duplex gRPC streaming data in parallel. 
- Generated prime numbers from client sieve are streaming to server sieve. 
- In parallel, server sieve is calculated and data are streaming to client ui. 
### _You can see both working streams on the page._
    
![Web Blazor Client V](https://user-images.githubusercontent.com/14275269/199564909-0ff9897c-27aa-4d63-a924-b7f0e45a6fdf.png)

#### Some of used packages
 - gRPC.Net
 - MediatR
   - Query, Command
   - Fluent Validation 
   - Logging Behaviour
   - Exception Handler
 - Serilog with Seq Server
 - Fluent Assertion
 - BUnit.Web
 - Test Server
 - Moq, xUnit
 
