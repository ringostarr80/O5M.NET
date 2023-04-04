![license](https://img.shields.io/github/license/ringostarr80/O5M.NET)
![codeql_analysis](https://img.shields.io/github/actions/workflow/status/ringostarr80/O5M.NET/codeql-analysis.yml)
![nuget_version](https://img.shields.io/nuget/v/O5MLibrary)
![github_tag](https://img.shields.io/github/v/tag/ringostarr80/O5M.NET?sort=semver)

# O5M.NET
This project provides reading and writing capabilities of O5M Data.

## build and publish nupkg file
```sh
dotnet pack --configuration=Release
dotnet nuget push bin/Release/O5MLibrary.<version>.nupkg --api-key=<api-key>
```
