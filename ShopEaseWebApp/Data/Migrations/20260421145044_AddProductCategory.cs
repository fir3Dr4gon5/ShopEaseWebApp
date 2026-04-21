using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopEaseWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Accessories");

            migrationBuilder.Sql(
                """
                UPDATE Products
                SET Category = CASE Name
                    WHEN 'Men''s White T-Shirt' THEN 'Shirt'
                    WHEN 'Women''s Jeans' THEN 'Clothing'
                    WHEN 'Leather Wallet' THEN 'Leather'
                    WHEN 'Sunglasses' THEN 'Accessories'
                    WHEN 'Canvas Sneakers' THEN 'Footwear'
                    WHEN 'Wool Scarf' THEN 'Accessories'
                    WHEN 'Leather Belt' THEN 'Leather'
                    WHEN 'Hooded Sweatshirt' THEN 'Shirt'
                    ELSE 'Accessories'
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");
        }
    }
}
