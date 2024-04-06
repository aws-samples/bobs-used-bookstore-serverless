using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace BookInventory.Service.UnitTests
{
    public class ImageServiceTests
    {
        private readonly IAmazonRekognition rekognitionClient;
        private readonly IAmazonS3 amazonS3Client;
        private readonly ImageService sut;
        private const string destinationBucket = "destination-bucket";

        public ImageServiceTests()
        {
            this.rekognitionClient = A.Fake<IAmazonRekognition>();
            this.amazonS3Client = A.Fake<IAmazonS3>();
            Environment.SetEnvironmentVariable("PUBLISH_IMAGE_BUCKET", destinationBucket);
            this.sut = new ImageService(rekognitionClient, amazonS3Client);
        }

        [Fact]
        public async Task IsSafeAsync_WhenImageIsSafe_ReturnsTrue()
        {
            // Arrange
            var bucket = "test-bucket";
            var image = "test-image.jpg";
            var detectionResult = new DetectModerationLabelsResponse
            {
                ModerationLabels = [new ModerationLabel { Name = "Safe" }]
            };

            A.CallTo(() => this.rekognitionClient.DetectModerationLabelsAsync(A<DetectModerationLabelsRequest>.Ignored, default))
                .Returns(detectionResult);

            // Act
            var result = await sut.IsSafeAsync(bucket, image);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSafeAsync_WhenImageIsUnsafe_ReturnsFalse()
        {
            // Arrange
            var bucket = "test-bucket";
            var image = "test-image.jpg";
            var detectionResult = new DetectModerationLabelsResponse
            {
                ModerationLabels = [new ModerationLabel { Name = "Explicit Nudity" }]
            };

            A.CallTo(() => this.rekognitionClient.DetectModerationLabelsAsync(A<DetectModerationLabelsRequest>.Ignored, default))
                .Returns(detectionResult);

            // Act
            var result = await sut.IsSafeAsync(bucket, image);

            // Assert
            result.Should().BeFalse();
        }

        //This test will comver SaveImageAsync and ResizeImageAsync functionality
        [Fact]
        public async Task SaveImageAsync_ResizesImageAndUploadsToDestinationBucket()
        {
            // Arrange
            var sourceBucket = "source-bucket";
            var sourceImage = "1001jokes.png";
            var coverPageImage = File.ReadAllBytes("TestData/1001jokes.png");
            var coverPageStream = new MemoryStream(coverPageImage);
            A.CallTo(() => amazonS3Client.GetObjectStreamAsync(sourceBucket, sourceImage, null, default))
                .Returns(Task.FromResult<Stream>(coverPageStream));
            // Act
            await sut.SaveImageAsync(sourceBucket, sourceImage);

            // Assert
            A.CallTo(() => amazonS3Client.PutObjectAsync(A<PutObjectRequest>.That.Matches(r =>
                r.BucketName == destinationBucket &&
                r.Key == sourceImage &&
                r.InputStream == coverPageStream), default))
                .MustHaveHappenedOnceExactly();
        }
    }
}