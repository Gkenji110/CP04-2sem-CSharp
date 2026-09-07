using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoBiblioteca.Migrations
{
    /// <inheritdoc />
    public partial class PrimeiraMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AUTORES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    NACIONALIDADE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUTORES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "LIVROS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TITULO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ANOPUBLICACAO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EDITORA = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LIVROS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AUTORESLIVROS",
                columns: table => new
                {
                    AUTORID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LIVROID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUTORESLIVROS", x => new { x.AUTORID, x.LIVROID });
                    table.ForeignKey(
                        name: "FK_AUTORESLIVROS_AUTORES_AUTORID",
                        column: x => x.AUTORID,
                        principalTable: "AUTORES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AUTORESLIVROS_LIVROS_LIVROID",
                        column: x => x.LIVROID,
                        principalTable: "LIVROS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AUTORESLIVROS_LIVROID",
                table: "AUTORESLIVROS",
                column: "LIVROID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUTORESLIVROS");

            migrationBuilder.DropTable(
                name: "AUTORES");

            migrationBuilder.DropTable(
                name: "LIVROS");
        }
    }
}
