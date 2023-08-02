using System.Text.Json;
using Data.Models;
using Data.Models.Interfaces;
using Microsoft.Extensions.Options;

namespace Data;

public class BlogApiJsonDirectAccess : IBlogApi {
    private readonly BlogApiJsonDirectAccessSetting _settings;

    private List<BlogPost>? _blogPosts;
    private List<Category>? _categories;
    private List<Tag>? _tags;

    public BlogApiJsonDirectAccess(IOptions<BlogApiJsonDirectAccessSetting> option) {
        _settings = option.Value;

        if (!Directory.Exists(_settings.DataPath)) {
            Directory.CreateDirectory(_settings.DataPath);
        }

        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.BlogPostsFolder}")) {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.BlogPostsFolder}");
        }

        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.CategoriesFolder}")) {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.CategoriesFolder}");
        }

        if (!Directory.Exists($@"{_settings.DataPath}\{_settings.TagsFolder}")) {
            Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.TagsFolder}");
        }
    }

    public async Task<List<BlogPost>?> GetBlogPostsAsync(int numberOfPosts, int startIndex) {
        await LoadBlogPostAsync();
        return _blogPosts ?? new List<BlogPost>();
    }

    public async Task<BlogPost?> GetBlogPostAsync(string id) {
        await LoadBlogPostAsync();
        if (_blogPosts == null) {
            throw new Exception("Blog post not found");
        }

        return _blogPosts.FirstOrDefault(b => b.Id == id);
    }

    public async Task<int> GetBlogPostCountAsync() {
        await LoadBlogPostAsync();
        return _blogPosts?.Count ?? 0;
    }

    public async Task<List<Category>?> GetCategoriesAsync() {
        await LoadCategoriesAsync();
        return _categories ?? new List<Category>();
    }

    public async Task<Category?> GetCategoryAsync(string id) {
        await LoadCategoriesAsync();
        if (_categories == null) {
            throw new Exception("Categories not found");
        }

        return _categories.FirstOrDefault(c => c.Id == id);
    }

    public async Task<List<Tag>?> GetTagsAsync() {
        await LoadTagsAsync();
        return _tags ?? new List<Tag>();
    }

    public async Task<Tag?> GetTagAsync(string id) {
        await LoadTagsAsync();
        if (_tags == null) {
            throw new Exception("Tags not found");
        }

        return _tags.FirstOrDefault(t => t.Id == id);
    }

    public async Task<BlogPost?> SaveBlogPostAsync(BlogPost item) {
        item.Id ??= Guid.NewGuid().ToString();

        await SaveAsync(_blogPosts, _settings.BlogPostsFolder, $"{item.Id}.json", item);

        return item;
    }

    public async Task<Category?> SaveCategoryAsync(Category item) {
        item.Id ??= Guid.NewGuid().ToString();

        await SaveAsync(_categories, _settings.CategoriesFolder, $"{item.Id}.json", item);

        return item;
    }

    public async Task<Tag?> SaveTagAsync(Tag item) {
        item.Id ??= Guid.NewGuid().ToString();

        await SaveAsync(_tags, _settings.TagsFolder, $"{item.Id}.json", item);

        return item;
    }

    public Task DeleteBlogPostAsync(string id) {
        DeleteAsync(_blogPosts, _settings.BlogPostsFolder, id);
        var item = _blogPosts?.FirstOrDefault(b => b.Id == id);
        if (item != null) {
            _blogPosts?.Remove(item);
        }

        return Task.CompletedTask;
    }

    public Task DeleteCategoryAsync(string id) {
        DeleteAsync(_categories, _settings.CategoriesFolder, id);
        var item = _categories?.FirstOrDefault(b => b.Id == id);
        if (item != null) {
            _categories?.Remove(item);
        }

        return Task.CompletedTask;
    }

    public Task DeleteTagAsync(string id) {
        DeleteAsync(_tags, _settings.TagsFolder, id);
        var item = _tags?.FirstOrDefault(b => b.Id == id);
        if (item != null) {
            _tags?.Remove(item);
        }

        return Task.CompletedTask;
    }

    public Task InvalidateCacheAsync() {
        _blogPosts = null;
        _tags = null;
        _categories = null;
        return Task.CompletedTask;
    }

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