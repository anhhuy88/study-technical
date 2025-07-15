# Deploy
- Step 1. Publish
- Step 2. Create systemctl service
- Step 3. Create nginx service
- Step 4. Run generate SSL
- Step 5. Permission for database or directory
- Step 6. Start systemctl service
- Step 7. ReStart nginx service
- Step 8. Restart systemctl service
- 
# Versions DB
- Ver_01: Initial version
    + dotnet ef migrations add Ver_01
    + scripts: dotnet ef migrations script Ver_01 -o Scripts/Ver_01.sql
    + dotnet ef database update

