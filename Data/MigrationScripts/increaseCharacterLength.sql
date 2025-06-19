START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727055742_increaseCharacterLength') THEN
    ALTER TABLE "EMP_FAMILY" DROP COLUMN "IS_DEPENDENT";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727055742_increaseCharacterLength') THEN
    ALTER TABLE "EMP_DETAIL" ALTER COLUMN "EMP_MIDDLENAME" TYPE varchar(30);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727055742_increaseCharacterLength') THEN
    ALTER TABLE "EMP_DETAIL" ALTER COLUMN "EMP_LASTNAME" TYPE varchar(30);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727055742_increaseCharacterLength') THEN
    ALTER TABLE "EMP_DETAIL" ALTER COLUMN "EMP_FIRSTNAME" TYPE varchar(30);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20230727055742_increaseCharacterLength') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20230727055742_increaseCharacterLength', '6.0.10');
    END IF;
END $EF$;
COMMIT;

