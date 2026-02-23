USE [master]
GO
/****** Object:  Database [SistemaNominaADC]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE DATABASE [SistemaNominaADC]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'SistemaNominaADC', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\DATA\SistemaNominaADC.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'SistemaNominaADC_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\DATA\SistemaNominaADC_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [SistemaNominaADC] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [SistemaNominaADC].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [SistemaNominaADC] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET ARITHABORT OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SistemaNominaADC] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SistemaNominaADC] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET  DISABLE_BROKER 
GO
ALTER DATABASE [SistemaNominaADC] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SistemaNominaADC] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET RECOVERY FULL 
GO
ALTER DATABASE [SistemaNominaADC] SET  MULTI_USER 
GO
ALTER DATABASE [SistemaNominaADC] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SistemaNominaADC] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SistemaNominaADC] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [SistemaNominaADC] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [SistemaNominaADC] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [SistemaNominaADC] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [SistemaNominaADC] SET QUERY_STORE = ON
GO
ALTER DATABASE [SistemaNominaADC] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [SistemaNominaADC]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AguinaldoDetalle]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AguinaldoDetalle](
	[IdAguinaldoDetalle] [int] IDENTITY(1,1) NOT NULL,
	[idAguinaldo] [int] NULL,
	[IdEmpleado] [int] NULL,
	[MontoAguinaldo] [decimal](10, 2) NULL,
	[FechaCalculo] [date] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdAguinaldoDetalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AguinaldoEncabezado]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AguinaldoEncabezado](
	[IdAguinaldo] [int] IDENTITY(1,1) NOT NULL,
	[Periodo] [int] NULL,
	[Monto] [decimal](10, 2) NULL,
	[FechaCalculo] [date] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdAguinaldo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Asistencia]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Asistencia](
	[IdAsistencia] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[Fecha] [date] NULL,
	[HoraEntrada] [datetime] NULL,
	[HoraSalida] [datetime] NULL,
	[Ausencia] [bit] NULL,
	[Justificacion] [text] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdAsistencia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[Activo] [bit] NOT NULL,
	[EsSistema] [bit] NOT NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bitacora]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bitacora](
	[IdBitacora] [int] IDENTITY(1,1) NOT NULL,
	[Fecha] [datetime] NULL,
	[Accion] [varchar](150) NULL,
	[Descripcion] [text] NULL,
	[IdEstado] [int] NULL,
	[IdentityUserId] [nvarchar](450) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdBitacora] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Departamento]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departamento](
	[IdDepartamento] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](150) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDepartamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Empleado]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Empleado](
	[IdEmpleado] [int] IDENTITY(1,1) NOT NULL,
	[Cedula] [varchar](20) NULL,
	[NombreCompleto] [varchar](200) NULL,
	[FechaIngreso] [date] NULL,
	[FechaSalida] [date] NULL,
	[IdPuesto] [int] NULL,
	[SalarioBase] [decimal](10, 2) NULL,
	[IdEstado] [int] NULL,
	[IdentityUserId] [nvarchar](450) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdEmpleado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EscalaEvaluacion]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EscalaEvaluacion](
	[IdEscala] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[ValorMax] [int] NULL,
	[ValorMin] [int] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdEscala] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EscalaNivel]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EscalaNivel](
	[IdNivel] [int] IDENTITY(1,1) NOT NULL,
	[IdEscala] [int] NULL,
	[Valor] [int] NULL,
	[Descripcion] [varchar](100) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdNivel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Estado]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Estado](
	[IdEstado] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [int] NULL,
	[Nombre] [varchar](100) NULL,
	[Descripcion] [text] NULL,
	[Estado] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdEstado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EvaluacionDetalle]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EvaluacionDetalle](
	[IdDetalle] [int] IDENTITY(1,1) NOT NULL,
	[IdEvaluacion] [int] NULL,
	[IdPregunta] [int] NULL,
	[ValorRespuesta] [int] NULL,
	[ComentarioDetalle] [text] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDetalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EvaluacionEncabezado]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EvaluacionEncabezado](
	[IdEvaluacion] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[IdInstrumento] [int] NULL,
	[Fecha] [date] NULL,
	[EvaluadorId] [int] NULL,
	[PuntajeFinal] [decimal](5, 2) NULL,
	[Comentarios] [text] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdEvaluacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GrupoEstado]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GrupoEstado](
	[IdGrupoEstado] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[Descripcion] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdGrupoEstado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GrupoEstadoDetalle]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GrupoEstadoDetalle](
	[IdgrupoEstado] [int] NULL,
	[IdEstado] [int] NULL,
	[Orden] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Incapacidad]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Incapacidad](
	[IdIncapacidad] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[FechaInicio] [date] NULL,
	[FechaFin] [date] NULL,
	[IdTipoIncapacidad] [int] NULL,
	[MontoCubierto] [decimal](10, 2) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdIncapacidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InstrumentoEvaluacion]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InstrumentoEvaluacion](
	[IdInstrumento] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](200) NULL,
	[Descripcion] [text] NULL,
	[PeriodoInicio] [date] NULL,
	[PeriodoFin] [date] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdInstrumento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InstrumentoPregunta]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InstrumentoPregunta](
	[IdInstrumentoPregunta] [int] IDENTITY(1,1) NOT NULL,
	[IdInstrumento] [int] NULL,
	[IdPregunta] [int] NULL,
	[Ponderacion] [decimal](5, 2) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdInstrumentoPregunta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Liquidacion]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Liquidacion](
	[IdLiquidacion] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[Fecha] [date] NULL,
	[Preaviso] [decimal](10, 2) NULL,
	[Cesantia] [decimal](10, 2) NULL,
	[VacacionesPendientes] [decimal](10, 2) NULL,
	[AguinaldoProporcional] [decimal](10, 2) NULL,
	[Total] [decimal](10, 2) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdLiquidacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ModoCalculoConceptoNomina]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModoCalculoConceptoNomina](
	[IdModoCalculoConceptoNomina] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[Descripcion] [varchar](250) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdModoCalculoConceptoNomina] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ObjetoSistema]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ObjetoSistema](
	[IdObjeto] [int] IDENTITY(1,1) NOT NULL,
	[NombreEntidad] [varchar](100) NULL,
	[IdGrupoEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdObjeto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NombreEntidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ObjetoSistemaRol]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ObjetoSistemaRol](
	[IdObjeto] [int] NOT NULL,
	[RoleName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_ObjetoSistemaRol] PRIMARY KEY CLUSTERED 
(
	[IdObjeto] ASC,
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Permiso]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permiso](
	[IdPermiso] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[IdTipoPermiso] [int] NULL,
	[FechaInicio] [date] NULL,
	[FechaFin] [date] NULL,
	[Motivo] [varchar](200) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPermiso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanillaDetalle]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanillaDetalle](
	[IdPlanillaDetalle] [int] IDENTITY(1,1) NOT NULL,
	[IdPlanilla] [int] NULL,
	[IdEmpleado] [int] NULL,
	[SalarioBase] [decimal](10, 2) NULL,
	[TotalIngresos] [decimal](10, 2) NULL,
	[TotalDeducciones] [decimal](10, 2) NULL,
	[SalarioBruto] [decimal](10, 2) NULL,
	[SalarioNeto] [decimal](10, 2) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPlanillaDetalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanillaDetalleConcepto]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanillaDetalleConcepto](
	[IdDetalleConcepto] [int] IDENTITY(1,1) NOT NULL,
	[IdPlanillaDetalle] [int] NULL,
	[IdConceptoNomina] [int] NULL,
	[Monto] [decimal](10, 2) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDetalleConcepto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanillaEncabezado]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanillaEncabezado](
	[IdPlanilla] [int] IDENTITY(1,1) NOT NULL,
	[PeriodoInicio] [date] NULL,
	[PeriodoFin] [date] NULL,
	[PeriodoAguinaldo] [int] NULL,
	[FechaPago] [date] NULL,
	[IdTipoPlanilla] [int] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPlanilla] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PreguntaEvaluacion]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreguntaEvaluacion](
	[IdPregunta] [int] IDENTITY(1,1) NOT NULL,
	[Pregunta] [text] NULL,
	[IdEscala] [int] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPregunta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Puesto]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Puesto](
	[IdPuesto] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](150) NULL,
	[SalarioBase] [decimal](10, 2) NULL,
	[IdDepartamento] [int] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPuesto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SolicitudHorasExtra]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SolicitudHorasExtra](
	[IdSolicitudHorasExtra] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[Fecha] [date] NULL,
	[CantidadHoras] [decimal](10, 2) NULL,
	[IdTipoHoraExtra] [int] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdSolicitudHorasExtra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SolicitudVacaciones]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SolicitudVacaciones](
	[IdSolicitudVacaciones] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[CantidadDias] [int] NULL,
	[FechaInicio] [date] NULL,
	[FechaFin] [date] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdSolicitudVacaciones] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoConceptoNomina]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoConceptoNomina](
	[IdConceptoNomina] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](150) NULL,
	[IdModoCalculo] [int] NULL,
	[FormulaCalculo] [varchar](1000) NULL,
	[EsIngreso] [bit] NULL,
	[EsDeduccion] [bit] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdConceptoNomina] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoHoraExtra]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoHoraExtra](
	[IdTipoHoraExtra] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[PorcentajePago] [decimal](5, 4) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdTipoHoraExtra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoIncapacidad]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoIncapacidad](
	[IdTipoIncapacidad] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdTipoIncapacidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoPermiso]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoPermiso](
	[IdTipoPermiso] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdTipoPermiso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoPlanilla]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoPlanilla](
	[IdTipoPlanilla] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NULL,
	[Descripcion] [varchar](100) NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdTipoPlanilla] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vacaciones]    Script Date: 22/02/2026 05:21:18 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vacaciones](
	[IdVacaciones] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NULL,
	[DiasRestantes] [int] NULL,
	[IdEstado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdVacaciones] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 22/02/2026 05:21:18 p. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetRoles] ADD  CONSTRAINT [DF_AspNetRoles_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[AspNetRoles] ADD  CONSTRAINT [DF_AspNetRoles_EsSistema]  DEFAULT ((0)) FOR [EsSistema]
GO
ALTER TABLE [dbo].[AguinaldoDetalle]  WITH CHECK ADD FOREIGN KEY([idAguinaldo])
REFERENCES [dbo].[AguinaldoEncabezado] ([IdAguinaldo])
GO
ALTER TABLE [dbo].[AguinaldoDetalle]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[AguinaldoDetalle]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[AguinaldoEncabezado]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Asistencia]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Asistencia]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Bitacora]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Bitacora]  WITH CHECK ADD  CONSTRAINT [FK_Bitacora_AspNetUsers] FOREIGN KEY([IdentityUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[Bitacora] CHECK CONSTRAINT [FK_Bitacora_AspNetUsers]
GO
ALTER TABLE [dbo].[Departamento]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Empleado]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Empleado]  WITH CHECK ADD FOREIGN KEY([IdPuesto])
REFERENCES [dbo].[Puesto] ([IdPuesto])
GO
ALTER TABLE [dbo].[Empleado]  WITH CHECK ADD  CONSTRAINT [FK_Empleado_AspNetUsers] FOREIGN KEY([IdentityUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Empleado] CHECK CONSTRAINT [FK_Empleado_AspNetUsers]
GO
ALTER TABLE [dbo].[EscalaEvaluacion]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[EscalaNivel]  WITH CHECK ADD FOREIGN KEY([IdEscala])
REFERENCES [dbo].[EscalaEvaluacion] ([IdEscala])
GO
ALTER TABLE [dbo].[EscalaNivel]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[EvaluacionDetalle]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[EvaluacionDetalle]  WITH CHECK ADD FOREIGN KEY([IdEvaluacion])
REFERENCES [dbo].[EvaluacionEncabezado] ([IdEvaluacion])
GO
ALTER TABLE [dbo].[EvaluacionDetalle]  WITH CHECK ADD FOREIGN KEY([IdPregunta])
REFERENCES [dbo].[PreguntaEvaluacion] ([IdPregunta])
GO
ALTER TABLE [dbo].[EvaluacionEncabezado]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[EvaluacionEncabezado]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[EvaluacionEncabezado]  WITH CHECK ADD FOREIGN KEY([IdInstrumento])
REFERENCES [dbo].[InstrumentoEvaluacion] ([IdInstrumento])
GO
ALTER TABLE [dbo].[GrupoEstadoDetalle]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[GrupoEstadoDetalle]  WITH CHECK ADD FOREIGN KEY([IdgrupoEstado])
REFERENCES [dbo].[GrupoEstado] ([IdGrupoEstado])
GO
ALTER TABLE [dbo].[Incapacidad]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Incapacidad]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Incapacidad]  WITH CHECK ADD FOREIGN KEY([IdTipoIncapacidad])
REFERENCES [dbo].[TipoIncapacidad] ([IdTipoIncapacidad])
GO
ALTER TABLE [dbo].[InstrumentoEvaluacion]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[InstrumentoPregunta]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[InstrumentoPregunta]  WITH CHECK ADD FOREIGN KEY([IdInstrumento])
REFERENCES [dbo].[InstrumentoEvaluacion] ([IdInstrumento])
GO
ALTER TABLE [dbo].[InstrumentoPregunta]  WITH CHECK ADD FOREIGN KEY([IdPregunta])
REFERENCES [dbo].[PreguntaEvaluacion] ([IdPregunta])
GO
ALTER TABLE [dbo].[Liquidacion]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Liquidacion]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[ModoCalculoConceptoNomina]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[ObjetoSistema]  WITH CHECK ADD FOREIGN KEY([IdGrupoEstado])
REFERENCES [dbo].[GrupoEstado] ([IdGrupoEstado])
GO
ALTER TABLE [dbo].[ObjetoSistemaRol]  WITH CHECK ADD  CONSTRAINT [FK_ObjetoSistemaRol_ObjetoSistema_IdObjeto] FOREIGN KEY([IdObjeto])
REFERENCES [dbo].[ObjetoSistema] ([IdObjeto])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ObjetoSistemaRol] CHECK CONSTRAINT [FK_ObjetoSistemaRol_ObjetoSistema_IdObjeto]
GO
ALTER TABLE [dbo].[Permiso]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Permiso]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Permiso]  WITH CHECK ADD FOREIGN KEY([IdTipoPermiso])
REFERENCES [dbo].[TipoPermiso] ([IdTipoPermiso])
GO
ALTER TABLE [dbo].[PlanillaDetalle]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[PlanillaDetalle]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[PlanillaDetalle]  WITH CHECK ADD FOREIGN KEY([IdPlanilla])
REFERENCES [dbo].[PlanillaEncabezado] ([IdPlanilla])
GO
ALTER TABLE [dbo].[PlanillaDetalleConcepto]  WITH CHECK ADD FOREIGN KEY([IdConceptoNomina])
REFERENCES [dbo].[TipoConceptoNomina] ([IdConceptoNomina])
GO
ALTER TABLE [dbo].[PlanillaDetalleConcepto]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[PlanillaDetalleConcepto]  WITH CHECK ADD FOREIGN KEY([IdPlanillaDetalle])
REFERENCES [dbo].[PlanillaDetalle] ([IdPlanillaDetalle])
GO
ALTER TABLE [dbo].[PlanillaEncabezado]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[PlanillaEncabezado]  WITH CHECK ADD FOREIGN KEY([IdTipoPlanilla])
REFERENCES [dbo].[TipoPlanilla] ([IdTipoPlanilla])
GO
ALTER TABLE [dbo].[PreguntaEvaluacion]  WITH CHECK ADD FOREIGN KEY([IdEscala])
REFERENCES [dbo].[EscalaEvaluacion] ([IdEscala])
GO
ALTER TABLE [dbo].[PreguntaEvaluacion]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Puesto]  WITH CHECK ADD FOREIGN KEY([IdDepartamento])
REFERENCES [dbo].[Departamento] ([IdDepartamento])
GO
ALTER TABLE [dbo].[Puesto]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[SolicitudHorasExtra]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[SolicitudHorasExtra]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[SolicitudHorasExtra]  WITH CHECK ADD FOREIGN KEY([IdTipoHoraExtra])
REFERENCES [dbo].[TipoHoraExtra] ([IdTipoHoraExtra])
GO
ALTER TABLE [dbo].[SolicitudVacaciones]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[SolicitudVacaciones]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[TipoConceptoNomina]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[TipoConceptoNomina]  WITH CHECK ADD FOREIGN KEY([IdModoCalculo])
REFERENCES [dbo].[ModoCalculoConceptoNomina] ([IdModoCalculoConceptoNomina])
GO
ALTER TABLE [dbo].[TipoHoraExtra]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[TipoIncapacidad]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[TipoPermiso]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[TipoPlanilla]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
ALTER TABLE [dbo].[Vacaciones]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Vacaciones]  WITH CHECK ADD FOREIGN KEY([IdEstado])
REFERENCES [dbo].[Estado] ([IdEstado])
GO
USE [master]
GO
ALTER DATABASE [SistemaNominaADC] SET  READ_WRITE 
GO
