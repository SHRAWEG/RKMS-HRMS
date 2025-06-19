START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727074421_increaseCharLength') THEN
    ALTER TABLE "EMP_DETAIL" ALTER COLUMN "EMP_TDISTRICT" TYPE varchar(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727074421_increaseCharLength') THEN
    ALTER TABLE "EMP_DETAIL" ALTER COLUMN "EMP_PDISTRICT" TYPE varchar(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727074421_increaseCharLength') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230727074421_increaseCharLength', '6.0.10');
    END IF;
END $EF$;
COMMIT;

