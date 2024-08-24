using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class ADDViewProductionCosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.\"vw_productioncosts\"\r\nAS\r\nSELECT \tpp.\"Date\",\r\n\t\tDATE_PART('year', pp.\"Date\") as \"Year\",\r\n\t\tDATE_PART('month', pp.\"Date\") as \"Month\",\r\n\t\tDATE_PART('week', pp.\"Date\") as \"Week\",\r\n\t\tpp.\"WorkcenterTime\",\r\n\t\tpp.\"MachineHourCost\",\r\n\t\tROUND((pp.\"WorkcenterTime\"::decimal/60)*pp.\"MachineHourCost\", 2) as \"PartWorkcenterCost\",\r\n\t\tpp.\"OperatorTime\",\r\n\t\tpp.\"OperatorHourCost\",\r\n\t\tROUND((pp.\"OperatorTime\"::decimal/60)*pp.\"OperatorHourCost\", 2) as \"PartOperatorCost\",\r\n\t\twc.\"Name\" as \"WorkcenterName\",\r\n\t\twt.\"Name\" as \"WorkcenterTypeName\"\r\n\tFROM public.\"ProductionParts\" pp\r\n\t\tINNER JOIN public.\"Workcenters\" wc ON pp.\"WorkcenterId\" = wc.\"Id\"\r\n\t\tINNER JOIN public.\"WorkcenterTypes\" wt ON wc.\"WorkcenterTypeId\" = wt.\"Id\"\r\nORDER BY pp.\"Date\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW public.""vw_productioncosts""");
        }
    }
}
