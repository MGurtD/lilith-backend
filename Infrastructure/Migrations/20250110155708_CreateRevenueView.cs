using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class CreateRevenueView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE OR REPLACE VIEW public.vw_monthlyrevenue\r\n AS\r\n SELECT a.\"Year\",\r\n    a.\"Month\",\r\n    sum(a.\"OutcomeAmount\") AS \"OutcomeAmount\",\r\n    sum(a.\"ExpenseAmount\") AS \"ExpenseAmount\",\r\n    sum(a.\"IncomeAmount\") AS \"IncomeAmount\",\r\n    sum(a.\"IncomeAmount\") - (sum(a.\"ExpenseAmount\") + sum(a.\"OutcomeAmount\")) AS \"RevenueAmount\"\r\n   FROM ( SELECT date_part('year'::text, \"PurchaseInvoices\".\"PurchaseInvoiceDate\") AS \"Year\",\r\n            date_part('month'::text, \"PurchaseInvoices\".\"PurchaseInvoiceDate\") AS \"Month\",\r\n            sum(\"PurchaseInvoices\".\"NetAmount\") AS \"OutcomeAmount\",\r\n            0.0 AS \"IncomeAmount\",\r\n            0.0 AS \"ExpenseAmount\"\r\n           FROM \"PurchaseInvoices\"\r\n          GROUP BY (date_part('year'::text, \"PurchaseInvoices\".\"PurchaseInvoiceDate\")), (date_part('month'::text, \"PurchaseInvoices\".\"PurchaseInvoiceDate\"))\r\n        UNION ALL\r\n         SELECT date_part('year'::text, \"SalesInvoice\".\"InvoiceDate\") AS \"Year\",\r\n            date_part('month'::text, \"SalesInvoice\".\"InvoiceDate\") AS \"Month\",\r\n            0.0 AS \"OutcomeAmount\",\r\n            sum(\"SalesInvoice\".\"NetAmount\") AS \"IncomeAmount\",\r\n            0.0 AS \"ExpenseAmount\"\r\n           FROM \"SalesInvoice\"\r\n          GROUP BY (date_part('year'::text, \"SalesInvoice\".\"InvoiceDate\")), (date_part('month'::text, \"SalesInvoice\".\"InvoiceDate\"))\r\n        UNION ALL\r\n         SELECT date_part('year'::text, \"Expenses\".\"PaymentDate\") AS \"Year\",\r\n            date_part('month'::text, \"Expenses\".\"PaymentDate\") AS \"Month\",\r\n            0.0 AS \"OutcomeAmount\",\r\n            0.0 AS \"IncomeAmount\",\r\n            sum(\"Expenses\".\"Amount\") AS \"ExpenseAmount\"\r\n           FROM \"Expenses\"\r\n          GROUP BY (date_part('year'::text, \"Expenses\".\"PaymentDate\")), (date_part('month'::text, \"Expenses\".\"PaymentDate\"))) a\r\n  GROUP BY a.\"Year\", a.\"Month\"\r\n  ORDER BY a.\"Year\", a.\"Month\";\r\n\r\nALTER TABLE public.vw_monthlyrevenue\r\n    OWNER TO ubuntu;\r\n\r\n");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW public.vw_monthlyrevenue;");
        }
    }
}
