## 1. REQUIREMENT
- Add proto file GrpcFileReceiver project
- Define GrpcFileService to proto file
- Copy proto file to GrpcFileSender project
<br><br>


<!--------------------------------------------------------------------------------------------------->
## 2. WORK NOTE

### 2.1. ADD PROTO FILE TO SERVER PROJECT

Add GrpcFileService proto file to GrpcFileReceiver project.
![Image](https://github.com/user-attachments/assets/4e041916-2a58-48be-8cc7-ed3a8a4e7763)


Define gRPC service and message as follows:
```proto
syntax = "proto3";

option csharp_namespace = "Apps.SKD.GrpcFileReceiver.Protos";

service GrpcFileService {
    rpc UploadFile (stream FileUploadRequest) returns (UploadStatus);
}

message FileUploadRequest {
    string file_name = 1;
    bytes content = 2;
}

message UploadStatus {
    string status = 1;
}
```
<br>


### 2.2. MODIFY SERVER PROJECT SETTING FOR APPLYING NEW PROTO BUFFER

```xml
  <ItemGroup>
      <Protobuf Include="Protos\greet.proto" GrpcServices="Server" />
      <Protobuf Include="Protos\GrpcFileService.proto" GrpcServices="Server" />
  </ItemGroup>
```
<br>


### 2.3. COPY PROTO FILE TO CLIENT PROJECT

**(1) COPY PROTO FILE**

![Image](https://github.com/user-attachments/assets/f663f4ba-8a4e-4af1-8ceb-f6a464c3e5ae)


**(2) ADD gRPC RELATED NuGet PACKAGE**
```xml
  <ItemGroup>
      <PackageReference Include="SKD.Utility" Version="1.1.42" />
      <PackageReference Include="Google.Protobuf" Version="3.21.5" />
      <PackageReference Include="Grpc.Net.ClientFactory" Version="2.49.0" />
      <PackageReference Include="Grpc.Tools" Version="2.49.0">
          <PrivateAssets>all</PrivateAssets>
          <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>  
  </ItemGroup>
```

**(3) MODIFY GrpcFileService as CLIENT**
```xml
  <ItemGroup>
    <None Update="Protos\GrpcFileService.proto">
      <GrpcServices>Client</GrpcServices>
    </None>
  </ItemGroup>
```


<br>




<!--------------------------------------------------------------------------------------------------->
## 3. REMAIN PROBLEM
- nothing