using FluentMigrator;

namespace FileStorageApi.Data.Migrations;

[Migration(20240820_01)]
public class InitialMigration_20240820_01 : Migration
{
	public override void Up()
	{
		const string query = """
			CREATE TABLE IF NOT EXISTS users (
				id            UUID      PRIMARY KEY,
				email         VARCHAR   NOT NULL UNIQUE,
				is_confirmed  BOOLEAN   NOT NULL,
				username      VARCHAR   NOT NULL UNIQUE,
				password_hash VARCHAR   NOT NULL,
				created_at    TIMESTAMP NOT NULL,
				modified_at   TIMESTAMP NOT NULL,
				folder_id     UUID      NOT NULL
			);
		
			CREATE TABLE IF NOT EXISTS paths (
				id      UUID    PRIMARY KEY,
				path    VARCHAR NOT NULL,
				user_id UUID    NOT NULL,
				
				UNIQUE (path, user_id),
				FOREIGN KEY (user_id) REFERENCES users(id)
			);

			CREATE TABLE IF NOT EXISTS folders (
				id          UUID      PRIMARY KEY,
				name        VARCHAR   NOT NULL,
				path_id     UUID      NOT NULL,
				size        INTEGER   NOT NULL,
				is_trashed  BOOLEAN   NOT NULL,
				created_at  TIMESTAMP NOT NULL,
				modified_at TIMESTAMP NOT NULL,
				parent_id   UUID      NOT NULL,
				user_id     UUID      NOT NULL,
					
				UNIQUE (name, path_id, user_id),
				FOREIGN KEY (path_id) REFERENCES paths(id),
				FOREIGN KEY (user_id) REFERENCES users(id)
			);
			
			CREATE TYPE filetype AS ENUM(
				'Unknown',
				'Image',
				'Audio',
				'Video',
				'Document'
			);

			CREATE TABLE IF NOT EXISTS files (
				id          UUID      PRIMARY KEY,
				name        VARCHAR   NOT NULL,
				extension   VARCHAR   NOT NULL,
				size        INTEGER   NOT NULL,
				type        filetype  NOT NULL,
				is_trashed  BOOLEAN   NOT NULL,
				created_at  TIMESTAMP NOT NULL,
				modified_at TIMESTAMP NOT NULL,
				folder_id   UUID      NOT NULL,
				user_id     UUID      NOT NULL,
				
				UNIQUE (name, folder_id, user_id),
				FOREIGN KEY (folder_id) REFERENCES folders(id),
				FOREIGN KEY (user_id)   REFERENCES users(id)
			);
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