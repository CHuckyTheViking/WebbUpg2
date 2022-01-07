using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebbUpg2.Models;

namespace WebbUpg2.Data
{
    public class WebbUpg2Context : DbContext
    {
        public WebbUpg2Context (DbContextOptions<WebbUpg2Context> options)
            : base(options)
        {
        }

        public DbSet<Comment> Comment { get; set; }
        public DbSet<DataFile> DataFile { get; set; }
    }
}
