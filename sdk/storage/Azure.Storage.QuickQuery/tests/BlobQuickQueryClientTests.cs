﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Testing;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.QuickQuery.Models;
using Azure.Storage.Test;
using NUnit.Framework;

namespace Azure.Storage.QuickQuery.Tests
{
    public class BlobQuickQueryClientTests : QuickQueryTestBase
    {
        public BlobQuickQueryClientTests(bool async) : this(async, null) { }

        public BlobQuickQueryClientTests(bool async, RecordedTestMode? mode = null)
            : base(async, mode) { }

        public DateTimeOffset OldDate => Recording.Now.AddDays(-1);
        public DateTimeOffset NewDate => Recording.Now.AddDays(1);

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_Min()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
            Stream stream = CreateDataStream(Constants.KB);
            await blockBlobClient.UploadAsync(stream);

            // Act
            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT _2 from BlobStorage WHERE _1 > 250;";
            Response<BlobDownloadInfo> response =  await queryClient.QueryAsync(query);

            using StreamReader streamReader = new StreamReader(response.Value.Content);
            string s = await streamReader.ReadToEndAsync();

            // Assert
            Assert.AreEqual("400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n", s);
        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_Snapshot()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
            Stream stream = CreateDataStream(Constants.KB);
            await blockBlobClient.UploadAsync(stream);
            Response<BlobSnapshotInfo> snapshotResponse = await blockBlobClient.CreateSnapshotAsync();
            BlockBlobClient snapshotClient = InstrumentClient(blockBlobClient.WithSnapshot(snapshotResponse.Value.Snapshot));

            // Act
            BlobQuickQueryClient queryClient = snapshotClient.GetQuickQueryClient();
            string query = @"SELECT _2 from BlobStorage WHERE _1 > 250;";
            Response<BlobDownloadInfo> response = await queryClient.QueryAsync(query);

            using StreamReader streamReader = new StreamReader(response.Value.Content);
            string s = await streamReader.ReadToEndAsync();

