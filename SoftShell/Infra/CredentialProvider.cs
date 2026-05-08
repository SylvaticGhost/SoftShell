using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoftShell.Infra;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class CredentialJsonContext : JsonSerializerContext
{
}

public class CredentialProvider
{
    private const string FileName = "secrets.json";
    private Dictionary<string, string>? _credentials;
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);

    public string? GetValue(string key)
    {
        var credentials = GetOrLoadCredentials();
        return credentials.GetValueOrDefault(key);
    }

    private async Task<Dictionary<string, string>> GetOrLoadCredentialsAsync()
    {
        if (_credentials is not null)
            return _credentials;

        if (!File.Exists(_filePath))
        {
            _credentials = new Dictionary<string, string>();
            await ReWriteFileAsync();
            return _credentials;
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            _credentials = await JsonSerializer.DeserializeAsync(stream, CredentialJsonContext.Default.DictionaryStringString);
        }
        catch
        {
            _credentials = null;
        }

        return _credentials ??= new Dictionary<string, string>();
    }

    private Dictionary<string, string> GetOrLoadCredentials()
    {
        if (_credentials is not null)
            return _credentials;

        if (!File.Exists(_filePath))
        {
            _credentials = new Dictionary<string, string>();
            ReWriteFile();
            return _credentials;
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            _credentials = JsonSerializer.Deserialize(stream, CredentialJsonContext.Default.DictionaryStringString);
        }
        catch
        {
            _credentials = null;
        }
        return _credentials ??= new Dictionary<string, string>();
    }
    
    private async Task ReWriteFileAsync() 
    {
        if (_credentials is null) return;
        
        using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, _credentials, CredentialJsonContext.Default.DictionaryStringString);
    }

    private void ReWriteFile()
    {
        if (_credentials is null) return;
        using var stream = File.Create(_filePath);
        JsonSerializer.Serialize(stream, _credentials, CredentialJsonContext.Default.DictionaryStringString);
    }
    
    public async Task<string[]> ListAllAsync() 
    {
        var credentials = await GetOrLoadCredentialsAsync();
        return credentials.Keys.ToArray();
    }

    public async Task SetValueAsync(string key, string value)
    {
        var credentials = await GetOrLoadCredentialsAsync();
        credentials[key] = value;
        await ReWriteFileAsync();
    }
}
