using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Createinvoicenotesfieldoncustomerentityv2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNotes",
                table: "Customers",
                type: "varchar",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldDefaultValue: "4000");

            migrationBuilder.Sql("UPDATE public.\"Customers\" SET \"InvoiceNotes\" = ''");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNotes",
                table: "Customers",
                type: "varchar",
                nullable: false,
                defaultValue: "4000",
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 4000);
        }
    }
}