            // Assert
            Assert.AreEqual("400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n400\n", s);
        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_Error()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));

            // Act
            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT _2 from BlobStorage WHERE _1 > 250;";

            // Act
            // Act
            await TestHelper.AssertExpectedExceptionAsync<RequestFailedException>(
                queryClient.QueryAsync(
                    query),
                e => Assert.AreEqual("BlobNotFound", e.ErrorCode));
        }

        [Test]
        //[Ignore("Don't want to record 16 MB of data.")]
        public async Task QueryAsync_MultipleDataRecords()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
            Stream stream = CreateDataStream(16 * Constants.MB);
            await blockBlobClient.UploadAsync(stream);

            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT * from BlobStorage";

            // Act
            TestProgress progressReporter = new TestProgress();
            Response<BlobDownloadInfo> response = await queryClient.QueryAsync(
                query,
                progressReceiver: progressReporter);

            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader expectedStreamReader = new StreamReader(stream);
            string expected = await expectedStreamReader.ReadToEndAsync();

            using StreamReader actualStreamReader = new StreamReader(response.Value.Content);
            string actual = await actualStreamReader.ReadToEndAsync();

            // Assert
            // Check we got back the same content that we uploaded.
            Assert.AreEqual(expected, actual);

            // Check progress reporter
            Assert.AreEqual(5, progressReporter.List.Count);
            Assert.AreEqual(4 * Constants.MB, progressReporter.List[0]);
            Assert.AreEqual(8 * Constants.MB, progressReporter.List[1]);
            Assert.AreEqual(12 * Constants.MB, progressReporter.List[2]);
            Assert.AreEqual(16 * Constants.MB, progressReporter.List[3]);
            Assert.AreEqual(16 * Constants.MB, progressReporter.List[4]);
        }

        [Test]
        //[Ignore("Don't want to record 120 MB of data.")]
        public async Task QueryAsync_Large()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
            Stream stream = CreateDataStream(120 * Constants.MB);
            await blockBlobClient.UploadAsync(stream);

            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT * from BlobStorage";

            // Act
            TestProgress progressReporter = new TestProgress();
            Response<BlobDownloadInfo> response = await queryClient.QueryAsync(
                query,
                progressReceiver: progressReporter);

            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader expectedStreamReader = new StreamReader(stream);
            string expected = await expectedStreamReader.ReadToEndAsync();

            using StreamReader actualStreamReader = new StreamReader(response.Value.Content);
            string actual = await actualStreamReader.ReadToEndAsync();

            // Assert
            // Check we got back the same content that we uploaded.
            Assert.AreEqual(expected, actual);
        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_Progress()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
            Stream stream = CreateDataStream(Constants.KB);
            await blockBlobClient.UploadAsync(stream);

            // Act
            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT _2 from BlobStorage WHERE _1 > 250;";
            TestProgress progressReporter = new TestProgress();

            Response<BlobDownloadInfo> response = await queryClient.QueryAsync(
                query,
                progressReceiver: progressReporter);

            using StreamReader streamReader = new StreamReader(response.Value.Content);
            await streamReader.ReadToEndAsync();

            Assert.AreEqual(2, progressReporter.List.Count);
            Assert.AreEqual(Constants.KB, progressReporter.List[0]);
            Assert.AreEqual(Constants.KB, progressReporter.List[1]);
        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_QueryTextConfigurations()
        {
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
            Stream stream = CreateDataStream(Constants.KB);
            await blockBlobClient.UploadAsync(stream);

            // Act
            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT _2 from BlobStorage WHERE _1 > 250;";

            CsvTextConfiguration cvsTextConfiguration = new CsvTextConfiguration
            {
                ColumnSeparator = ',',
                FieldQuote = '"',
                EscapeCharacter = '\\',
                RecordSeparator = '\n',
                HasHeaders = false
            };

            JsonTextConfiguration jsonTextConfiguration = new JsonTextConfiguration
            {
                RecordSeparator = '\n'
            };

            // Act
            Response<BlobDownloadInfo> response = await queryClient.QueryAsync(
                query,
                inputTextConfiguration: cvsTextConfiguration,
                outputTextConfiguration: jsonTextConfiguration);

            using StreamReader streamReader = new StreamReader(response.Value.Content);
            string s = await streamReader.ReadToEndAsync();

            // Assert
            Assert.AreEqual("{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n{\"_1\":\"400\"}\n", s);
        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_NonFatalError()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));

            byte[] data = Encoding.UTF8.GetBytes("100,pizza,300,400\n300,400,500,600\n");
            using MemoryStream stream = new MemoryStream(data);
            await blockBlobClient.UploadAsync(stream);

            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT _1 from BlobStorage WHERE _2 > 250;";

            // Act - with no IBlobQueryErrorReceiver
            Response<BlobDownloadInfo> response = await queryClient.QueryAsync(query);
            using StreamReader streamReader = new StreamReader(response.Value.Content);
            string s = await streamReader.ReadToEndAsync();


            // Act - with  IBlobQueryErrorReceiver
            BlobQueryError expectedBlobQueryError = new BlobQueryError
            {
                IsFatal = false,
                Name = "InvalidTypeConversion",
                Description = "Invalid type conversion.",
                Position = 0
            };

            response = await queryClient.QueryAsync(
                query,
                errorReceiver: new ErrorReceiver(expectedBlobQueryError));
            using StreamReader streamReader2 = new StreamReader(response.Value.Content);
            s = await streamReader2.ReadToEndAsync();
        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_FatalError()
        {
            // Arrange
            await using DisposingContainer test = await GetTestContainerAsync();
            BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
            Stream stream = CreateDataStream(Constants.KB);
            await blockBlobClient.UploadAsync(stream);

            BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
            string query = @"SELECT * from BlobStorage;";
            JsonTextConfiguration jsonTextConfiguration = new JsonTextConfiguration
            {
                RecordSeparator = '\n'
            };

            // Act - with no IBlobQueryErrorReceiver
            Response<BlobDownloadInfo> response = await queryClient.QueryAsync(
                query,
                inputTextConfiguration: jsonTextConfiguration);
            using StreamReader streamReader = new StreamReader(response.Value.Content);
            string s = await streamReader.ReadToEndAsync();

            // Act - with  IBlobQueryErrorReceiver
            BlobQueryError expectedBlobQueryError = new BlobQueryError
            {
                IsFatal = true,
                Name = "ParseError",
                Description = "Unexpected token ',' at [byte: 3]. Expecting tokens '{', or '['.",
                Position = 0
            };

            response = await queryClient.QueryAsync(
                query,
                inputTextConfiguration: jsonTextConfiguration,
                errorReceiver: new ErrorReceiver(expectedBlobQueryError));
            using StreamReader streamReader2 = new StreamReader(response.Value.Content);
            s = await streamReader2.ReadToEndAsync();

        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_AccessConditions()
        {
            var garbageLeaseId = GetGarbageLeaseId();
            foreach (AccessConditionParameters parameters in AccessConditions_Data)
            {
                // Arrange
                await using DisposingContainer test = await GetTestContainerAsync();
                BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
                Stream stream = CreateDataStream(Constants.KB);
                await blockBlobClient.UploadAsync(stream);

                parameters.Match = await SetupBlobMatchCondition(blockBlobClient, parameters.Match);
                parameters.LeaseId = await SetupBlobLeaseCondition(blockBlobClient, parameters.LeaseId, garbageLeaseId);
                BlobRequestConditions accessConditions = BuildAccessConditions(
                    parameters: parameters,
                    lease: true);

                BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
                string query = @"SELECT * from BlobStorage";

                // Act
                Response<BlobDownloadInfo> response = await queryClient.QueryAsync(
                    query,
                    conditions: accessConditions);

                // Assert
                Assert.IsNotNull(response.Value.Details.ETag);
            }
        }

        [Test]
        //[Ignore("Recording framework doesn't play nicely with Avro")]
        public async Task QueryAsync_AccessConditionsFail()
        {
            var garbageLeaseId = GetGarbageLeaseId();
            foreach (AccessConditionParameters parameters in GetAccessConditionsFail_Data(garbageLeaseId))
            {
                // Arrange
                await using DisposingContainer test = await GetTestContainerAsync();
                BlockBlobClient blockBlobClient = InstrumentClient(test.Container.GetBlockBlobClient(GetNewBlobName()));
                Stream stream = CreateDataStream(Constants.KB);
                await blockBlobClient.UploadAsync(stream);

                parameters.NoneMatch = await SetupBlobMatchCondition(blockBlobClient, parameters.NoneMatch);
                BlobRequestConditions accessConditions = BuildAccessConditions(parameters);


                BlobQuickQueryClient queryClient = blockBlobClient.GetQuickQueryClient();
                string query = @"SELECT * from BlobStorage";

                // Act
                await TestHelper.AssertExpectedExceptionAsync<RequestFailedException>(
                    queryClient.QueryAsync(
                        query,
                        conditions: accessConditions),
                    e => { });
            }
        }

        private Stream CreateDataStream(long size)
        {
            MemoryStream stream = new MemoryStream();
            byte[] rowData = Encoding.UTF8.GetBytes("100,200,300,400\n300,400,500,600\n");
            long blockLength = 0;
            while (blockLength < size)
            {
                stream.Write(rowData, 0, rowData.Length);
                blockLength += rowData.Length;
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public IEnumerable<AccessConditionParameters> AccessConditions_Data
            => new[]
            {
                new AccessConditionParameters(),
                new AccessConditionParameters { IfModifiedSince = OldDate },
                new AccessConditionParameters { IfUnmodifiedSince = NewDate },
                new AccessConditionParameters { Match = ReceivedETag },
                new AccessConditionParameters { NoneMatch = GarbageETag },
                new AccessConditionParameters { LeaseId = ReceivedLeaseId }
            };

        public IEnumerable<AccessConditionParameters> GetAccessConditionsFail_Data(string garbageLeaseId)
            => new[]
            {
                new AccessConditionParameters { IfModifiedSince = NewDate },
                new AccessConditionParameters { IfUnmodifiedSince = OldDate },
                new AccessConditionParameters { Match = GarbageETag },
                new AccessConditionParameters { NoneMatch = ReceivedETag },
                new AccessConditionParameters { LeaseId = garbageLeaseId },
             };

        private RequestConditions BuildRequestConditions(
            AccessConditionParameters parameters)
            => new RequestConditions
            {
                IfModifiedSince = parameters.IfModifiedSince,
                IfUnmodifiedSince = parameters.IfUnmodifiedSince,
                IfMatch = parameters.Match != null ? new ETag(parameters.Match) : default(ETag?),
                IfNoneMatch = parameters.NoneMatch != null ? new ETag(parameters.NoneMatch) : default(ETag?)
            };

        private BlobRequestConditions BuildAccessConditions(
            AccessConditionParameters parameters,
            bool lease = true)
        {
            var accessConditions = BuildRequestConditions(parameters).ToBlobRequestConditions();
            if (lease)
            {
                accessConditions.LeaseId = parameters.LeaseId;
            }
            return accessConditions;
        }

        public class AccessConditionParameters
        {
            public DateTimeOffset? IfModifiedSince { get; set; }
            public DateTimeOffset? IfUnmodifiedSince { get; set; }
            public string Match { get; set; }
            public string NoneMatch { get; set; }
            public string LeaseId { get; set; }
        }

        private class ErrorReceiver
            : IBlobQueryErrorReceiver
        {
            private readonly BlobQueryError _expectedBlobQueryError;

            public ErrorReceiver(BlobQueryError expected)
            {
                _expectedBlobQueryError = expected;
            }

            public void ReportError(BlobQueryError blobQueryError)
            {
                Assert.AreEqual(_expectedBlobQueryError.IsFatal, blobQueryError.IsFatal);
                Assert.AreEqual(_expectedBlobQueryError.Name, blobQueryError.Name);
                Assert.AreEqual(_expectedBlobQueryError.Description, blobQueryError.Description);
                Assert.AreEqual(_expectedBlobQueryError.Position, blobQueryError.Position);
            }
        }
    }
}
