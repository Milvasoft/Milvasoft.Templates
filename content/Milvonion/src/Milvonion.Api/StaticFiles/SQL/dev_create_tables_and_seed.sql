﻿

BEGIN TRANSACTION;

--------------------------------------------------------------------------------------------------
-- Create Users table
CREATE TABLE public."Users" (
    "Id" SERIAL PRIMARY KEY ,
    "UserName" VARCHAR(100) NOT NULL,
    "NormalizedUserName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(100) NULL,
    "NormalizedEmail" VARCHAR(100) NULL,
    "Name" VARCHAR(100),
    "Surname" VARCHAR(100),
    "UserType" SMALLINT NOT NULL,
    "PasswordHash" VARCHAR NOT NULL,
    "EmailConfirmed" BOOLEAN NOT NULL,    
    "PhoneNumberConfirmed" BOOLEAN NOT NULL,    
    "TwoFactorEnabled" BOOLEAN NOT NULL,    
    "PhoneNumber" VARCHAR(100) NULL,    
    "LockoutEnd" timestamp with time zone null,
    "LockoutEnabled" BOOLEAN NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    "CreatorUserName" VARCHAR(100) null,
    "LastModificationDate" timestamp with time zone null,
    "LastModifierUserName" VARCHAR(100) null,
    "DeletionDate" timestamp with time zone null,
    "DeleterUserName" VARCHAR(100) null,
    "IsDeleted" BOOLEAN NOT NULL
);

--------------------------------------------------------------------------------------------------
-- Create UserSessions table
CREATE TABLE public."UserSessions" (
    "Id" SERIAL PRIMARY KEY ,
    "UserName" VARCHAR(100) NOT NULL,    
    "AccessToken" VARCHAR NULL,
    "RefreshToken" VARCHAR NULL,
    "DeviceId" VARCHAR NULL,
    "UserId" int NOT NULL,
    "ExpiryDate" timestamp with time zone NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    "CreatorUserName" VARCHAR(100) null,
    "LastModificationDate" timestamp with time zone null,
    "LastModifierUserName" VARCHAR(100) null,
    "DeletionDate" timestamp with time zone null,
    "DeleterUserName" VARCHAR(100) null,
    "IsDeleted" BOOLEAN NOT NULL,
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);

--------------------------------------------------------------------------------------------------
-- Create Roles table
CREATE TABLE public."Roles" (
    "Id" SERIAL PRIMARY KEY ,
    "Name" VARCHAR(100) NOT NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    "CreatorUserName" VARCHAR(100) null,
    "LastModificationDate" timestamp with time zone null,
    "LastModifierUserName" VARCHAR(100) null,
    "DeletionDate" timestamp with time zone null,
    "DeleterUserName" VARCHAR(100) null,
    "IsDeleted" BOOLEAN NOT NULL
);

--------------------------------------------------------------------------------------------------
-- Create Permissions table
CREATE TABLE public."Permissions" (
    "Id" SERIAL PRIMARY KEY ,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(255) NULL,
    "NormalizedName" VARCHAR(100) NOT NULL,
    "PermissionGroup" VARCHAR(100),
    "PermissionGroupDescription" VARCHAR(255) NULL
);

--------------------------------------------------------------------------------------------------
-- Create RolePermissionRelations table with indexes
CREATE TABLE public."RolePermissionRelations" (
    "Id" SERIAL PRIMARY KEY ,
    "RoleId" INTEGER NOT NULL,
    "PermissionId" INTEGER NOT NULL,
    FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    FOREIGN KEY ("PermissionId") REFERENCES "Permissions"("Id")
);

--------------------------------------------------------------------------------------------------
-- Create UserRoleRelations table
CREATE TABLE public."UserRoleRelations" (
    "Id" SERIAL PRIMARY KEY ,
    "UserId" INTEGER NOT NULL,
    "RoleId" INTEGER NOT NULL,
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id")
);

--------------------------------------------------------------------------------------------------
-- Create ActivityLogs table
CREATE TABLE public."ActivityLogs" (
    "Id" SERIAL PRIMARY KEY ,
    "UserName" VARCHAR(100) NOT NULL,
    "Activity" SMALLINT NOT NULL,
    "ActivityDate" timestamp with time zone NOT NULL
);

--------------------------------------------------------------------------------------------------
-- Create ApiLogs table
CREATE TABLE public."ApiLogs" (
    "Id" SERIAL PRIMARY KEY ,
    "TransactionId" VARCHAR(255) NOT NULL,
    "Severity" VARCHAR(50),
    "Timestamp" timestamp with time zone NOT NULL,
    "Path" VARCHAR(255),
    "RequestInfoJson" jsonb,
    "ResponseInfoJson" jsonb,
	"ElapsedMs" bigint,
    "IpAddress" VARCHAR(50),
    "UserName" VARCHAR(100)
);

