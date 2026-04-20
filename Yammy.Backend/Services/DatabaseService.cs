using MySql.Data.MySqlClient;
using Yammy.Backend.Models;
using System.Data.Common;
using System.Text;

namespace Yammy.Backend.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Port=3306;Database=yammy_db;User Id=yammy_user;Password=yammy_password;Charset=utf8mb4;";
    }

    private async Task EnsureUtf8Async(MySqlConnection connection)
    {
        using var setNamesCmd = new MySqlCommand("SET NAMES 'utf8mb4'", connection);
        await setNamesCmd.ExecuteNonQueryAsync();
    }

    

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var query = "SELECT * FROM users WHERE email = @email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", email);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    FullName = (reader.GetString(reader.GetOrdinal("full_name"))),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                    Role = reader.GetString(reader.GetOrdinal("role")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    AvatarColor = reader.GetString(reader.GetOrdinal("avatar_color"))
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserByEmailAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<User> CreateUserAsync(User user, string passwordHash)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var query = @"INSERT INTO users (full_name, email, password_hash, phone, role, avatar_color) 
                         VALUES (@fullName, @email, @passwordHash, @phone, @role, @avatarColor);
                         SELECT LAST_INSERT_ID();";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@fullName", user.FullName);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@phone", user.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@role", "user");
            command.Parameters.AddWithValue("@avatarColor", user.AvatarColor);
            user.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateUserAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Restaurant>> GetAllRestaurantsAsync()
    {
        var restaurants = new List<Restaurant>();
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var query = "SELECT * FROM restaurants ORDER BY name";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                restaurants.Add(MapRestaurant(reader));
            return restaurants;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllRestaurantsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Restaurant>> GetFilteredRestaurantsAsync(
        string? search,
        string? cuisineType,
        string? restaurantType,
        string? priceRange,
        decimal? minPrice,
        decimal? maxPrice,
        double? minRating)
    {
        var restaurants = new List<Restaurant>();
        var conditions = new List<string>();
        var parameters = new List<MySqlParameter>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add("LOWER(name) LIKE @search");
            parameters.Add(new MySqlParameter("@search", $"%{search.ToLower()}%"));
        }

        if (!string.IsNullOrWhiteSpace(cuisineType))
        {
            Console.WriteLine($"[FILTER] cuisineType = '{cuisineType}'");
            conditions.Add("cuisine_type LIKE @cuisineType");
            parameters.Add(new MySqlParameter("@cuisineType", "%" + cuisineType + "%"));
        }

        if (!string.IsNullOrWhiteSpace(restaurantType))
        {
            conditions.Add("restaurant_type = @restaurantType");
            parameters.Add(new MySqlParameter("@restaurantType", restaurantType));
        }

        if (!string.IsNullOrWhiteSpace(priceRange))
        {
            conditions.Add("price_range = @priceRange");
            parameters.Add(new MySqlParameter("@priceRange", priceRange));
        }

        if (minPrice.HasValue)
        {
            conditions.Add("LENGTH(price_range) - LENGTH(REPLACE(price_range, '$', '')) >= @minPriceCount");
            parameters.Add(new MySqlParameter("@minPriceCount", minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            conditions.Add("LENGTH(price_range) - LENGTH(REPLACE(price_range, '$', '')) <= @maxPriceCount");
            parameters.Add(new MySqlParameter("@maxPriceCount", maxPrice.Value));
        }

        if (minRating.HasValue)
        {
            conditions.Add("rating >= @minRating");
            parameters.Add(new MySqlParameter("@minRating", minRating.Value));
        }

        string whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
        string query = $"SELECT * FROM restaurants {whereClause} ORDER BY name";

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

using (var debugCmd = new MySqlCommand("SELECT DISTINCT cuisine_type FROM restaurants", connection))
using (var debugReader = await debugCmd.ExecuteReaderAsync())
{
    while (await debugReader.ReadAsync())
    {
        var val = debugReader.GetString(0);
        Console.WriteLine($"DEBUG: cuisine_type in DB = '{val}', length={val.Length}, bytes={BitConverter.ToString(Encoding.UTF8.GetBytes(val))}");
    }
}

            await EnsureUtf8Async(connection);
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddRange(parameters.ToArray());
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                restaurants.Add(MapRestaurant(reader));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFilteredRestaurantsAsync: {ex.Message}");
            throw;
        }
        return restaurants;
    }

    public async Task<List<Restaurant>> SearchRestaurantsByNameAsync(string query)
    {
        var restaurants = new List<Restaurant>();
        string sql = @"SELECT * FROM restaurants 
                       WHERE LOWER(name) LIKE @pattern 
                       ORDER BY 
                           CASE WHEN LOWER(name) = @exact THEN 1 ELSE 2 END,
                           name
                       LIMIT 10";
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@pattern", $"%{query.ToLower()}%");
            command.Parameters.AddWithValue("@exact", query.ToLower());
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                restaurants.Add(MapRestaurant(reader));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SearchRestaurantsByNameAsync: {ex.Message}");
            throw;
        }
        return restaurants;
    }

    public async Task<Restaurant?> GetRestaurantByIdAsync(int id)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var query = "SELECT * FROM restaurants WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapRestaurant(reader);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRestaurantByIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<Restaurant> AddRestaurantAsync(Restaurant restaurant)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var query = @"INSERT INTO restaurants (name, description, cuisine_type, restaurant_type, price_range, phone, city, street, house, opening_date, features, image_url) 
                         VALUES (@name, @description, @cuisineType, @restaurantType, @priceRange, @phone, @city, @street, @house, @openingDate, @features, @imageUrl);
                         SELECT LAST_INSERT_ID();";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", restaurant.Name);
            command.Parameters.AddWithValue("@description", restaurant.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@cuisineType", restaurant.CuisineType ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@restaurantType", restaurant.RestaurantType ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@priceRange", restaurant.PriceRange ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", restaurant.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", restaurant.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@street", restaurant.Street ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@house", restaurant.House ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@openingDate", restaurant.OpeningDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@features", restaurant.Features ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@imageUrl", restaurant.ImageUrl ?? "https://via.placeholder.com/300x200?text=No+Image");
            restaurant.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return restaurant;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddRestaurantAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteRestaurantAsync(int id)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "DELETE FROM restaurants WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteRestaurantAsync: {ex.Message}");
            return false;
        }
    }

    private Restaurant MapRestaurant(DbDataReader reader)
    {
        return new Restaurant
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = (reader.GetString(reader.GetOrdinal("name"))),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : (reader.GetString(reader.GetOrdinal("description"))),
            ShortDescription = reader.IsDBNull(reader.GetOrdinal("short_description")) ? null : (reader.GetString(reader.GetOrdinal("short_description"))),
            CuisineType = reader.IsDBNull(reader.GetOrdinal("cuisine_type")) ? null : (reader.GetString(reader.GetOrdinal("cuisine_type"))),
            RestaurantType = reader.IsDBNull(reader.GetOrdinal("restaurant_type")) ? null : (reader.GetString(reader.GetOrdinal("restaurant_type"))),
            PriceRange = reader.IsDBNull(reader.GetOrdinal("price_range")) ? null : (reader.GetString(reader.GetOrdinal("price_range"))),
            Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
            Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : (reader.GetString(reader.GetOrdinal("address"))),
            City = reader.IsDBNull(reader.GetOrdinal("city")) ? null : (reader.GetString(reader.GetOrdinal("city"))),
            Street = reader.IsDBNull(reader.GetOrdinal("street")) ? null : (reader.GetString(reader.GetOrdinal("street"))),
            House = reader.IsDBNull(reader.GetOrdinal("house")) ? null : reader.GetString(reader.GetOrdinal("house")),
            OpeningDate = reader.IsDBNull(reader.GetOrdinal("opening_date")) ? null : reader.GetDateTime(reader.GetOrdinal("opening_date")),
            Rating = reader.IsDBNull(reader.GetOrdinal("rating")) ? 0 : reader.GetDecimal(reader.GetOrdinal("rating")),
            ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url")),
            Features = reader.IsDBNull(reader.GetOrdinal("features")) ? null : (reader.GetString(reader.GetOrdinal("features"))),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }

    public async Task<List<Review>> GetReviewsByRestaurantIdAsync(int restaurantId)
    {
        var reviews = new List<Review>();
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var query = @"
                SELECT r.*, u.full_name as user_name, u.avatar_color 
                FROM reviews r
                JOIN users u ON r.user_id = u.id
                WHERE r.restaurant_id = @restaurantId
                ORDER BY r.created_at DESC";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@restaurantId", restaurantId);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var review = new Review
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                    RestaurantId = reader.GetInt32(reader.GetOrdinal("restaurant_id")),
                    Rating = reader.GetInt32(reader.GetOrdinal("rating")),
                    Comment = reader.IsDBNull(reader.GetOrdinal("comment")) ? null : (reader.GetString(reader.GetOrdinal("comment"))),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    User = new User
                    {
                        FullName = (reader.GetString(reader.GetOrdinal("user_name"))),
                        AvatarColor = reader.GetString(reader.GetOrdinal("avatar_color"))
                    }
                };
                reviews.Add(review);
            }
            return reviews;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetReviewsByRestaurantIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<Review> AddReviewAsync(int userId, int restaurantId, int rating, string comment)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var checkQuery = "SELECT id FROM reviews WHERE user_id = @userId AND restaurant_id = @restaurantId";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@userId", userId);
            checkCommand.Parameters.AddWithValue("@restaurantId", restaurantId);
            var existing = await checkCommand.ExecuteScalarAsync();
            if (existing != null)
                throw new Exception("Вы уже оставляли отзыв на этот ресторан");
            var query = @"INSERT INTO reviews (user_id, restaurant_id, rating, comment) 
                         VALUES (@userId, @restaurantId, @rating, @comment);
                         SELECT LAST_INSERT_ID();";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@restaurantId", restaurantId);
            command.Parameters.AddWithValue("@rating", rating);
            command.Parameters.AddWithValue("@comment", comment);
            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            await UpdateRestaurantRatingAsync(restaurantId);
            return new Review
            {
                Id = id,
                UserId = userId,
                RestaurantId = restaurantId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddReviewAsync: {ex.Message}");
            throw;
        }
    }

    private async Task UpdateRestaurantRatingAsync(int restaurantId)
    {
        var query = @"UPDATE restaurants r 
                     SET r.rating = (SELECT AVG(rating) FROM reviews WHERE restaurant_id = @restaurantId)
                     WHERE r.id = @restaurantId";
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@restaurantId", restaurantId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Restaurant>> GetFavouritesByUserIdAsync(int userId)
    {
        var restaurants = new List<Restaurant>();
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await EnsureUtf8Async(connection);
            var query = @"
                SELECT r.* FROM restaurants r
                JOIN favourites f ON r.id = f.restaurant_id
                WHERE f.user_id = @userId
                ORDER BY f.created_at DESC";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                restaurants.Add(MapRestaurant(reader));
            return restaurants;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFavouritesByUserIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> AddFavouriteAsync(int userId, int restaurantId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO favourites (user_id, restaurant_id) VALUES (@userId, @restaurantId)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@restaurantId", restaurantId);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddFavouriteAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveFavouriteAsync(int userId, int restaurantId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "DELETE FROM favourites WHERE user_id = @userId AND restaurant_id = @restaurantId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@restaurantId", restaurantId);
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RemoveFavouriteAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsFavouriteAsync(int userId, int restaurantId)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT COUNT(*) FROM favourites WHERE user_id = @userId AND restaurant_id = @restaurantId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@restaurantId", restaurantId);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in IsFavouriteAsync: {ex.Message}");
            return false;
        }
    }
}