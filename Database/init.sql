IF DB_ID(N'ClickerEmpire') IS NULL
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
        Points bigint NOT NULL,
        ClickValue int NOT NULL,
        CONSTRAINT PK_PlayerData PRIMARY KEY (PlayerId)
    );
END;
GO

IF COL_LENGTH(N'dbo.PlayerData', N'Points') IS NULL
BEGIN
    ALTER TABLE dbo.PlayerData ADD Points bigint NULL, ClickValue int NULL;

    IF COL_LENGTH(N'dbo.PlayerData', N'State') IS NOT NULL
    BEGIN
        UPDATE dbo.PlayerData
        SET Points = CONVERT(bigint, JSON_VALUE(State, '$.Points')),
            ClickValue = CONVERT(int, JSON_VALUE(State, '$.ClickValue'));
    END;

    UPDATE dbo.PlayerData
    SET Points = ISNULL(Points, 0), ClickValue = ISNULL(ClickValue, 1);

    ALTER TABLE dbo.PlayerData ALTER COLUMN Points bigint NOT NULL;
    ALTER TABLE dbo.PlayerData ALTER COLUMN ClickValue int NOT NULL;

    IF COL_LENGTH(N'dbo.PlayerData', N'State') IS NOT NULL
        ALTER TABLE dbo.PlayerData DROP COLUMN State;
END;
GO

IF OBJECT_ID(N'dbo.UpgradeData', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UpgradeData
    (
        PlayerId nvarchar(450) NOT NULL,
        UpgradeId nvarchar(100) NOT NULL,
        Name nvarchar(200) NOT NULL,
        Description nvarchar(1000) NOT NULL,
        Cost int NOT NULL,
        PointsPerSecond int NOT NULL,
        ClickMultiplier int NOT NULL,
        Level int NOT NULL,
        CONSTRAINT PK_UpgradeData PRIMARY KEY (PlayerId, UpgradeId),
        CONSTRAINT FK_UpgradeData_PlayerData FOREIGN KEY (PlayerId)
            REFERENCES dbo.PlayerData (PlayerId) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PlayerData WHERE PlayerId = N'example-player-1')
BEGIN
    INSERT INTO dbo.PlayerData (PlayerId, Points, ClickValue)
    VALUES
    (N'example-player-1', 0, 1),
    (N'example-player-2', 25, 2),
    (N'example-player-3', 100, 5),
    (N'example-player-4', 500, 1),
    (N'example-player-5', 1000, 5);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.UpgradeData WHERE PlayerId = N'example-player-1')
BEGIN
    INSERT INTO dbo.UpgradeData
        (PlayerId, UpgradeId, Name, Description, Cost, PointsPerSecond, ClickMultiplier, Level)
    SELECT players.PlayerId, upgrades.UpgradeId, upgrades.Name, upgrades.Description,
           upgrades.Cost, upgrades.PointsPerSecond, upgrades.ClickMultiplier, 0
    FROM (VALUES
        (N'example-player-1'), (N'example-player-2'), (N'example-player-3'),
        (N'example-player-4'), (N'example-player-5')) AS players(PlayerId)
    CROSS JOIN (VALUES
        (N'auto-clicker', N'Auto Clicker', N'Clicks automatically every second', 10, 1, 1),
        (N'double-click', N'Double Click', N'Each click is worth 2 points', 25, 0, 2),
        (N'click-frenzy', N'Click Frenzy', N'Each click is worth 5 points', 100, 0, 5),
        (N'mega-farm', N'Mega Farm', N'Generates 10 points per second', 500, 10, 1)
    ) AS upgrades(UpgradeId, Name, Description, Cost, PointsPerSecond, ClickMultiplier);
END;
GO
