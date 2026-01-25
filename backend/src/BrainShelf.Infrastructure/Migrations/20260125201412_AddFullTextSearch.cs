using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrainShelf.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add search vector columns for entries
            migrationBuilder.Sql(@"
                ALTER TABLE ""Entries"" 
                ADD COLUMN ""SearchVector"" tsvector;
            ");

            // Create function to update search vector
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION entries_search_vector_update() 
                RETURNS trigger AS $$
                BEGIN
                    NEW.""SearchVector"" := 
                        setweight(to_tsvector('english', COALESCE(NEW.""Title"", '')), 'A') ||
                        setweight(to_tsvector('english', COALESCE(NEW.""Content"", '')), 'B') ||
                        setweight(to_tsvector('english', COALESCE(NEW.""Url"", '')), 'C') ||
                        setweight(to_tsvector('russian', COALESCE(NEW.""Title"", '')), 'A') ||
                        setweight(to_tsvector('russian', COALESCE(NEW.""Content"", '')), 'B');
                    RETURN NEW;
                END
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger to automatically update search vector
            migrationBuilder.Sql(@"
                CREATE TRIGGER entries_search_vector_trigger
                BEFORE INSERT OR UPDATE ON ""Entries""
                FOR EACH ROW
                EXECUTE FUNCTION entries_search_vector_update();
            ");

            // Update existing rows
            migrationBuilder.Sql(@"
                UPDATE ""Entries""
                SET ""SearchVector"" = 
                    setweight(to_tsvector('english', COALESCE(""Title"", '')), 'A') ||
                    setweight(to_tsvector('english', COALESCE(""Content"", '')), 'B') ||
                    setweight(to_tsvector('english', COALESCE(""Url"", '')), 'C') ||
                    setweight(to_tsvector('russian', COALESCE(""Title"", '')), 'A') ||
                    setweight(to_tsvector('russian', COALESCE(""Content"", '')), 'B');
            ");

            // Create GIN index for full-text search
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_Entries_SearchVector"" 
                ON ""Entries"" 
                USING GIN(""SearchVector"");
            ");

            // Add search vector columns for metadata
            migrationBuilder.Sql(@"
                ALTER TABLE ""Metadata"" 
                ADD COLUMN ""SearchVector"" tsvector;
            ");

            // Create function to update metadata search vector
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION metadata_search_vector_update() 
                RETURNS trigger AS $$
                BEGIN
                    NEW.""SearchVector"" := 
                        setweight(to_tsvector('english', COALESCE(NEW.""Title"", '')), 'A') ||
                        setweight(to_tsvector('english', COALESCE(NEW.""Description"", '')), 'B') ||
                        setweight(to_tsvector('english', COALESCE(NEW.""Keywords"", '')), 'C') ||
                        setweight(to_tsvector('english', COALESCE(NEW.""Author"", '')), 'D') ||
                        setweight(to_tsvector('russian', COALESCE(NEW.""Title"", '')), 'A') ||
                        setweight(to_tsvector('russian', COALESCE(NEW.""Description"", '')), 'B');
                    RETURN NEW;
                END
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger for metadata
            migrationBuilder.Sql(@"
                CREATE TRIGGER metadata_search_vector_trigger
                BEFORE INSERT OR UPDATE ON ""Metadata""
                FOR EACH ROW
                EXECUTE FUNCTION metadata_search_vector_update();
            ");

            // Update existing metadata rows
            migrationBuilder.Sql(@"
                UPDATE ""Metadata""
                SET ""SearchVector"" = 
                    setweight(to_tsvector('english', COALESCE(""Title"", '')), 'A') ||
                    setweight(to_tsvector('english', COALESCE(""Description"", '')), 'B') ||
                    setweight(to_tsvector('english', COALESCE(""Keywords"", '')), 'C') ||
                    setweight(to_tsvector('english', COALESCE(""Author"", '')), 'D') ||
                    setweight(to_tsvector('russian', COALESCE(""Title"", '')), 'A') ||
                    setweight(to_tsvector('russian', COALESCE(""Description"", '')), 'B');
            ");

            // Create GIN index for metadata search
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_Metadata_SearchVector"" 
                ON ""Metadata"" 
                USING GIN(""SearchVector"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop metadata search infrastructure
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Metadata_SearchVector"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS metadata_search_vector_trigger ON ""Metadata"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS metadata_search_vector_update();");
            migrationBuilder.Sql(@"ALTER TABLE ""Metadata"" DROP COLUMN IF EXISTS ""SearchVector"";");

            // Drop entries search infrastructure
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Entries_SearchVector"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS entries_search_vector_trigger ON ""Entries"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS entries_search_vector_update();");
            migrationBuilder.Sql(@"ALTER TABLE ""Entries"" DROP COLUMN IF EXISTS ""SearchVector"";");
        }
    }
}
