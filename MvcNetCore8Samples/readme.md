<!--TOC-->
- [Versions DB](#versions-db)
- [Using barcode](#using-barcode)
- [Using serilog](#using-serilog)
- [Create Area in library project](#create-area-in-library-project)
- [using admin theme](#using-admin-theme)
<!--/TOC-->

# Versions DB
- Ver_01: Initial version
    + dotnet ef migrations add Ver_01
    + scripts: dotnet ef migrations script Ver_01 -o Scripts/Ver_01.sql
    + dotnet ef database update

# Using barcode
- Package ZXing.Net
``` XML
<PackageReference Include="ZXing.Net.Bindings.ZKWeb.System.Drawing" Version="0.16.7" />
```
- Implement: BaCodeController.Generation

# Using serilog

- Package Serilog
``` XML
<PackageReference Include="Serilog.AspNetCore" Version="8.0.2" />
```
- Appsettings
``` JSON
  "Serilog": {
    "Using": [ "Serilog.Sinks.File", "Serilog.Sinks.Console" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "rollOnFileSizeLimit": true,
          "fileSizeLimitBytes": 25000000, // 5kb = 5_000, 1MB = 1_000_000, 25MB = 25_000_000
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithThreadId", "WithMachineName" ]
  }
```

- program.cs
``` C#
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
```

# Create Area in library project
- Create new project Razor Class Library
- Go to the .csproj file and then modify it as follows.:
  + **Sdk="Microsoft.NET.Sdk.Razor"**
  + **AddRazorSupportForMvc: true**
  + **FrameworkReference inclue: Microsoft.AspNetCore.App**
``` XML
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

</Project>

```

# using admin theme
- https://github.com/themekita/Atlantis-Lite?tab=readme-ov-file