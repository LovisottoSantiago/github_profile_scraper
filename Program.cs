using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Program
{
    static async Task Main(string[] args)
    {
        string username = "LovisottoSantiago"; 
        var repositories = await GetGitHubRepositories(username);

        // Filtrar por Topics
        var filteredRepositories = FilterRepositories(repositories, new List<string> { "portfolio-project", "university-project" });

        // Convertir a JSON
        string json = JsonConvert.SerializeObject(filteredRepositories, Formatting.Indented);
        Console.WriteLine(json);
    }

    static async Task<List<Repo>> GetGitHubRepositories(string username)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.github.com/users/{username}/repos";
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // Necesario para que la API funcione

            var response = await client.GetStringAsync(url);
            var repositories = JsonConvert.DeserializeObject<List<Repo>>(response);

            // Obtener Topics para cada repositorio
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var repo in repositories)
            {
                repo.Topics = await GetRepoTopics(client, username, repo.Name);
            }
            #pragma warning restore CS8602 // Dereference of a possibly null reference.

            return repositories;
        }
    }

    static async Task<List<string>> GetRepoTopics(HttpClient client, string username, string repoName)
    {
        string url = $"https://api.github.com/repos/{username}/{repoName}/topics";
        client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

        var response = await client.GetStringAsync(url);
        var topicsResponse = JsonConvert.DeserializeObject<TopicsResponse>(response);
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        return topicsResponse.Topics;
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    static List<Repo> FilterRepositories(List<Repo> repositories, List<string> keywords)
    {
        var filtered = new List<Repo>();
        foreach (var repo in repositories)
        {
            // Verifica si alguno de los Topics contiene alguna de las palabras clave
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
