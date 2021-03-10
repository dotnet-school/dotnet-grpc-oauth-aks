

- Create project

  ```bash
  mkdir GrpcOAuth
  cd GrpcOAuth
  
  # Create a solution 
  dotnet new sln 
  
  # Create a grpc server
  dotnet new grpc -o GrpcServer
  dotnet sln add GrpcServer/GrpcServer.csproj
  
  # Create a web server
  dotnet new web -o WebServer
  dotnet sln add WebServer/WebServer.csproj
  
  # Create git ignore
  dotnet new gitignore
  
  # Commit "Created project"
  ```

  

- Add grpc client library to WebServer

  ```bash
  dotnet add WebServer/WebServer.csproj package Grpc.Net.Client
  dotnet add WebServer/WebServer.csproj package Google.Protobuf
  dotnet add WebServer/WebServer.csproj package Grpc.Tools
  ```

  

- Copy server proto files to client project

  ```bash
  cp -r GrpcServer/Protos WebServer/Protos
  ```

- Update `WebServer/WebServer.csproj` to add protos

  ```xml
  <ItemGroup>
    <Protobuf Include="Protos\greet.proto" GrpcServices="Client" />
  </ItemGroup>
  ```

  

- In Startup.cs : 

  ```csharp
  app.UseEndpoints(endpoints =>
  	{
      endpoints.MapGet("/", async context =>
  			{
          using var channel = GrpcChannel.ForAddress(
            							Environment.GetEnvironmentVariable("GrpcServer"));
  
          // Greeter service is defined in hello.proto
          // <service-name>.<service-name>Client is auto-created
          var client = new Greeter.GreeterClient(channel);
  
          // HelloRequest is defined in hello.proto
          var request = new HelloRequest();
          request.Name = "Nishant";
  
          // SayHello method is defined in hello.proto
          var response = client.SayHello(request);
  
          await context.Response.WriteAsync(response.Message);
        });
    });
  ```

  

- Create dockerfile for server 

  ```bash
  cd GrpcServer
  docker build -t grpc-server .
  ```

- 