--------------------------------------------------------------------------------------------------
-- Create MethodLogs table
CREATE TABLE public."MethodLogs" (
    "Id" SERIAL PRIMARY KEY ,
    "TransactionId" text NOT NULL,
    "Namespace" text,
    "ClassName" text,
    "MethodName" text,
    "MethodParams" text,
    "MethodResult" text,
    "ElapsedMs" int,
    "UtcLogTime" timestamp with time zone NOT NULL,
    "CacheInfo" text,
    "Exception" text,
	"IsSuccess" BOOLEAN
);

--------------------------------------------------------------------------------------------------
-- Create MenuGroups table
CREATE TABLE public."MenuGroups" (
    "Id" SERIAL PRIMARY KEY ,
    "Translations" jsonb NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    "CreatorUserName" VARCHAR(100) null
);

--------------------------------------------------------------------------------------------------
-- Create MenuItems table
CREATE TABLE public."MenuItems" (
    "Id" SERIAL PRIMARY KEY ,
    "Url" varchar(100) NULL,
    "PageName" varchar(100) NULL,
    "PermissionOrGroupNames" jsonb NULL,
    "Translations" jsonb NULL,
    "GroupId" INTEGER NOT NULL,
    "ParentId" INTEGER NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    "CreatorUserName" VARCHAR(100) null,
    FOREIGN KEY ("GroupId") REFERENCES "MenuGroups"("Id"),
    FOREIGN KEY ("ParentId") REFERENCES "MenuItems"("Id")
);

--------------------------------------------------------------------------------------------------
-- Create Pages table
CREATE TABLE public."Pages" (
    "Id" SERIAL PRIMARY KEY ,
    "Name" varchar(100) NULL,
    "HasCreate" BOOLEAN NOT NULL,
    "HasDetail" BOOLEAN NOT NULL,
    "HasEdit" BOOLEAN NOT NULL,
    "HasDelete" BOOLEAN NOT NULL,
    "CreatePermissions" jsonb NULL,
    "DetailPermissions" jsonb NULL,
    "EditPermissions" jsonb NULL,
    "DeletePermissions" jsonb NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    "CreatorUserName" VARCHAR(100) null
);

--------------------------------------------------------------------------------------------------
-- Create PageActions table
CREATE TABLE public."PageActions" (
    "Id" SERIAL PRIMARY KEY ,
    "ActionName" varchar(100) NULL,
    "Permissions" jsonb NULL,
    "Translations" jsonb NULL,
    "PageId" INTEGER NULL,
    "CreationDate" timestamp with time zone NOT NULL,
    "CreatorUserName" VARCHAR(100) null,
    FOREIGN KEY ("PageId") REFERENCES "Pages"("Id")
);

--------------------------------------------------------------------------------------------------
-- Create IsDeleted Indexes
CREATE INDEX idx_roles_isdeleted ON "Roles"("IsDeleted" ASC);
CREATE INDEX idx_users_isdeleted ON "Users"("IsDeleted" ASC);
CREATE INDEX idx_usersessions_isdeleted ON "UserSessions"("IsDeleted" ASC);

-- Create Foreign Key Indexes
CREATE INDEX idx_rolepermissionrelations_roleid ON "RolePermissionRelations"("RoleId");
CREATE INDEX idx_rolepermissionrelations_permissionid ON "RolePermissionRelations"("PermissionId");
CREATE INDEX idx_userrolerelations_userid ON "UserRoleRelations"("UserId");
CREATE INDEX idx_userrolerelations_roleid ON "UserRoleRelations"("RoleId");
CREATE INDEX idx_usersessions_userid ON "UserSessions"("UserId");
CREATE INDEX idx_pageactions_pageid ON "PageActions"("PageId");

-- Create Custom Indexes
-- Users
CREATE UNIQUE INDEX idx_users_username ON "Users"("UserName");

-- Pages
CREATE UNIQUE INDEX idx_pages_name ON "Pages"("Name");

-- ActivityLogs 
CREATE INDEX idx_activitylogs_activitydate ON "ActivityLogs"("ActivityDate" DESC);

-- Permissions
CREATE UNIQUE INDEX idx_permissions_permissiongroup ON "Permissions"("PermissionGroup", "Name");

