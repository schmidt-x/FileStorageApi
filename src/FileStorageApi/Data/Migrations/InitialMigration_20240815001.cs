using FluentMigrator;

namespace FileStorageApi.Data.Migrations;

[Migration(20240815001)]
public class InitialMigration_20240815001 : Migration
{
	public override void Up()
	{
		const string query = """
			CREATE TABLE IF NOT EXISTS "user" (
				id            UUID PRIMARY KEY,
				email         VARCHAR(254) NOT NULL UNIQUE,
				is_confirmed  BOOLEAN      NOT NULL,
				username      VARCHAR(32)  NOT NULL UNIQUE,
				password_hash VARCHAR(60)  NOT NULL,
				created_at    TIMESTAMP    NOT NULL,
				modified_at   TIMESTAMP    NOT NULL,
				folder_id     UUID         NOT NULL UNIQUE
			);
		
			CREATE TABLE IF NOT EXISTS folder (
				id          UUID PRIMARY KEY,
				name        VARCHAR(64) NOT NULL,
				path        VARCHAR     NOT NULL,
				size        INTEGER     NOT NULL,
				is_trashed  BOOLEAN     NOT NULL,
				created_at  TIMESTAMP   NOT NULL,
				modified_at TIMESTAMP   NOT NULL,
				parent_id   UUID        NOT NULL,
				user_id     UUID        NOT NULL,
					
				UNIQUE (name, path, user_id),
				FOREIGN KEY (user_id) REFERENCES "user"(id)
			);
			
			CREATE table if not exists file (
				id          UUID PRIMARY KEY,
				name        VARCHAR(64) NOT NULL,
				size        INTEGER     NOT NULL,
				extension   VARCHAR(32) NOT NULL,
				is_trashed  BOOLEAN     NOT NULL,
				created_at  TIMESTAMP   NOT NULL,
				modified_at TIMESTAMP   NOT NULL,
				folder_id   UUID        NOT NULL,
				user_id     UUID        NOT NULL,
				
				UNIQUE (name, folder_id, user_id),
				FOREIGN KEY (folder_id) REFERENCES folder(id),
				FOREIGN KEY (user_id)   REFERENCES "user"(id)
			);
			""";
		
		Execute.Sql(query);
	}
	
	public override void Down()
	{
		const string query = """
			DROP TABLE IF EXISTS file, folder, "user";
			""";
		
		Execute.Sql(query);
	}
}