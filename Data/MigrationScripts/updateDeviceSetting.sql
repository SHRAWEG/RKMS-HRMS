START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230601104215_updateDeviceSetting') THEN
    ALTER TABLE "DEVICE_SETTINGS" ADD "LAST_FETCHED_AT" timestamp with time zone NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230601104215_updateDeviceSetting') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230601104215_updateDeviceSetting', '6.0.10');
    END IF;
END $EF$;
COMMIT;

