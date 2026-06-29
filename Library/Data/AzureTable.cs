using System;
using Azure.Data.Tables;

namespace VedAstro.Library
{

    /// <summary>
    /// static storage for connection access to Azure tables
    /// </summary>
    public static class AzureTable
    {
        //STORAGE SETTINGS
        public const string AccountName = "vedastrocentralstorage";
        private static readonly string StorageAccountKey = Secrets.VedAstroCentralStorageKey;

        //TABLE NAMES
        public const string APIAbuseListName = "APIAbuseList";
        public const string PersonListName = "PersonList";
        public const string PersonShareListName = "PersonShareList";
        public const string PersonListRecycleBinName = "PersonListRecycleBin";
        public const string LifeEventListName = "LifeEventList";
        
        //URL
        public const string APIAbuseListUri = $"https://vedastrocentralstorage.table.core.windows.net/{APIAbuseListName}";
        public const string PersonListUri = $"https://vedastrocentralstorage.table.core.windows.net/{PersonListName}";
        public const string PersonShareListUri = $"https://vedastrocentralstorage.table.core.windows.net/{PersonShareListName}";
        public const string PersonListRecycleBinUri = $"https://vedastrocentralstorage.table.core.windows.net/{PersonListRecycleBinName}";
        public const string LifeEventListUri = $"https://vedastrocentralstorage.table.core.windows.net/{LifeEventListName}";
        
        //TABLE
        public static readonly TableClient? PersonList = (new TableServiceClient(StorageAccountKey)).GetTableClient(PersonListName);
        public static readonly TableClient? APIAbuseList = (new TableServiceClient(StorageAccountKey)).GetTableClient(APIAbuseListName);
        public static readonly TableClient? PersonListRecycleBin = (new TableServiceClient(StorageAccountKey)).GetTableClient(PersonListRecycleBinName);
        public static readonly TableClient? LifeEventList = (new TableServiceClient(StorageAccountKey)).GetTableClient(LifeEventListName);
        public static readonly TableClient? PersonShareList = (new TableServiceClient(StorageAccountKey)).GetTableClient(PersonShareListName);

        static AzureTable()
        {
            try
            {
                PersonList?.CreateIfNotExists();
                APIAbuseList?.CreateIfNotExists();
                PersonListRecycleBin?.CreateIfNotExists();
                LifeEventList?.CreateIfNotExists();
                PersonShareList?.CreateIfNotExists();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to auto-create Azure tables: {e.Message}");
            }
        }
    }
}
