﻿using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Server.Data;

public class ServerContext : DbContext
{
    public ServerContext (DbContextOptions<ServerContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get;  set; }
}