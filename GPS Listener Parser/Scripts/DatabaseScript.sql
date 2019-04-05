-- Create empty database GPS_Tracking then run this script

USE [GPS_Tracking]
GO
/****** Object:  Table [dbo].[GPS_Log]    Script Date: 4/6/2019 12:56:43 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GPS_Log](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DeviceId] [varchar](50) NOT NULL,
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
/****** Object:  Table [dbo].[GPS_Real]    Script Date: 4/6/2019 12:56:43 AM ******/
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
/****** Object:  Trigger [dbo].[GPS_Log_trigger]    Script Date: 4/6/2019 12:56:43 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[GPS_Log_trigger]
   ON  [dbo].[GPS_Real]
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
   INSERT INTO [dbo].[GPS_Log] ([DeviceId]
           ,[ServerTimestamp]
           ,[Long]
           ,[Lat]
           ,[Altitude]
           ,[Direction]
           ,[Satellites]
           ,[Speed]) 
    SELECT i.[DeviceId]
           ,i.[ServerTimestamp]
           ,i.[Long]
           ,i.[Lat]
           ,i.[Altitude]
           ,i.[Direction]
           ,i.[Satellites]
           ,i.[Speed]
    FROM Inserted i
    
END
GO
ALTER TABLE [dbo].[GPS_Real] ENABLE TRIGGER [GPS_Log_trigger]
GO