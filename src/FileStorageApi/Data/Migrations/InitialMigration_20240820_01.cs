using FluentMigrator;

namespace FileStorageApi.Data.Migrations;

[Migration(20240820_01)]
public class InitialMigration_20240820_01 : Migration
{
	public override void Up()
	{
		const string query = """
			CREATE TABLE IF NOT EXISTS users (
				id            UUID        PRIMARY KEY,
				email         VARCHAR     NOT NULL UNIQUE,
				is_confirmed  BOOLEAN     NOT NULL,
				username      VARCHAR     NOT NULL UNIQUE,
				password_hash VARCHAR     NOT NULL,
				created_at    TIMESTAMPTZ NOT NULL,
				modified_at   TIMESTAMPTZ NOT NULL,
				folder_id     UUID        NOT NULL
			);
		
			CREATE TABLE IF NOT EXISTS paths (
				id      UUID    PRIMARY KEY,
				path    VARCHAR NOT NULL,
				user_id UUID    NOT NULL REFERENCES users(id),
				
				UNIQUE (path, user_id)
			);

			CREATE TABLE IF NOT EXISTS folders (
				id          UUID        PRIMARY KEY,
				name        VARCHAR     NOT NULL,
				path_id     UUID        NOT NULL REFERENCES paths(id),
				size        INTEGER     NOT NULL,
				is_trashed  BOOLEAN     NOT NULL,
				created_at  TIMESTAMPTZ NOT NULL,
				modified_at TIMESTAMPTZ NOT NULL,
				parent_id   UUID        NOT NULL,
				user_id     UUID        NOT NULL REFERENCES users(id)
			);

			CREATE UNIQUE INDEX non_trashed_unique_folder
			ON folders(name, path_id, user_id)
			WHERE is_trashed = false;

			CREATE TYPE filetype AS ENUM(
				'Unknown',
				'Image',
				'Audio',
				'Video',
				'Document',
				'Archive'
			);

			CREATE TABLE IF NOT EXISTS files (
				id          UUID        PRIMARY KEY,
				name        VARCHAR     NOT NULL,
				extension   VARCHAR     NOT NULL,
				size        INTEGER     NOT NULL,
				type        filetype    NOT NULL,
				is_trashed  BOOLEAN     NOT NULL,
				created_at  TIMESTAMPTZ NOT NULL,
				modified_at TIMESTAMPTZ NOT NULL,
				folder_id   UUID        NOT NULL REFERENCES folders(id),
				user_id     UUID        NOT NULL REFERENCES users(id)
			);

			CREATE UNIQUE INDEX non_trashed_unique_file
			ON files(name, extension, folder_id, user_id)
			WHERE is_trashed = false;
			""";
		
		Execute.Sql(query);
	}
	
	public override void Down()
	{
		const string query = """
			DROP TABLE IF EXISTS files, folders, paths, users;
			DROP TYPE IF EXISTS filetype;
			""";
		
		Execute.Sql(query);
	}
}