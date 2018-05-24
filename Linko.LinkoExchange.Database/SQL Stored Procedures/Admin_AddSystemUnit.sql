SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM [dbo].[sysobjects] WHERE id = OBJECT_ID(N'dbo.Admin_AddSystemUnit') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[Admin_AddSystemUnit]
GO

-- =============================================
-- Author:		Rajeeb Saha
-- Create date: 2018-05-23
-- Description:	Add new system unit
-- =============================================
CREATE PROCEDURE Admin_AddSystemUnit
	(
		@UnitName varchar(50),
		@Description varchar(500),
		@UnitDimensionName varchar(50),
		@ConversionFactor float,
		@AdditiveFactor float
	)
AS
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;
	
BEGIN TRY
	BEGIN TRANSACTION
	
	IF NOT EXISTS ( SELECT TOP 1 * FROM [dbo].[tUnitDimension] WHERE Name = @UnitDimensionName)
	BEGIN
		INSERT INTO [dbo].[tUnitDimension] ([Name],[Description]) VALUES(@UnitDimensionName, @Description )
	END

	DECLARE @UnitDimensionId int
    SELECT @UnitDimensionId = UnitDimensionId 
    FROM [dbo].[tUnitDimension]  
    WHERE Name LIKE @UnitDimensionName;

	IF NOT EXISTS ( SELECT TOP 1 * FROM [dbo].[tSystemUnit] WHERE Name = @UnitName)
	BEGIN
		INSERT INTO [dbo].[tSystemUnit]
				   (
						[Name]
					   ,[Description]
					   ,[UnitDimensionId]
					   ,[ConversionFactor]
					   ,[AdditiveFactor]
				   )
			 VALUES
				   (
					    @UnitName
					   ,@Description
					   ,@UnitDimensionId
					   ,@ConversionFactor
					   ,@AdditiveFactor
				   )
	END

	PRINT SCOPE_IDENTITY();
	COMMIT TRANSACTION

END TRY

BEGIN CATCH

    DECLARE @ErrorMessage nvarchar(4000);
    DECLARE @ErrorSeverity int;
    DECLARE @ErrorState int;
    DECLARE @ErrorProcedure nvarchar(1000);
    DECLARE @ErrorNumber int;
    DECLARE @ErrorLine int;

	--if using XACT_ABORT ON setting, need to check xact_state
	IF (XACT_STATE() = -1) 
    BEGIN 
        ROLLBACK TRAN 
    END


    SELECT @ErrorNumber = ERROR_NUMBER()
        , @ErrorSeverity = ERROR_SEVERITY()
        , @ErrorState = ERROR_STATE()
        , @ErrorProcedure = ERROR_PROCEDURE()
        , @ErrorLine = ERROR_LINE()
        , @ErrorMessage = ERROR_MESSAGE() + ' in LinkoExchange procedure ' + CONVERT(varchar(100), ISNULL(ERROR_PROCEDURE(), '-none-')) + ' at line ' + CONVERT(varchar(5), ERROR_LINE()); 
				
	RAISERROR (@ErrorMessage, @ErrorSeverity, 1)

END CATCH