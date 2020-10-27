using Microsoft.AspNetCore.Http;
using SkillsGardenApi.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkillsGardenApiTests.Mock
{
    public class AzureServiceMock : IAzureService
    {
        public bool deleteImageFromBlobStorage(string imageName)
        {
            return true;
        }

        public bool doesBlobExist(string imageName)
        {
            return true;
        }

        public async Task<string> saveImageToBlobStorage(FormFile file)
        {
            return "test.png";
        }

        public string getBlobSas(string imageName)
        {
            return "test.png";
        }
    }
}
