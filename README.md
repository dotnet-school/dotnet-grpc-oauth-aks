Todo

- [x] Create client and server as docker containers
- [ ] Create minikube local cluster
- [ ] Deploy on AKS
- [ ] Add OAuth 



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

  

### Create dockerfile for server 

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /source

COPY ./*.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal
WORKDIR /app

COPY --from=build /app .

EXPOSE 80  
ENTRYPOINT ["dotnet", "GrpcServer.dll"]
```

Create docker image : 

```bash
cd GrpcServer
docker build -t grpc-server .

# Run and check server
docker run -p 5001:80 grpc-server
```



### Create grpc client in web-server

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



- Add a dockerfile to web-server

  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
  WORKDIR /source
  
  COPY ./*.csproj .
  RUN dotnet restore
  
  COPY . .
  RUN dotnet publish -c release -o /app --no-restore
  
  FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal
  WORKDIR /app
  
  COPY --from=build /app .
  
  EXPOSE 80  
  ENTRYPOINT ["dotnet", "WebServer.dll"]
  ```

  

- Create docker file 

  ```bash
  docker build -t web-server .
  
  # Run and check
  docker run -p 5002:80 -e GrpcServer="http://host.docker.internal:5001" web-server
  ```

  

### Create kuberntes cluster

- Push images to docker registry 

  ```bash
  # Login to docker hub
  docker login
  
  # Replace nishants with your dockerhub user name
  
  docker tag web-server nishants/web-server:v0.1 
  docker push nishants/web-server:v0.1 
  
  docker tag grpc-server nishants/grpc-server:v0.1 
  docker push nishants/grpc-server:v0.1 
  ```

  **Login to docker hub and ensure that images are public**

  

- Create manifest for web-server in `WebServer/k8s.yml`

  ```yaml
  # This part creates a load balancer pod that receives traffic from
  # internet and load-balances to our pods
  apiVersion: v1
  kind: Service
  metadata:
    name: web-server-service
  spec:
    selector:
      app: web-server     # This makes load balancer point to web-server deployment
    ports:
      - port: 80
        targetPort: 80  # The port our container(in pods) listens to
    type: LoadBalancer
  ---
  # This part creates a pod that runs our docker image
  apiVersion: apps/v1
  kind: Deployment
  metadata:
    name: web-server
  spec:
    # Keep two replicas of our app
    replicas: 2
    selector:
      matchLabels:
        app: web-server
    template:
      metadata:
        labels:
          app: web-server
      spec:
        containers:
          - name: web-server
            image: nishants/web-server:v0.1  # Our docker image on docker hub
            ports:
              - containerPort: 80           # Port that our app listens to
            env:
              - name: GrpcServer
                value: http://grpc-server-service:5001
            imagePullPolicy: Always
  ```

  

- Create manigest for grpc server in `GrpcServer/k8s.yml`

  ```yaml
  apiVersion: v1
  kind: Service
  metadata:
    name: grpc-server-service   # Name of service (we use this as domain in todo-app config)
  spec:
    selector:
      app: grpc-server       # Exposes stream-web-app as service
    ports:
      - port: 5001
        targetPort: 80 # Map service port to container port
  ---
  apiVersion: apps/v1
  kind: Deployment
  metadata:
    name: grpc-server-deployment     # Name of deployment, we wil refer this in service
  spec:
    replicas: 2
    selector:
      matchLabels:
        app: grpc-server
    template:
      metadata:
        labels:
          app: grpc-server
      spec:
        containers:
          - name: grpc-server
            image: nishants/grpc-server:v0.1   # Image name for stream-web-app container
            ports:
              - containerPort: 80
  ```



- Create a local cluster 

  ```bash
  # Start a single node cluster locally
  minikube start
  
  # Check our local cluster
  kubectl cluster-info
  
  kubectl apply -f GrpcServer/k8s.yml
  kubectl apply -f WebServer/k8s.yml
  
  # Check configuration 
  minikube service web-server-service
  ```

  