using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WPF_Audio.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLikedToAudioTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLiked",
                table: "AudioTracks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLiked",
                table: "AudioTracks");
        }
    }
}
