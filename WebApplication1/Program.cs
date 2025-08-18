using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Controller
builder.Services.AddControllers();

// Đăng ký Swagger (giúp test API trực tiếp trên trình duyệt)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Chỉ bật Swagger ở môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bật HTTPS redirection
app.UseHttpsRedirection();

// Bật Authorization (nếu có dùng)
app.UseAuthorization();

// Map các controller để API chạy
app.MapControllers();

app.Run();
