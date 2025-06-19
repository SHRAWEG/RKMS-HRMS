START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230515103019_updateAtt') THEN
    ALTER TABLE "ATTENDANCE_LOG_NO_DIRECTION" ADD "DEVICE_IP" varchar(255) NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230515103019_updateAtt') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230515103019_updateAtt', '6.0.10');
    END IF;
END $EF$;
COMMIT;