-- ApiLogs
CREATE INDEX idx_apilogs_timestamp ON "ApiLogs"("Timestamp" DESC);
CREATE INDEX idx_apilogs_path ON "ApiLogs"("Path");
CREATE INDEX idx_apilogs_transactionid ON "ApiLogs"("TransactionId");
CREATE INDEX idx_apilogs_severity ON "ApiLogs"("Severity");
CREATE INDEX idx_apilogs_username ON "ApiLogs"("UserName");

-- MethodLogs
CREATE INDEX idx_methodlogs_utclogtime ON "MethodLogs"("UtcLogTime" DESC);
CREATE INDEX idx_methodlogs_transactionid ON "MethodLogs"("TransactionId");
CREATE INDEX idx_methodlogs_issuccess ON "MethodLogs"("IsSuccess");



--SEED--------------------------------------------------------------------------------------------------------------
--Root user seed
insert into "Users"
    (
	 "Id",	
     "UserName",
     "NormalizedUserName",
     "Email",
     "NormalizedEmail",
     "Name",
     "Surname",
	 "UserType",	
     "PasswordHash",
     "EmailConfirmed",
     "PhoneNumberConfirmed",
     "TwoFactorEnabled",
     "PhoneNumber",
     "LockoutEnd",
     "LockoutEnabled",
     "AccessFailedCount",
     "CreationDate",
     "CreatorUserName",
     "IsDeleted")
     values 
     (
	  1,
      'rootuser',
      'ROOTUSER',
      'rootuser@gmail.com' ,
      'ROOTUSER@GMAIL.COM' ,
      'Administrator' ,
      'User' ,
	  1,	 
      'AQAAAAEAACcQAAAAEIMgDN79W03UXM/6VMhU4ua5i6DRRmS/kj77Jdfi1vjxSUnugS7ZF1CszYSLuhKvpw==' ,
      true ,
      true,
      false,
      null,
      null ,
      false,
      0,
      CURRENT_DATE ,
      'System',
      false);

--Insert SuperAdmin Role
insert into "Roles"
	   ("Id","Name","CreationDate","CreatorUserName","IsDeleted")
values (1,'SuperAdmin',CURRENT_DATE,'System',false);

	
--Insert SuperAdmin Permission
insert into "Permissions"
	   ("Id","Name","Description","NormalizedName","PermissionGroup","PermissionGroupDescription")
values (1,'SuperAdmin','Provides access to the entire system.','SUPERADMIN','App','Application-wide permissions.');
	

--Insert SuperAdmin Permission-Role relation
insert into "RolePermissionRelations"
	   ("Id","RoleId","PermissionId")
values (1,1,1);


--Insert SuperAdmin User-Role relation
insert into "UserRoleRelations"
	   ("Id","UserId","RoleId")
values (1,1,1);

----------------------------------------------------------------------------------------------------------------
-- Auto increment start 21 

DO $$
DECLARE
    seq_record RECORD;
BEGIN
    FOR seq_record IN
        SELECT table_name, column_name, pg_get_serial_sequence('"' || table_name || '"', column_name) AS seq_name
		FROM information_schema.columns
		WHERE table_schema = 'public' AND data_type = 'integer' AND column_default LIKE 'nextval%'
    LOOP
        -- Sequence başlangıç değerini güncelle
        EXECUTE format('ALTER SEQUENCE %s RESTART WITH 22;', seq_record.seq_name);
    END LOOP;
END $$;

----------------------------------------------------------------------------------------------------------------
-- Seed modification prevention trigger

CREATE OR REPLACE FUNCTION prevent_seed_modification()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        IF NEW."Id" < 21 THEN
            RAISE EXCEPTION 'Seed records cannot be modified.';
        END IF;
    ELSIF TG_OP = 'UPDATE' THEN
        IF NEW."Id" < 21 THEN
            RAISE EXCEPTION 'Seed records cannot be modified.';
        END IF;
    ELSIF TG_OP = 'DELETE' THEN
        IF OLD."Id" < 21 THEN
            RAISE EXCEPTION 'Seed records cannot be deleted.';
        END IF;
    END IF;

    RETURN NEW; -- RETURN OLD; if TG_OP = 'DELETE'
END;
$$ LANGUAGE plpgsql;

----------------------------------------------------------------------------------------------------------------
-- Add trigger to all tables

DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE format('
            CREATE TRIGGER prevent_seed_modification_trigger
            BEFORE INSERT OR UPDATE ON %I
            FOR EACH ROW EXECUTE FUNCTION prevent_seed_modification();', r.tablename);
    END LOOP;
END $$;


COMMIT TRANSACTION;


