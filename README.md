![license](https://img.shields.io/github/license/ringostarr80/O5M.NET)
[![CodeQL](https://github.com/ringostarr80/O5M.NET/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/ringostarr80/O5M.NET/actions/workflows/github-code-scanning/codeql)
[![.NET Build & Test](https://github.com/ringostarr80/O5M.NET/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ringostarr80/O5M.NET/actions/workflows/dotnet.yml)
![nuget_version](https://img.shields.io/nuget/v/O5MLibrary)
![github_tag](https://img.shields.io/github/v/tag/ringostarr80/O5M.NET?sort=semver)
[![codecov](https://codecov.io/gh/ringostarr80/O5M.NET/graph/badge.svg?token=7J64CBACA6)](https://codecov.io/gh/ringostarr80/O5M.NET)

# O5M.NET
This project provides reading and writing capabilities of O5M Data.

## build and publish nupkg file
```sh
dotnet pack --configuration=Release
dotnet nuget push bin/Release/O5MLibrary.<version>.nupkg --api-key=<api-key>
```
