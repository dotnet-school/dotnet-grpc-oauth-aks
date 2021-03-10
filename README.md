

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

  

- 