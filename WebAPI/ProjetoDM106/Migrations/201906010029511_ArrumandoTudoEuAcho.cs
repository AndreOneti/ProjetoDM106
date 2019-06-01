namespace ProjetoDM106.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArrumandoTudoEuAcho : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "DateDelivery", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "DateDelivery", c => c.DateTime(nullable: false));
        }
    }
}
