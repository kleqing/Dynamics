IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [UserName] nvarchar(256) NOT NULL,
    [UserDOB] date NULL,
    [PhoneNumber] nvarchar(11) NULL,
    [UserAddress] nvarchar(max) NULL,
    [UserAvatar] nvarchar(max) NULL,
    [UserDescription] nvarchar(1000) NULL,
    [isBanned] bit NOT NULL,
    [CreatedDate] datetime2 NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Organizations] (
    [OrganizationID] uniqueidentifier NOT NULL,
    [OrganizationName] nvarchar(100) NOT NULL,
    [OrganizationEmail] nvarchar(450) NULL,
    [OrganizationPhoneNumber] nvarchar(max) NULL,
    [OrganizationAddress] nvarchar(max) NULL,
    [OrganizationDescription] nvarchar(max) NOT NULL,
    [OrganizationPictures] nvarchar(max) NULL,
    [StartTime] date NOT NULL,
    [ShutdownDay] date NULL,
    [OrganizationStatus] int NOT NULL,
    CONSTRAINT [PK_Organizations] PRIMARY KEY ([OrganizationID])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Notifications] (
    [NotificationID] uniqueidentifier NOT NULL,
    [UserID] uniqueidentifier NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Date] datetime2 NOT NULL,
    [Link] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationID]),
    CONSTRAINT [FK_Notifications_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Reports] (
    [ReportID] uniqueidentifier NOT NULL,
    [ReporterID] uniqueidentifier NULL,
    [ObjectID] uniqueidentifier NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Reason] nvarchar(100) NOT NULL,
    [ReportDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportID]),
    CONSTRAINT [FK_Reports_AspNetUsers_ReporterID] FOREIGN KEY ([ReporterID]) REFERENCES [AspNetUsers] ([Id])
);
GO

