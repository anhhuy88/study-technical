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

# Image uploads and database storage
- Both upload endpoints try ImgBB first for normal images. Configure `ImgBBKEY` securely (for example via a systemd environment variable); do not commit the key. A missing key, timeout (20 seconds), network failure or invalid/error response causes a warning and falls back to the database. Request cancellation is not treated as an upload failure.
- ImgBB receives the original image. An HTTP success response with a non-null, non-empty string `data.url` returns that value unchanged (not `display_url` or `url_viewer`); its scheme, whitespace and the response's `success` flag are not checked. A multipart upload response may contain a mix of external and local URLs in input order. Logo images are always stored locally.
- Multipart files are validated before external uploads. External uploads cannot be rolled back if a later upload, database save, or request fails, so orphaned ImgBB images may need manual cleanup. ImgBB controls the lifetime and availability of its hosted images.
- Both upload endpoints process new JPEG, PNG, GIF and WebP images before saving them. Images keep their format, aspect ratio, transparency and animation; their longest side is limited to 1600 pixels without enlarging smaller images. JPEG and WebP use quality 82, or 92 for `Logo`.
- Non-image files, unsupported image formats, animated PNG (APNG), and invalid image data are rejected. The multipart endpoint retains its 2 MB per-file upload limit.
- Image bytes, content type, file name and size reflect the stored result. If no resize or orientation correction is needed and re-encoding is not smaller, the original is stored. Existing database records are not changed.
- Run the image and upload tests with `dotnet test ..\CommonWebAPI.Tests\CommonWebAPI.Tests.csproj` from this project directory.

# File cleanup
- When `FileCleanup:Enabled` is true, database and image-cache cleanup may start only from 22:00 inclusive until 03:00 exclusive, in the server's local time zone.
- The first cleanup starts when the service starts within that window, or waits until 22:00. Subsequent runs are at least `FileCleanup:IntervalHours` apart, measured from their start; a run due outside the window waits until the next window. Restarting the application resets the interval.
- A cleanup already started may finish after 03:00. Database cleanup excludes `Logo` files and uses `FileCleanup:RetentionDays`; image-cache cleanup uses `FileCleanup:ImageCacheRetentionDays`.
