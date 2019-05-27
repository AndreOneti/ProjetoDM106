namespace ProjetoDM106.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderItemAndOrderUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "userEmail", c => c.String());
            AddColumn("dbo.Orders", "DateOrder", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "DateDelivery", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "Status", c => c.String());
            AddColumn("dbo.Orders", "ItemPrice", c => c.Single(nullable: false));
            AddColumn("dbo.Orders", "ItemWeight", c => c.Single(nullable: false));
            AddColumn("dbo.OrderItems", "TotalItens", c => c.Int(nullable: false));
            DropColumn("dbo.Orders", "userName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "userName", c => c.String());
            DropColumn("dbo.OrderItems", "TotalItens");
            DropColumn("dbo.Orders", "ItemWeight");
            DropColumn("dbo.Orders", "ItemPrice");
            DropColumn("dbo.Orders", "Status");
            DropColumn("dbo.Orders", "DateDelivery");
            DropColumn("dbo.Orders", "DateOrder");
            DropColumn("dbo.Orders", "userEmail");
        }
    }
}
