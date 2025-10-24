using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Helper class for deserializing the translation response
static string GetThisFilePath([CallerFilePath] string path = null)
{
    return path;
}

/// <summary>
/// Translates text using a LibreTranslate API.
/// </summary>
/// <param name="text">The text to translate.</param>
/// <param name="sourceLang">The source language code (e.g., "en").</param>
/// <param name="targetLang">The target language code (e.g., "de").</param>
/// <param name="apiUrl">The URL of the LibreTranslate /translate endpoint.</param>
/// <returns>The translated text.</returns>
static async Task<string> Translate(string text, string sourceLang, string targetLang, string apiUrl)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        return string.Empty;
    }

    using var client = new HttpClient();
    var requestData = new
    {
        q = text,
        source = sourceLang,
        target = targetLang,
        format = "text",
        api_key = ""
    };
    var jsonContent = JsonSerializer.Serialize(requestData);
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    try
    {
        var response = await client.PostAsync(apiUrl, content);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var translation = JsonSerializer.Deserialize<TranslationResponse>(jsonResponse);
        return translation?.TranslatedText ?? string.Empty;
    }
    catch (HttpRequestException e)
    {
        // Handle potential network errors or non-success status codes
        Console.WriteLine($"Error translating '{text}' to '{targetLang}': {e.Message}");
        return text; // Return original text on failure
    }
}

// --- Main execution ---

// CONFIGURATION: Adjust these values
string libreTranslateUrl = "http://localhost:5000/translate"; // URL to your LibreTranslate instance
string defaultCultureCode = "en"; // Language code for your "Default Culture" column

var csvPath = Path.Combine(Path.GetDirectoryName(GetThisFilePath()), "CSVs", "Loca.csv");
string[] lines = File.ReadAllLines(csvPath);

if (lines.Length < 2)
{
    Console.WriteLine("CSV file is empty or has only a header.");
    return;
}

string[] headers = lines[0].Split(';');
var defaultCultureIndex = Array.IndexOf(headers, "\"Default Culture\"");

if (defaultCultureIndex == -1)
{
    Console.WriteLine("Could not find the '\"Default Culture\"' column.");
    return;
}

// Get target languages and their column indices from the header
var targetLanguages = new List<(string langCode, int index)>();
for (int i = defaultCultureIndex + 1; i < headers.Length; i++)
{
    var langCode = headers[i].Trim('"');
    if (!string.IsNullOrWhiteSpace(langCode))
    {
        targetLanguages.Add((langCode, i));
    }
}

// Process each line (skip header)
for (int i = 1; i < lines.Length; i++)
{
    string[] fields = lines[i].Split(';');
    string sourceText = fields[defaultCultureIndex].Trim('"');

    foreach (var (langCode, langIndex) in targetLanguages)
    {
        // Only translate if the target cell is empty
        if (langIndex < fields.Length && string.IsNullOrWhiteSpace(fields[langIndex].Trim('"')))
        {
            Console.WriteLine($"Translating '{sourceText}' to '{langCode}'...");
            string translatedText = await Translate(sourceText, defaultCultureCode, langCode, libreTranslateUrl);
            fields[langIndex] = $"\"{translatedText}\"";
        }
    }
    lines[i] = string.Join(";", fields);
}

// Overwrite the original file with the translated content
File.WriteAllLines(csvPath, lines);

Console.WriteLine("Translation complete.");

public class TranslationResponse
{
    [JsonPropertyName("translatedText")]
    public string TranslatedText { get; set; }
}