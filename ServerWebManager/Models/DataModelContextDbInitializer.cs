using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ServerWebManager.Models
{
    public class DataModelContextDbInitializer : DropCreateDatabaseIfModelChanges<DataModel>
    {
        protected override void Seed(DataModel context)
        {
            context.ServerSet.Add(new ServerSet() { Name = "Minimo", Description = "Minimo", Port = "2112", Private = false, RelativePath = @"\Minimo" });
            context.ServerSet.Add(new ServerSet() { Name = "RHS", Description = "RHS", Port = "2212", Private = false, RelativePath = @"\RHS" });
            context.ServerSet.Add(new ServerSet() { Name = "CUP", Description = "CUP", Port = "2312", Private = false, RelativePath = @"\CUP" });
            context.ServerSet.Add(new ServerSet() { Name = "Test #1", Description = "Servidor de Test", Port = "2412", Private = false, RelativePath = @"\Test1" });
            context.ServerSet.Add(new ServerSet() { Name = "2GM", Description = "Segunda Guerra Mundial", Port = "2512", Private = false, RelativePath = @"\2GM" });
            context.ServerSet.Add(new ServerSet() { Name = "Vietnam", Description = "Vietnam", Port = "2612", Private = false, RelativePath = @"\Vietnam" });

            context.SaveChanges();

            base.Seed(context);
        }
       
    }
}