CREATE TABLE [dbo].[WeatherForecast]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Forecast] VARCHAR(50) NULL, 
    [Date] DATE NULL, 
    [Temperature] INT NULL
)
