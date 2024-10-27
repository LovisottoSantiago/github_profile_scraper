using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

public class Program
{
    static async Task Main(string[] args)
    {
        string username = "LovisottoSantiago";
        #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        var repositories = await GetGitHubRepositories(username, token); // Pasa el token aquí

        // Filtrar por Topics
        var filteredRepositories = FilterRepositories(repositories, new List<string> { "portfolio-project", "university-project" });

        // Convertir a JSON
        string json = JsonConvert.SerializeObject(filteredRepositories, Formatting.Indented);
        Console.WriteLine(json);
        System.IO.File.WriteAllText("repositorios.json", json);
    }

    static async Task<List<Repo>> GetGitHubRepositories(string username, string? token) // El token es nullable
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token), "El token de GitHub no puede ser nulo.");
        }

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);

            string url = $"https://api.github.com/users/{username}/repos";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // Necesario para que la API funcione

            var response = await client.GetStringAsync(url);
            var repositories = JsonConvert.DeserializeObject<List<Repo>>(response);

            // Obtener Topics para cada repositorio
            foreach (var repo in repositories)
            {
                repo.Topics = await GetRepoTopics(client, username, repo.Name);
                repo.Images = new List<string>(); // Espacio en blanco para imágenes
            }

            return repositories;
        }
    }

    static async Task<List<string>> GetRepoTopics(HttpClient client, string username, string repoName)
    {
        string url = $"https://api.github.com/repos/{username}/{repoName}/topics";
        client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

        var response = await client.GetStringAsync(url);
        var topicsResponse = JsonConvert.DeserializeObject<TopicsResponse>(response);
        return topicsResponse.Topics;
    }

    static List<Repo> FilterRepositories(List<Repo> repositories, List<string> keywords)
    {
        var filtered = new List<Repo>();
        foreach (var repo in repositories)
        {
            if (repo.Topics != null && repo.Topics.Any(topic => keywords.Any(keyword => topic.Contains(keyword, StringComparison.OrdinalIgnoreCase))))
            {
                filtered.Add(repo);
            }
        }
        return filtered;
    }
}

public class TopicsResponse
{
    [JsonProperty("names")]
    public required List<string> Topics { get; set; }
}
