﻿using FluentMigrator;

namespace App.Data.Migrations;

[Migration(2024073001)]
public class InitialMigration_2024073001 : Migration
{
	public override void Up()
	{
		const string sql = """
			CREATE TABLE IF NOT EXISTS "user" (
				id           UUID PRIMARY KEY,
				email        VARCHAR(254) NOT NULL UNIQUE,
				is_confirmed BOOLEAN      NOT NULL,
				username     VARCHAR(32)  NOT NULL UNIQUE,
				password     VARCHAR(64)  NOT NULL, -- TODO: set to the total length of a hashed password (if the algorithm has it fixed)
				created_at   TIMESTAMP    NOT NULL,
				modified_at  TIMESTAMP    NOT NULL,
				folder_id    UUID         NOT NULL
			);
			""";
		
		Execute.Sql(sql);
	}
	
	public override void Down()
	{
		Execute.Sql("DROP TABLE IF EXISTS \"user\";");
	}
}