using InstaQuiz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace InstaQuiz.Services
{
    public class MLService
    {
        private readonly HttpClient _httpClient;

        public MLService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<List<Question>> GenerateQuiz(string prompt)
        {
            try
            {
                // Prepare the request body
                var requestBody = new { text = prompt };

                // Call the Python ML API
                var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:5000/generate_quiz", requestBody);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Deserialize the JSON response into a list of dynamic objects
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiQuestions = JsonSerializer.Deserialize<List<ApiQuestion>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Convert the API response to your Question model
                var questions = apiQuestions.Select((apiQuestion, index) => ConvertApiQuestionToQuestion(apiQuestion, index + 1)).ToList();

                return questions;
            }
            catch (Exception ex)
            {
                // Handle errors appropriately (log or rethrow)
                throw new Exception("Error generating quiz", ex);
            }
        }

        private Question ConvertApiQuestionToQuestion(ApiQuestion apiQuestion, int questionId)
        {
            // Parse the options JSON string into a list of strings if needed (if options are not yet a list)
            var options = apiQuestion.Options;

            // Find the correct option index
            int correctOption = options.IndexOf(apiQuestion.Correct_Answer) + 1; // +1 to match 1-based index

            return new Question
            {
                QuestionId = questionId,
                Text = apiQuestion.Question,
                Option1 = options.ElementAtOrDefault(0),
                Option2 = options.ElementAtOrDefault(1),
                Option3 = options.ElementAtOrDefault(2),
                Option4 = options.ElementAtOrDefault(3),
                CorrectOption = correctOption
            };
        }
    }

    // Class to represent the API response structuressss
    public class ApiQuestion
    {
        public string Question { get; set; }
        public string Correct_Answer { get; set; }
        public List<string> Options { get; set; }  // Adjusted to be a List<string>
    }
}
