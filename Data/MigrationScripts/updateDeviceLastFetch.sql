START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230602052226_updateDeviceLastFetch') THEN
    ALTER TABLE "DEVICE_SETTINGS" DROP COLUMN "LAST_FETCHED_AT";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230602052226_updateDeviceLastFetch') THEN
    ALTER TABLE "DEVICE_SETTINGS" ADD "LAST_FETCHED_DATE" date NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230602052226_updateDeviceLastFetch') THEN
    ALTER TABLE "DEVICE_SETTINGS" ADD "LAST_FETCHED_TIME" time without time zone NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230602052226_updateDeviceLastFetch') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230602052226_updateDeviceLastFetch', '6.0.10');
    END IF;
END $EF$;
COMMIT;

