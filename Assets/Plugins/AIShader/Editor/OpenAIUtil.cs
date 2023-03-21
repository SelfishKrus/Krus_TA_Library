using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace AIShader {

static class OpenAIUtil
{
    static string CreateChatRequestBody(string prompt)
    {
        var msg = new OpenAI.RequestMessage();
        msg.role = "user";
        msg.content = prompt;

        var req = new OpenAI.Request();
        req.model = "gpt-3.5-turbo";
        req.messages = new [] { msg };

        return JsonUtility.ToJson(req);
    }

    public static string InvokeChat(string prompt)
    {
        // Create request body
        var requestBody = CreateChatRequestBody(prompt);

        // Create multipart form data section with request body
        var formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("application/json", System.Text.Encoding.UTF8.GetBytes(requestBody), "request"));

        // Create web request
        using var post = UnityWebRequest.Post(OpenAI.Api.Url, formData);

        // API key authorization
        post.SetRequestHeader("Authorization", "Bearer " + AIShaderSettings.instance.apiKey);

        // Request start
        var req = post.SendWebRequest();

        // Progress bar (Totally fake! Don't try this at home.)
        for (var progress = 0.0f; !req.isDone; progress += 0.01f)
        {
            EditorUtility.DisplayProgressBar("AI Shader Importer", "Generating...", progress);
            System.Threading.Thread.Sleep(100);
            progress += 0.01f;
        }
        EditorUtility.ClearProgressBar();

        // Response extraction
        var json = post.downloadHandler.text;
        var data = JsonUtility.FromJson<OpenAI.Response>(json);
        return data.choices[0].message.content;
    }
}

} // namespace AIShader
