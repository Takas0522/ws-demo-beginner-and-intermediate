using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ─── App1: サービスダッシュボード ─────────────────────────────────────────────

var app1Db = builder
    .AddPostgres("app1-postgres")
    .WithHostPort(5433)
    .AddDatabase("app1-db");

var app1Backend = builder
    .AddProject<Projects.App1Backend>("app1-backend")
    .WithReference(app1Db)
    .WithHttpEndpoint(port: 5001)
    .WaitFor(app1Db);

var app1Frontend = builder
    .AddNpmApp("app1-frontend", "../../src/app1-service-dashboard/frontend", "dev")
    .WithHttpEndpoint(port: 3001, env: "PORT")
    .WithReference(app1Backend)
    .WaitFor(app1Backend);

// ─── App2: 開発状況確認ダッシュボード ──────────────────────────────────────────

var app2Db = builder
    .AddPostgres("app2-postgres")
    .WithHostPort(5434)
    .AddDatabase("app2-db");

var app2Backend = builder
    .AddProject<Projects.App2Backend>("app2-backend")
    .WithReference(app2Db)
    .WithHttpEndpoint(port: 5002)
    .WaitFor(app2Db);

var app2Frontend = builder
    .AddNpmApp("app2-frontend", "../../src/app2-dev-dashboard/frontend", "dev")
    .WithHttpEndpoint(port: 3002, env: "PORT")
    .WithReference(app2Backend)
    .WaitFor(app2Backend);

await builder.Build().RunAsync();
