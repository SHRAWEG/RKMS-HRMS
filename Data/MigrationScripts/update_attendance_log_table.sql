START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230531094200_update_attendance_log_table') THEN
    ALTER TABLE "EMP_DEVICE_CODE" DROP CONSTRAINT "FK_EMP_DEVICE_CODE_DEVICE_SETTINGS_DEVICE_ID";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230531094200_update_attendance_log_table') THEN
    ALTER TABLE "ATTENDANCE_LOG" DROP COLUMN "EMP_CODE";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230531094200_update_attendance_log_table') THEN
    ALTER TABLE "EMP_DEVICE_CODE" ALTER COLUMN "DEVICE_ID" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230531094200_update_attendance_log_table') THEN
    ALTER TABLE "EMP_DEVICE_CODE" ADD "LAST_FETCHED_AT" timestamp with time zone NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230531094200_update_attendance_log_table') THEN
    ALTER TABLE "ATTENDANCE_LOG" ADD "DEVICE_CODE" varchar(30) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230531094200_update_attendance_log_table') THEN
    ALTER TABLE "EMP_DEVICE_CODE" ADD CONSTRAINT "FK_EMP_DEVICE_CODE_DEVICE_SETTINGS_DEVICE_ID" FOREIGN KEY ("DEVICE_ID") REFERENCES "DEVICE_SETTINGS" ("ID");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230531094200_update_attendance_log_table') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230531094200_update_attendance_log_table', '6.0.10');
    END IF;
END $EF$;
COMMIT;

