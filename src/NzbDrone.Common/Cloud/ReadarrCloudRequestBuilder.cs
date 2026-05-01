using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface IReadarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory Metadata { get; }
    }

    public class ReadarrCloudRequestBuilder : IReadarrCloudRequestBuilder
    {
        public ReadarrCloudRequestBuilder()
        {
            //TODO: Create Update Endpoint
            Services = new HttpRequestBuilder("https://readarr.servarr.com/v1/")
                .CreateFactory();

            // Metadata is now served directly by OpenLibraryProxy against openlibrary.org
            Metadata = new HttpRequestBuilder("https://openlibrary.org/{route}")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory Metadata { get; }
    }
}
