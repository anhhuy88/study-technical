BEGIN TRANSACTION;

CREATE TABLE "FileDatas" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FileDatas" PRIMARY KEY,
    "ContentType" TEXT NULL,
    "FileName" TEXT NULL,
    "Data" BLOB NULL,
    "CreatedDate" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241125031831_Ver_02', '8.0.7');

COMMIT;



--CREATE TABLE "FileDatas" (
--    "Id" TEXT NOT NULL CONSTRAINT "PK_FileDatas" PRIMARY KEY,
--    "ContentType" TEXT NULL,
--    "FileName" TEXT NULL,
--    "Data" BLOB NULL,
--    "CreatedDate" TEXT NOT NULL
--);