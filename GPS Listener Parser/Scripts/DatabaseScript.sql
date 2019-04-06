USE [GPS_Tracking]
GO
/****** Object:  Table [dbo].[GPS_Log]    Script Date: 4/6/2019 8:02:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GPS_Log](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DeviceId] [varchar](50) NOT NULL,
	[DeviceTimeStamp] [datetime2](7) NULL,
	[ServerTimestamp] [datetime2](7) NULL,
	[Long] [decimal](12, 9) NULL,
	[Lat] [decimal](12, 9) NULL,
	[Altitude] [int] NULL,
	[Direction] [int] NULL,
	[Satellites] [int] NULL,
	[Speed] [int] NULL,
 CONSTRAINT [PK_GPS_Log] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GPS_Real]    Script Date: 4/6/2019 8:02:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GPS_Real](
	[DeviceId] [varchar](50) NOT NULL,
	[ServerTimestamp] [datetime2](7) NULL,
	[DeviceTimeStamp] [datetime2](7) NULL,
	[Long] [decimal](12, 9) NULL,
	[Lat] [decimal](12, 9) NULL,
	[Altitude] [int] NULL,
	[Direction] [int] NULL,
	[Satellites] [int] NULL,
	[Speed] [int] NULL,
 CONSTRAINT [PK_GPS_Real] PRIMARY KEY CLUSTERED 
(
	[DeviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[SaveGPSpointFMXXXX]    Script Date: 4/6/2019 8:02:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SaveGPSpointFMXXXX] 
	-- Add the parameters for the stored procedure here
    @priority TINYINT ,
    @device_id CHAR(15) ,
    @latitude FLOAT ,
    @longitude FLOAT ,
    @altitude SMALLINT ,
    @speed SMALLINT ,
    @direction SMALLINT ,
    @satellites TINYINT ,
    @rtc_time DATETIME2
AS 
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON ;

-- CHECK IF IS A REAL POSITION POINT OR LOGED POINT FROM DEVICE     
IF EXISTS (SELECT 1 FROM dbo.GPS_Real where  DeviceId = @device_id)
  BEGIN
    -- TRY UPDATE  table row
                UPDATE  dbo.GPS_Real
                SET     Lat = @latitude ,
                        Long = @longitude ,
                        Altitude = @altitude ,
                        Speed = @speed ,
                        Direction = @direction ,
                        [Satellites] = @satellites ,
                        DeviceTimeStamp = @rtc_time,
						ServerTimestamp = GETDATE()
                WHERE   DeviceId = @device_id AND DeviceTimeStamp < @rtc_time
                
   END
   ELSE
   BEGIN
       -- CHECK IF TABLE ROW NOT UPDATES BECAUSE NOT EXISTS THEN INSERT
                     BEGIN
                        INSERT  INTO dbo.GPS_Real
                                ( DeviceId ,
                                  Long ,
                                  Lat ,
                                  Altitude ,
                                  Direction ,
                                  Satellites ,
                                  Speed ,
                                  DeviceTimeStamp ,
								  ServerTimestamp		          
		                    )
                        VALUES  ( @device_id , -- DeviceId - char(15)
                                  @longitude , -- Long - float
                                  @latitude , -- Lat - float
                                  @altitude , -- Altitude - smallint
                                  @direction , -- Direction - smallint
                                  @satellites , -- Satellites - tinyint
                                  @speed , -- Speed - smallint
                                  @rtc_time , -- DeviceTimeStamp - datetime	
								  GETDATE()	          
		                    )			
                    END
        
END
    -- AFTER UPDATE OR ADD ROW IN GPS_REAL INSERT ROW TO GPS_LOG TABLE
        INSERT  INTO dbo.GPS_Log
                ( DeviceId ,
                  Long ,
                  Lat ,
                  Altitude ,
                  Direction ,                  
                  Satellites ,
                  Speed ,
                  DeviceTimeStamp,
				  ServerTimestamp 	          
	          )
        VALUES  ( @device_id , -- DeviceId - char(15)
                  @longitude , -- Long - float
                  @latitude , -- Lat - float
                  @altitude , -- Altitude - smallint
                  @direction , -- Direction - smallint
                  @satellites , -- Satellites - tinyint
                  @speed , -- Speed - smallint
                  @rtc_time , -- DeviceTimeStamp - datetime		
				  GETDATE()
	          ) 
    END


GO
