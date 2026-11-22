CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "Itineraries" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Itineraries" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NULL,
    "Description" TEXT NULL,
    "Theme" TEXT NULL,
    "Status" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    "GeographicArea" TEXT NULL
);

CREATE TABLE "Stops" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Stops" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "Latitude" REAL NOT NULL,
    "Longitude" REAL NOT NULL
);

CREATE TABLE "ItineraryStops" (
    "ItineraryId" INTEGER NOT NULL,
    "StopId" INTEGER NOT NULL,
    "Position" INTEGER NOT NULL,
    CONSTRAINT "PK_ItineraryStops" PRIMARY KEY ("ItineraryId", "StopId"),
    CONSTRAINT "FK_ItineraryStops_Itineraries_ItineraryId" FOREIGN KEY ("ItineraryId") REFERENCES "Itineraries" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ItineraryStops_Stops_StopId" FOREIGN KEY ("StopId") REFERENCES "Stops" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Itineraries_Status" ON "Itineraries" ("Status");

CREATE INDEX "IX_ItineraryStops_ItineraryId_Position" ON "ItineraryStops" ("ItineraryId", "Position");

CREATE INDEX "IX_ItineraryStops_StopId" ON "ItineraryStops" ("StopId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261119160016_InitialCreate', '8.0.11');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "ef_temp_Stops" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Stops" PRIMARY KEY AUTOINCREMENT,
    "Address" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Latitude" REAL NOT NULL,
    "Longitude" REAL NOT NULL,
    "Name" TEXT NOT NULL,
    CONSTRAINT "CK_Stops_Address_NotEmpty" CHECK (length(trim("Address")) > 0),
    CONSTRAINT "CK_Stops_Latitude" CHECK ("Latitude" BETWEEN -90 AND 90),
    CONSTRAINT "CK_Stops_Longitude" CHECK ("Longitude" BETWEEN -180 AND 180),
    CONSTRAINT "CK_Stops_Name_NotEmpty" CHECK (length(trim("Name")) > 0)
);

INSERT INTO "ef_temp_Stops" ("Id", "Address", "Description", "Latitude", "Longitude", "Name")
SELECT "Id", "Address", "Description", "Latitude", "Longitude", "Name"
FROM "Stops";

CREATE TABLE "ef_temp_ItineraryStops" (
    "ItineraryId" INTEGER NOT NULL,
    "StopId" INTEGER NOT NULL,
    "Position" INTEGER NOT NULL,
    CONSTRAINT "PK_ItineraryStops" PRIMARY KEY ("ItineraryId", "StopId"),
    CONSTRAINT "CK_ItineraryStops_Position" CHECK ("Position" >= 0),
    CONSTRAINT "FK_ItineraryStops_Itineraries_ItineraryId" FOREIGN KEY ("ItineraryId") REFERENCES "Itineraries" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ItineraryStops_Stops_StopId" FOREIGN KEY ("StopId") REFERENCES "Stops" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_ItineraryStops" ("ItineraryId", "StopId", "Position")
SELECT "ItineraryId", "StopId", "Position"
FROM "ItineraryStops";

CREATE TABLE "ef_temp_Itineraries" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Itineraries" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" TEXT NOT NULL,
    "Description" TEXT NULL,
    "GeographicArea" TEXT NULL,
    "Status" TEXT NOT NULL,
    "Theme" TEXT NULL,
    "Title" TEXT NULL,
    "UpdatedAt" TEXT NOT NULL,
    CONSTRAINT "CK_Itineraries_Status" CHECK ("Status" IN ('DRAFT', 'PUBLISHED')),
    CONSTRAINT "CK_Itineraries_UpdatedAt" CHECK ("UpdatedAt" >= "CreatedAt")
);

INSERT INTO "ef_temp_Itineraries" ("Id", "CreatedAt", "Description", "GeographicArea", "Status", "Theme", "Title", "UpdatedAt")
SELECT "Id", "CreatedAt", "Description", "GeographicArea", "Status", "Theme", "Title", "UpdatedAt"
FROM "Itineraries";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Stops";

ALTER TABLE "ef_temp_Stops" RENAME TO "Stops";

DROP TABLE "ItineraryStops";

ALTER TABLE "ef_temp_ItineraryStops" RENAME TO "ItineraryStops";

DROP TABLE "Itineraries";

ALTER TABLE "ef_temp_Itineraries" RENAME TO "Itineraries";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_ItineraryStops_ItineraryId_Position" ON "ItineraryStops" ("ItineraryId", "Position");

CREATE INDEX "IX_ItineraryStops_StopId" ON "ItineraryStops" ("StopId");

CREATE INDEX "IX_Itineraries_Status" ON "Itineraries" ("Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261119160507_AddEntityConstraints', '8.0.11');

COMMIT;

