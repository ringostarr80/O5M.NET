# O5M.NET
This project provides reading and writing capabilities of O5M Data.

## build and publish nupkg file
```sh
dotnet pack --configuration=Release
dotnet nuget push bin/Release/O5MLibrary.<version>.nupkg --api-key=<api-key>
```
