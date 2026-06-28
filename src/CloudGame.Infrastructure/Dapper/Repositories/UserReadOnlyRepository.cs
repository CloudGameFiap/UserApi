using CloudGame.Domain.Commom;
using CloudGame.Domain.Entities;
using CloudGame.Domain.Interfaces;
using CloudGame.Domain.Parameters;
using CloudGame.Infrastructure.Dapper.Contracts;
using Dapper;
using System.Data;

namespace CloudGame.Infrastructure.Dapper.Repositories;

public sealed class UserReadOnlyRepository(IDapperContext context)
    : AbstractRepository<User, int>(context), IUserReadOnlyRepository
{
    public async Task<bool> CheckIfIsEmailBeingUsedAsync(string email)
    {
        using IDbConnection connection = Context.OpenConnection();
        var result = await connection.QueryFirstOrDefaultAsync<int>("SELECT TOP 1 COUNT(1) FROM Users WHERE Email = @Email", new { Email = email });
        return result > 0;
    }

    public async Task<Pagination<User>> FindAsync(FindUsersParameter parameters)
    {
        var sqlBuilder = new SqlBuilder();

        if (parameters.Active.HasValue)
            sqlBuilder.Where("Active=@active", new { active = parameters.Active });

        if (parameters.IsAdmin.HasValue)
            sqlBuilder.Where("IsAdmin=@isAdmin", new { isAdmin = parameters.IsAdmin });

        if (!string.IsNullOrWhiteSpace(parameters.Name))
            sqlBuilder.Where("Name LIKE @name", new { name = $"%{parameters.Name}%" });

        var countQuery = sqlBuilder.AddTemplate("Select count(*) from Users /**where**/");
        var queryUsers = sqlBuilder.AddTemplate(@"select Id, Name, Email, Active, IsAdmin from Users
                                                /**where**/ ORDER BY Id OFFSET @skip ROWS FETCH NEXT @size ROWS ONLY",
                                                new { skip = parameters.Skip, size = parameters.PageSize });

        using IDbConnection connection = Context.OpenConnection();

        int count = await connection.ExecuteScalarAsync<int>(countQuery.RawSql, countQuery.Parameters);
        var users = (await connection.QueryAsync<User>(queryUsers.RawSql, queryUsers.Parameters)).ToList();

        return new Pagination<User>(users, count);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var parameters = new DynamicParameters();
        parameters.Add("email", email, DbType.String, size: 120);

        using IDbConnection connection = Context.OpenConnection();
        return await connection.QueryFirstOrDefaultAsync<User>("SELECT Id,Name,Email,Password,Active,IsAdmin FROM Users WHERE Email = @email", parameters);
    }

    public override async Task<User> GetByIdAsync(int id)
    {
        using IDbConnection connection = Context.OpenConnection();
        return await connection.QueryFirstOrDefaultAsync<User>("SELECT Id,Name,Email,Active,Password,IsAdmin,BirthDate,CreatedAt,UpdateAt FROM Users WHERE Id = @id", new { id });
    }
}
