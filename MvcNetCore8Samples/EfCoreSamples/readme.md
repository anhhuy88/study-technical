- Interceptor demo with EF Core
- Concurrent update demo with EF Core
- 


# Versions
- Ver_01: Initial version
- Ver_02: Add FileData table
    + dotnet ef migrations add Ver_02
    + scripts: dotnet ef migrations script Ver_01 Ver_02 -o Scripts/Ver_02.sql
    + dotnet ef database update

# Migration
- dotnet ef migrations add Ver_01
- dotnet ef database update
