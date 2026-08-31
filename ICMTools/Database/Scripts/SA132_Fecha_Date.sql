SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    SA132: guardar Fecha solo como fecha, sin componente de hora.
    Tablas afectadas:
    - dbo.STG_INGRESOSSIX
    - dbo.BDIINGRESOSSIX
*/

BEGIN TRY
    BEGIN TRAN;

    IF EXISTS (
        SELECT 1
        FROM sys.key_constraints
        WHERE name = 'PK_BDIINGRESOSSIX'
          AND parent_object_id = OBJECT_ID('dbo.BDIINGRESOSSIX')
    )
    BEGIN
        ALTER TABLE dbo.BDIINGRESOSSIX DROP CONSTRAINT PK_BDIINGRESOSSIX;
    END

    ALTER TABLE dbo.STG_INGRESOSSIX
        ALTER COLUMN Fecha date NOT NULL;

    ALTER TABLE dbo.BDIINGRESOSSIX
        ALTER COLUMN Fecha date NOT NULL;

    ALTER TABLE dbo.BDIINGRESOSSIX
        ADD CONSTRAINT PK_BDIINGRESOSSIX PRIMARY KEY CLUSTERED (
            Fecha,
            CeBeCategoria,
            CeBe,
            Categoria,
            SumaML
        );

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    THROW;
END CATCH
