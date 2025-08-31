using MedicareApi.Utils;
using Xunit;

namespace MedicareApi.Tests.Utils
{
    public class FileUploadSecurityHelperTests
    {
        [Theory]
        [InlineData("test.jpg", "test.jpg")]
        [InlineData("test file.jpg", "test_file.jpg")]
        [InlineData("test/../file.jpg", "test__file.jpg")]
        [InlineData("test./file.jpg", "test_file.jpg")]
        [InlineData("test.\\file.jpg", "test_file.jpg")]
        [InlineData("test<>file.jpg", "test__file.jpg")]
        [InlineData("test|file.jpg", "test_file.jpg")]
        [InlineData("test:file.jpg", "test_file.jpg")]
        [InlineData("test\"file.jpg", "test_file.jpg")]
        [InlineData("test*file.jpg", "test_file.jpg")]
        [InlineData("test?file.jpg", "test_file.jpg")]
        [InlineData("", "file")]
        [InlineData("   ", "file")]
        [InlineData("...", "file")]
        public void SanitizeFilename_SanitizesCorrectly(string input, string expected)
        {
            // Act
            var result = FileUploadSecurityHelper.SanitizeFilename(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(".exe", true)]
        [InlineData(".bat", true)]
        [InlineData(".cmd", true)]
        [InlineData(".js", true)]
        [InlineData(".vbs", true)]
        [InlineData(".asp", true)]
        [InlineData(".aspx", true)]
        [InlineData(".php", true)]
        [InlineData(".sh", true)]
        [InlineData(".ps1", true)]
        [InlineData(".jpg", false)]
        [InlineData(".png", false)]
        [InlineData(".gif", false)]
        [InlineData(".jpeg", false)]
        [InlineData(".txt", false)]
        [InlineData(".pdf", false)]
        [InlineData("exe", true)] // Without dot
        [InlineData("EXE", true)] // Case insensitive
        [InlineData(".EXE", true)] // Case insensitive with dot
        public void IsDangerousExtension_ReturnsExpectedResult(string extension, bool expectedDangerous)
        {
            // Act
            var result = FileUploadSecurityHelper.IsDangerousExtension(extension);

            // Assert
            Assert.Equal(expectedDangerous, result);
        }

        [Fact]
        public void ValidateFilename_ValidImageFile_ReturnsTrue()
        {
            // Arrange
            var allowedExtensions = new[] { ".jpg", ".png", ".gif" };

            // Act
            var result = FileUploadSecurityHelper.ValidateFilename("photo.jpg", allowedExtensions);

            // Assert
            Assert.True(result.isValid);
            Assert.Null(result.errorMessage);
        }

        [Fact]
        public void ValidateFilename_DangerousExtension_ReturnsFalse()
        {
            // Arrange
            var allowedExtensions = new[] { ".jpg", ".png", ".gif" };

            // Act
            var result = FileUploadSecurityHelper.ValidateFilename("malware.exe", allowedExtensions);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("not allowed for security reasons", result.errorMessage);
        }

        [Fact]
        public void ValidateFilename_DisallowedButSafeExtension_ReturnsFalse()
        {
            // Arrange
            var allowedExtensions = new[] { ".jpg", ".png" };

            // Act
            var result = FileUploadSecurityHelper.ValidateFilename("document.pdf", allowedExtensions);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("not allowed. Allowed extensions", result.errorMessage);
        }

        [Fact]
        public void ValidateFilename_NoExtension_ReturnsFalse()
        {
            // Arrange
            var allowedExtensions = new[] { ".jpg", ".png" };

            // Act
            var result = FileUploadSecurityHelper.ValidateFilename("filename", allowedExtensions);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("must have an extension", result.errorMessage);
        }

        [Fact]
        public void ValidateFilename_EmptyFilename_ReturnsFalse()
        {
            // Arrange
            var allowedExtensions = new[] { ".jpg", ".png" };

            // Act
            var result = FileUploadSecurityHelper.ValidateFilename("", allowedExtensions);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains("cannot be empty", result.errorMessage);
        }

        [Theory]
        [InlineData("script.js", ".js")]
        [InlineData("virus.exe", ".exe")]
        [InlineData("batch.bat", ".bat")]
        [InlineData("command.cmd", ".cmd")]
        [InlineData("visual.vbs", ".vbs")]
        [InlineData("page.asp", ".asp")]
        [InlineData("webapp.aspx", ".aspx")]
        [InlineData("site.php", ".php")]
        [InlineData("shell.sh", ".sh")]
        [InlineData("powershell.ps1", ".ps1")]
        [InlineData("registry.reg", ".reg")]
        [InlineData("java.jar", ".jar")]
        [InlineData("python.py", ".py")]
        [InlineData("ruby.rb", ".rb")]
        [InlineData("perl.pl", ".pl")]
        public void ValidateFilename_CommonDangerousExtensions_ReturnsFalse(string filename, string extension)
        {
            // Arrange
            var allowedExtensions = new[] { ".jpg", ".png", ".gif" };

            // Act
            var result = FileUploadSecurityHelper.ValidateFilename(filename, allowedExtensions);

            // Assert
            Assert.False(result.isValid);
            Assert.Contains($"'{extension}' is not allowed for security reasons", result.errorMessage);
        }

        [Fact]
        public void GenerateSecureFilename_BasicCase_GeneratesSecureName()
        {
            // Arrange
            var originalFilename = "profile-photo.jpg";
            var prefix = "user123";

            // Act
            var result = FileUploadSecurityHelper.GenerateSecureFilename(originalFilename, prefix, true);

            // Assert
            Assert.EndsWith(".jpg", result);
            Assert.StartsWith("user123_", result);
            Assert.Contains("profile-photo", result);
            // Should contain GUID (8 characters after prefix and before filename)
            var parts = result.Split('_');
            Assert.True(parts.Length >= 3);
        }

        [Fact]
        public void GenerateSecureFilename_UnsafeCharacters_SanitizesCorrectly()
        {
            // Arrange
            var originalFilename = "unsafe../file<>.jpg";
            var prefix = "user../123";

            // Act
            var result = FileUploadSecurityHelper.GenerateSecureFilename(originalFilename, prefix, true);

            // Assert
            Assert.EndsWith(".jpg", result);
            Assert.DoesNotContain("..", result);
            Assert.DoesNotContain("<", result);
            Assert.DoesNotContain(">", result);
            Assert.DoesNotContain("/", result);
        }

        [Fact]
        public void GenerateSecureFilename_WithoutGuid_GeneratesNameWithoutGuid()
        {
            // Arrange
            var originalFilename = "test.jpg";
            var prefix = "user";

            // Act
            var result = FileUploadSecurityHelper.GenerateSecureFilename(originalFilename, prefix, false);

            // Assert
            Assert.Equal("user_test.jpg", result);
        }

        [Fact]
        public void GenerateSecureFilename_WithoutPrefix_GeneratesNameWithoutPrefix()
        {
            // Arrange
            var originalFilename = "test.jpg";

            // Act
            var result = FileUploadSecurityHelper.GenerateSecureFilename(originalFilename, null, false);

            // Assert
            Assert.Equal("test.jpg", result);
        }

        [Fact]
        public void GenerateSecureFilename_LongFilename_TruncatesCorrectly()
        {
            // Arrange
            var longName = new string('a', 200);
            var originalFilename = $"{longName}.jpg";

            // Act
            var result = FileUploadSecurityHelper.GenerateSecureFilename(originalFilename, "user", true);

            // Assert
            Assert.EndsWith(".jpg", result);
            Assert.True(result.Length < originalFilename.Length);
            Assert.StartsWith("user_", result);
        }
    }
}