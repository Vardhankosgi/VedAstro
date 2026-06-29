using Azure.Data.Tables;
using VedAstro.Library;

namespace API
{
    /// <summary>
    /// Specialized class to log data to table in AZ store
    /// </summary>
    public static class LogBook
    {
        private static readonly TableServiceClient tableServiceClient;
        private static string tableName = "LogBook";
        private static readonly TableClient tableClient;

        static LogBook()
        {
            //todo cleanup
            string storageAccountKey = Secrets.VedAstroApiStorageKey;

            //get connection & load tables
            tableServiceClient = new TableServiceClient(storageAccountKey);
            tableClient = tableServiceClient.GetTableClient(tableName);

        }

        /// <summary>
        /// Marks the call as running
        /// </summary>
        public static void Add(LogBookEntity newLogRecord)
        {
            

            //creates record if no exist, update if already there
            tableClient.UpsertEntity(newLogRecord);

        }


    }

}
