using System.Text.Json;
using Data.Models;
using Data.Models.Interfaces;
using Microsoft.Extensions.Options;

namespace Data; 

public class BlogApiJsonDirectAccess : IBlogApi {
    private readonly BlogApiJsonDirectAccessSetting _settings;

    public BlogApiJsonDirectAccess(IOptions<BlogApiJsonDirectAccessSetting> option) {
        _settings = option.Value;

        if (!Directory.Exists(_settings.DataPath)) {
            Directory.CreateDirectory(_settings.DataPath);
        }
        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.BlogPostsFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.BlogPostsFolder}");
        }
        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.CategoriesFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.CategoriesFolder}");
        }
        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.TagsFolder}"))
        {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.TagsFolder}");
        } 
    }
    
    private List<BlogPost>? _blogPosts;
    private List<Category>? _categories;
    private List<Tag>? _tags;

    private void Load<T>(ref List<T>? list, string folder) {
        if (list == null) {
            list = new List<T>();
            var fullPath = $@"{_settings.DataPath}\{folder}";
            foreach (var file in Directory.GetFiles(fullPath)) {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<T>(json);
                if (data != null) {
                    list.Add(data);
                }
            }
        }
    }

    private Task LoadBlogPostAsync() {
        Load(ref _blogPosts, _settings.BlogPostsFolder);
        return Task.CompletedTask;
    }

    private Task LoadTagsAsync() {
        Load(ref _tags, _settings.TagsFolder);
        return Task.CompletedTask;
    }

    private Task LoadCategoriesAsync() {
        Load(ref _categories, _settings.CategoriesFolder);
        return Task.CompletedTask;
    }

    private async Task SaveAsync<T>(List<T>? list, string folder, string filename, T item) {
        var filePath = $@"{_settings.DataPath}\{folder}\{filename}";
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize<T>(item));
        
        list ??= new List<T>();

        if (!list.Contains(item)) {
            list.Add(item);
        }
    }

    private void DeleteAsync<T>(List<T> list, string folder, string id) {
        var filePath = $@"{_settings.DataPath}\{folder}\{id}.json";
        try {
            File.Delete(filePath);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }
}