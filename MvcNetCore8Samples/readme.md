<!--TOC-->
- [Using barcode](#using-barcode)
- [Using serilog](#using-serilog)
<!--/TOC-->

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