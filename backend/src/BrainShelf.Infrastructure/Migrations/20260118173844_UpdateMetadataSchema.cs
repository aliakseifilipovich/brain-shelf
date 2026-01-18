using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrainShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMetadataSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedAt",
                table: "Metadata");

            migrationBuilder.RenameColumn(
                name: "PreviewImageUrl",
                table: "Metadata",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "PageTitle",
                table: "Metadata",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "MetaDescription",
                table: "Metadata",
                newName: "FaviconUrl");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Metadata",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Metadata",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteName",
                table: "Metadata",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "SiteName",
                table: "Metadata");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Metadata",
                newName: "PageTitle");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Metadata",
                newName: "PreviewImageUrl");

            migrationBuilder.RenameColumn(
                name: "FaviconUrl",
                table: "Metadata",
                newName: "MetaDescription");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExtractedAt",
                table: "Metadata",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