CREATE TABLE [Requests] (
    [RequestID] uniqueidentifier NOT NULL,
    [UserID] uniqueidentifier NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreationDate] datetime2 NULL,
    [RequestTitle] nvarchar(max) NOT NULL,
    [RequestEmail] nvarchar(max) NOT NULL,
    [RequestPhoneNumber] nvarchar(max) NOT NULL,
    [Location] nvarchar(max) NOT NULL,
    [Attachment] nvarchar(max) NULL,
    [isEmergency] int NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Requests] PRIMARY KEY ([RequestID]),
    CONSTRAINT [FK_Requests_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Wallets] (
    [WalletId] uniqueidentifier NOT NULL,
    [Amount] int NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Wallets] PRIMARY KEY ([WalletId]),
    CONSTRAINT [FK_Wallets_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [OrganizationMember] (
    [UserID] uniqueidentifier NOT NULL,
    [OrganizationID] uniqueidentifier NOT NULL,
    [Status] int NULL,
    CONSTRAINT [PK_OrganizationMember] PRIMARY KEY ([OrganizationID], [UserID]),
    CONSTRAINT [FK_OrganizationMember_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrganizationMember_Organizations_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [Organizations] ([OrganizationID]) ON DELETE CASCADE
);
GO

CREATE TABLE [OrganizationResources] (
    [ResourceID] uniqueidentifier NOT NULL,
    [OrganizationID] uniqueidentifier NOT NULL,
    [ResourceName] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [Unit] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_OrganizationResources] PRIMARY KEY ([ResourceID]),
    CONSTRAINT [FK_OrganizationResources_Organizations_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [Organizations] ([OrganizationID]) ON DELETE CASCADE
);
GO

CREATE TABLE [Projects] (
    [ProjectID] uniqueidentifier NOT NULL,
    [OrganizationID] uniqueidentifier NOT NULL,
    [RequestID] uniqueidentifier NULL,
    [ProjectName] nvarchar(100) NOT NULL,
    [ProjectEmail] nvarchar(max) NULL,
    [ProjectPhoneNumber] nvarchar(max) NULL,
    [ProjectAddress] nvarchar(max) NOT NULL,
    [ProjectStatus] int NOT NULL,
    [Attachment] nvarchar(max) NULL,
    [ReportFile] nvarchar(max) NULL,
    [ProjectDescription] nvarchar(1000) NOT NULL,
    [StartTime] date NULL,
    [EndTime] date NULL,
    [ShutdownReason] nvarchar(max) NULL,
    [isBanned] bit NOT NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY ([ProjectID]),
    CONSTRAINT [FK_Projects_Organizations_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [Organizations] ([OrganizationID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Projects_Requests_RequestID] FOREIGN KEY ([RequestID]) REFERENCES [Requests] ([RequestID])
);
GO

CREATE TABLE [UserWalletTransactions] (
    [TransactionId] uniqueidentifier NOT NULL,
    [Amount] int NOT NULL,
    [Message] nvarchar(500) NULL,
    [TransactionType] nvarchar(max) NOT NULL,
    [Time] datetime2 NOT NULL,
    [WalletId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_UserWalletTransactions] PRIMARY KEY ([TransactionId]),
    CONSTRAINT [FK_UserWalletTransactions_Wallets_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Wallets] ([WalletId]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserToOrganizationTransactionHistories] (
    [TransactionID] uniqueidentifier NOT NULL,
    [ResourceID] uniqueidentifier NOT NULL,
    [UserID] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [Amount] int NOT NULL,
    [Message] nvarchar(max) NULL,
    [Time] date NOT NULL,
    [Attachments] nvarchar(max) NULL,
    CONSTRAINT [PK_UserToOrganizationTransactionHistories] PRIMARY KEY ([TransactionID]),
    CONSTRAINT [FK_UserToOrganizationTransactionHistories_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserToOrganizationTransactionHistories_OrganizationResources_ResourceID] FOREIGN KEY ([ResourceID]) REFERENCES [OrganizationResources] ([ResourceID]) ON DELETE CASCADE
);
GO

CREATE TABLE [Histories] (
    [HistoryID] uniqueidentifier NOT NULL,
    [ProjectID] uniqueidentifier NOT NULL,
    [Phase] nvarchar(100) NOT NULL,
    [Date] date NOT NULL,
    [Content] nvarchar(1000) NOT NULL,
    [Attachment] nvarchar(max) NULL,
    CONSTRAINT [PK_Histories] PRIMARY KEY ([HistoryID]),
    CONSTRAINT [FK_Histories_Projects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [Projects] ([ProjectID]) ON DELETE CASCADE
);
GO

CREATE TABLE [ProjectMembers] (
    [UserID] uniqueidentifier NOT NULL,
    [ProjectID] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_ProjectMembers] PRIMARY KEY ([ProjectID], [UserID]),
    CONSTRAINT [FK_ProjectMembers_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProjectMembers_Projects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [Projects] ([ProjectID]) ON DELETE CASCADE
);
GO

CREATE TABLE [ProjectResources] (
    [ResourceID] uniqueidentifier NOT NULL,
    [ProjectID] uniqueidentifier NOT NULL,
    [ResourceName] nvarchar(100) NOT NULL,
    [Quantity] int NULL,
    [ExpectedQuantity] int NOT NULL,
    [Unit] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_ProjectResources] PRIMARY KEY ([ResourceID]),
    CONSTRAINT [FK_ProjectResources_Projects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [Projects] ([ProjectID]) ON DELETE CASCADE
);
GO

CREATE TABLE [Withdraws] (
    [WithdrawID] uniqueidentifier NOT NULL,
    [ProjectID] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [BankAccountNumber] nvarchar(500) NOT NULL,
    [BankName] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NULL,
    [Time] datetime2 NOT NULL,
    CONSTRAINT [PK_Withdraws] PRIMARY KEY ([WithdrawID]),
    CONSTRAINT [FK_Withdraws_Projects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [Projects] ([ProjectID]) ON DELETE CASCADE
);
GO

CREATE TABLE [OrganizationToProjectTransactionHistory] (
    [TransactionID] uniqueidentifier NOT NULL,
    [OrganizationResourceID] uniqueidentifier NOT NULL,
    [ProjectResourceID] uniqueidentifier NULL,
    [Status] int NOT NULL,
    [Amount] int NOT NULL,
    [Message] nvarchar(100) NULL,
    [Time] date NOT NULL,
    [Attachments] nvarchar(max) NULL,
    CONSTRAINT [PK_OrganizationToProjectTransactionHistory] PRIMARY KEY ([TransactionID]),
    CONSTRAINT [FK_OrganizationToProjectTransactionHistory_OrganizationResources_OrganizationResourceID] FOREIGN KEY ([OrganizationResourceID]) REFERENCES [OrganizationResources] ([ResourceID]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrganizationToProjectTransactionHistory_ProjectResources_ProjectResourceID] FOREIGN KEY ([ProjectResourceID]) REFERENCES [ProjectResources] ([ResourceID])
);
GO

CREATE TABLE [UserToProjectTransactionHistories] (
    [TransactionID] uniqueidentifier NOT NULL,
    [ProjectResourceID] uniqueidentifier NOT NULL,
    [UserID] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [Amount] int NOT NULL,
    [Message] nvarchar(100) NULL,
    [Time] date NOT NULL,
    [Attachments] nvarchar(max) NULL,
    CONSTRAINT [PK_UserToProjectTransactionHistories] PRIMARY KEY ([TransactionID]),
    CONSTRAINT [FK_UserToProjectTransactionHistories_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserToProjectTransactionHistories_ProjectResources_ProjectResourceID] FOREIGN KEY ([ProjectResourceID]) REFERENCES [ProjectResources] ([ResourceID]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [IX_AspNetUsers_Email] ON [AspNetUsers] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_AspNetUsers_UserName] ON [AspNetUsers] ([UserName]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Histories_ProjectID] ON [Histories] ([ProjectID]);
GO

CREATE INDEX [IX_Notifications_UserID] ON [Notifications] ([UserID]);
GO

CREATE INDEX [IX_OrganizationMember_UserID] ON [OrganizationMember] ([UserID]);
GO

CREATE INDEX [IX_OrganizationResources_OrganizationID] ON [OrganizationResources] ([OrganizationID]);
GO

CREATE UNIQUE INDEX [IX_Organizations_OrganizationEmail] ON [Organizations] ([OrganizationEmail]) WHERE [OrganizationEmail] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_Organizations_OrganizationName] ON [Organizations] ([OrganizationName]);
GO

CREATE INDEX [IX_OrganizationToProjectTransactionHistory_OrganizationResourceID] ON [OrganizationToProjectTransactionHistory] ([OrganizationResourceID]);
GO

CREATE INDEX [IX_OrganizationToProjectTransactionHistory_ProjectResourceID] ON [OrganizationToProjectTransactionHistory] ([ProjectResourceID]);
GO

CREATE INDEX [IX_ProjectMembers_UserID] ON [ProjectMembers] ([UserID]);
GO

CREATE INDEX [IX_ProjectResources_ProjectID] ON [ProjectResources] ([ProjectID]);
GO

CREATE INDEX [IX_Projects_OrganizationID] ON [Projects] ([OrganizationID]);
GO

CREATE UNIQUE INDEX [IX_Projects_RequestID] ON [Projects] ([RequestID]) WHERE [RequestID] IS NOT NULL;
GO

CREATE INDEX [IX_Reports_ReporterID] ON [Reports] ([ReporterID]);
GO

CREATE INDEX [IX_Requests_UserID] ON [Requests] ([UserID]);
GO

CREATE INDEX [IX_UserToOrganizationTransactionHistories_ResourceID] ON [UserToOrganizationTransactionHistories] ([ResourceID]);
GO

CREATE INDEX [IX_UserToOrganizationTransactionHistories_UserID] ON [UserToOrganizationTransactionHistories] ([UserID]);
GO

CREATE INDEX [IX_UserToProjectTransactionHistories_ProjectResourceID] ON [UserToProjectTransactionHistories] ([ProjectResourceID]);
GO

CREATE INDEX [IX_UserToProjectTransactionHistories_UserID] ON [UserToProjectTransactionHistories] ([UserID]);
GO

CREATE INDEX [IX_UserWalletTransactions_WalletId] ON [UserWalletTransactions] ([WalletId]);
GO

CREATE UNIQUE INDEX [IX_Wallets_UserId] ON [Wallets] ([UserId]);
GO

CREATE INDEX [IX_Withdraws_ProjectID] ON [Withdraws] ([ProjectID]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241108144402_init', N'8.0.8');
GO

COMMIT;
GO

