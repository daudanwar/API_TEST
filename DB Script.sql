USE [master]
GO
/****** Object:  Database [StudentDB]    Script Date: 08/08/2024 4:46:00 pm ******/
CREATE DATABASE [StudentDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'SttudentDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\SttudentDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'SttudentDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\SttudentDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [StudentDB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [StudentDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [StudentDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [StudentDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [StudentDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [StudentDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [StudentDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [StudentDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [StudentDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [StudentDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [StudentDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [StudentDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [StudentDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [StudentDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [StudentDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [StudentDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [StudentDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [StudentDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [StudentDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [StudentDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [StudentDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [StudentDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [StudentDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [StudentDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [StudentDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [StudentDB] SET  MULTI_USER 
GO
ALTER DATABASE [StudentDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [StudentDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [StudentDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [StudentDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [StudentDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [StudentDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [StudentDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [StudentDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [StudentDB]
GO
/****** Object:  Table [dbo].[Student]    Script Date: 08/08/2024 4:46:00 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Student](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[MainImagePath] [nvarchar](500) NULL,
	[RegistrationNumber] [nchar](50) NULL,
	[RegistrationDate] [datetime] NULL,
	[SecondaryImagePaths] [nvarchar](max) NULL,
 CONSTRAINT [PK_Student] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[GET_STD_BY_ID]    Script Date: 08/08/2024 4:46:00 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GET_STD_BY_ID]
(
@ID int
)
as begin

select  ID	,Name,RegistrationNumber,		isnull(format(RegistrationDate,'yyyy-MM-ddTHH:mm:ssZ'),'') as RegistrationDate,	isnull(MainImagePath,'') as MainImagePath	,isnull(SecondaryImagePaths,'') as SecondaryImagePaths from Student
where ID=@ID
end

GO
/****** Object:  StoredProcedure [dbo].[Get_Student_Data]    Script Date: 08/08/2024 4:46:00 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[Get_Student_Data]
as begin

select  ID	,Name,RegistrationNumber,		isnull(format(RegistrationDate,'yyyy-MM-ddTHH:mm:ssZ'),'') as RegistrationDate,	MainImagePath	,isnull(SecondaryImagePaths,'') as SecondaryImagePaths from Student
end

GO
/****** Object:  StoredProcedure [dbo].[Save_Student]    Script Date: 08/08/2024 4:46:00 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Save_Student]
    @Name NVARCHAR(50),
    @RegistrationNumber NVARCHAR(10),
    @RegistrationDate DATETIME,
    @MainImagePath NVARCHAR(MAX),
    @SecondaryImagePaths NVARCHAR(MAX),
    @NewStudentId INT OUTPUT
AS
BEGIN
    -- Begin a transaction to ensure data consistency
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Insert student data into the Students table
        INSERT INTO Student (Name, RegistrationNumber, RegistrationDate, MainImagePath, SecondaryImagePaths)
        VALUES (@Name, @RegistrationNumber, @RegistrationDate, @MainImagePath, @SecondaryImagePaths);
        
        -- Retrieve the ID of the newly inserted record
        SET @NewStudentId = SCOPE_IDENTITY();
      -- print @NewStudentId
        -- Commit transaction if everything is successful
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Rollback transaction in case of an error
        ROLLBACK TRANSACTION;

        -- Optionally, you could log the error or rethrow
        THROW;
    END CATCH
END
GO
USE [master]
GO
ALTER DATABASE [StudentDB] SET  READ_WRITE 
GO
