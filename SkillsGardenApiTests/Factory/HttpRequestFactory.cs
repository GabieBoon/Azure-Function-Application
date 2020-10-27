using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SkillsGardenApiTests.Factory
{
    public static class HttpRequestFactory
    {
        public static HttpRequest CreateGetRequest(Dictionary<string, StringValues> query = null)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;

            if (query != null)
            {
                request.Query = new QueryCollection(query);
            }

            return request;
        }

        public static HttpRequest CreatePostRequest<T>(T model)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;
            request.ContentType = "application/json";
            request.Method = HttpMethods.Post;

            string json = JsonConvert.SerializeObject(model);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return request;
        }

        public static HttpRequest CreatePostRequest(string json)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;
            request.ContentType = "application/json";
            request.Method = HttpMethods.Post;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return request;
        }

        public static HttpRequest CreatePutRequest<T>(T model)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;
            request.ContentType = "application/json";
            request.Method = HttpMethods.Put;

            string json = JsonConvert.SerializeObject(model);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return request;
        }

        public static HttpRequest CreateDeleteRequest()
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;
            request.Method = HttpMethods.Delete;

            return request;
        }

        public static HttpRequest CreatePostRequestWithoutBody()
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;
            request.Method = HttpMethods.Post;

            return request;
        }

        public static async Task<HttpRequest> CreateFormDataRequest(Dictionary<string, StringValues> formdata, HttpMethod method, string file = "image")
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;
            request.ContentType = "multipart/form-data";
            request.Method = method.ToString();

            FormFileCollection formFileCollection = null;


            if (file == "image")
            {
                // add image to formdata
                byte[] imageData = new byte[64];
                Array.Clear(imageData, 0, imageData.Length);
                var imageContent = new ByteArrayContent(imageData);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                Stream stream = await imageContent.ReadAsStreamAsync();
                FormFile image = new FormFile(stream, 0, stream.Length, "Image", "image.png")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                formFileCollection = new FormFileCollection();
                formFileCollection.Add(image);
            }

            if (file == "gif")
            {
                byte[] imageData = new byte[64];
                Array.Clear(imageData, 0, imageData.Length);
                var gifContent = new ByteArrayContent(imageData);
                gifContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/gif");
                Stream stream = await gifContent.ReadAsStreamAsync();
                FormFile image = new FormFile(stream, 0, stream.Length, "Image", "image.gif")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/gif"
                };
                
                formFileCollection = new FormFileCollection();
                formFileCollection.Add(image);
            }

            if (file == "toBigImage")
            {
                // add image to formdata
                byte[] imageData = new byte[11000000];
                Array.Clear(imageData, 0, imageData.Length);
                var imageContent = new ByteArrayContent(imageData);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                Stream stream = await imageContent.ReadAsStreamAsync();
                FormFile image = new FormFile(stream, 0, stream.Length, "Image", "image.png")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                formFileCollection = new FormFileCollection();
                formFileCollection.Add(image);
            }

            if (file == "empty")
            {
                //add nothing
            }

            FormCollection formcollection = new FormCollection(formdata, formFileCollection);
            request.Form = formcollection;

            return request;
        }
    }
}
