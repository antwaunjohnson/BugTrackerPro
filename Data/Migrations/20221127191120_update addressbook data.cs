﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTrackerPro.Data.Migrations
{
    public partial class updateaddressbookdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "TicketComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "TicketComments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }
    }
}
