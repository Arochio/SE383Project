IF DB_ID(N'ClickerEmpire') IS NULL
BEGIN
    CREATE DATABASE [ClickerEmpire];
END;
GO

USE [ClickerEmpire];
GO

IF OBJECT_ID(N'dbo.PlayerData', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PlayerData
    (
        PlayerId nvarchar(450) NOT NULL,
        State nvarchar(max) NOT NULL,
        CONSTRAINT PK_PlayerData PRIMARY KEY (PlayerId)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PlayerData WHERE PlayerId = N'example-player-1')
BEGIN
    INSERT INTO dbo.PlayerData (PlayerId, State)
    VALUES
    (N'example-player-1', N'{"PlayerId":"example-player-1","Points":0,"ClickValue":1,"Upgrades":{}}'),
    (N'example-player-2', N'{"PlayerId":"example-player-2","Points":25,"ClickValue":2,"Upgrades":{}}'),
    (N'example-player-3', N'{"PlayerId":"example-player-3","Points":100,"ClickValue":5,"Upgrades":{}}'),
    (N'example-player-4', N'{"PlayerId":"example-player-4","Points":500,"ClickValue":1,"Upgrades":{}}'),
    (N'example-player-5', N'{"PlayerId":"example-player-5","Points":1000,"ClickValue":5,"Upgrades":{}}');
END;
GO
