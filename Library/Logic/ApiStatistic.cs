using Azure.Data.Tables;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Azure.Functions.Worker.Http;
using System.ComponentModel;

namespace VedAstro.Library
{
    public static class ApiStatistic
    {
        /// <summary>
        /// sample holder type when doing interop
        /// </summary>
        public record GeoLocationRawAPI(dynamic MainRow, dynamic MetadataRow);

        private static readonly TableServiceClient ipAddressServiceClient;
        private static readonly TableServiceClient requestUrlStatisticServiceClient;
        private static readonly TableServiceClient subscriberStatisticServiceClient;
        private static readonly TableServiceClient userAgentStatisticServiceClient;
        private static readonly TableServiceClient rawRequestStatisticServiceClient;

        private static readonly TableClient ipAddressStatisticTableClient;
        private static readonly TableClient requestUrlStatisticTableClient;
        private static readonly TableClient subscriberStatisticTableClient;
        private static readonly TableClient userAgentStatisticTableClient;
        private static readonly TableClient rawRequestStatisticTableClient;

        public const string RawRequestStatisticName = "RawRequestStatistic";
        public const string RequestUrlStatisticName = "RequestUrlStatistic";
        public const string SubscriberStatisticName = "SubscriberStatistic";
        public const string UserAgentStatisticName = "UserAgentStatistic";
        public const string IpAddressStatisticName = "IpAddressStatistic";


        /// <summary>
        /// init Table access
        /// </summary>
        static ApiStatistic()
        {
            string accountName = "centralapistorage"; //indic heritage 
                                                      //string accountName = "vedastroapistorage"; //vedastro 

            //# RAW REQUEST : (use only when needed, costly🤑)
            //------------------------------------
            //get connection & load tables
            rawRequestStatisticServiceClient = new TableServiceClient(Secrets.AzureGeoLocationStorageKey);
            rawRequestStatisticTableClient = rawRequestStatisticServiceClient.GetTableClient(RawRequestStatisticName);
            rawRequestStatisticTableClient.CreateIfNotExists();


            //# REQUEST URL
            //------------------------------------
            //get connection & load tables
            requestUrlStatisticServiceClient = new TableServiceClient(Secrets.AzureGeoLocationStorageKey);
            requestUrlStatisticTableClient = requestUrlStatisticServiceClient.GetTableClient(RequestUrlStatisticName);
            requestUrlStatisticTableClient.CreateIfNotExists();

            //# SUBSCRIBER
            //------------------------------------
            //get connection & load tables
            subscriberStatisticServiceClient = new TableServiceClient(Secrets.AzureGeoLocationStorageKey);
            subscriberStatisticTableClient = subscriberStatisticServiceClient.GetTableClient(SubscriberStatisticName);
            subscriberStatisticTableClient.CreateIfNotExists();

            //# USER AGENT
            //------------------------------------
            //get connection & load tables
            userAgentStatisticServiceClient = new TableServiceClient(Secrets.AzureGeoLocationStorageKey);
            userAgentStatisticTableClient = userAgentStatisticServiceClient.GetTableClient(UserAgentStatisticName);
            userAgentStatisticTableClient.CreateIfNotExists();


            //# IP ADDRESS
            //------------------------------------
            //get connection & load tables
            ipAddressServiceClient = new TableServiceClient(Secrets.AzureGeoLocationStorageKey);
            ipAddressStatisticTableClient = ipAddressServiceClient.GetTableClient(IpAddressStatisticName);
            ipAddressStatisticTableClient.CreateIfNotExists();

        }

        //-------------------------------------


        /// <summary>
        /// Logs IP to for statistics
        /// </summary>
        public static void LogIpAddress(HttpRequestData incomingRequest)
        {
            try
            {
                //# get ip address out
                var ipAddress = incomingRequest?.GetCallerIp()?.ToString() ?? "0.0.0.0";

                //# check if ip address already exist
                //make a search for ip address stored under row key
                Expression<Func<IpAddressStatisticEntity, bool>> expression = call => call.PartitionKey == ipAddress;

                //execute search
                var recordFound = ipAddressStatisticTableClient.Query(expression).FirstOrDefault();

                //# if existed, update call count
                var isExist = recordFound != null;
                if (isExist)
                {
                    //update row
                    recordFound.CallCount = ++recordFound.CallCount; //increment call count
                    ipAddressStatisticTableClient.UpsertEntity(recordFound);
                }

                //# if not exist, make new log
                else
                {
                    var newRow = new IpAddressStatisticEntity();
                    newRow.PartitionKey = Tools.CleanAzureTableKey(ipAddress);
                    //get month and year in correct format 2019-10
                    newRow.RowKey = DateTime.Now.ToString("yyyy-MM");
                    newRow.CallCount = 1;
                    ipAddressStatisticTableClient.UpsertEntity(newRow);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"Telemetry IP Address logging failed: {e.Message}");
#endif
            }
        }

        public static void LogRequestUrl(HttpRequestData incomingRequest)
        {
            try
            {
                //# get request URL
                var requestUrl = incomingRequest?.Url.ToString() ?? "no URL";

                //# check if URL already exist
                //make a search for ip address stored under row key
                var cleanAzureTableKey = Tools.CleanAzureTableKey(requestUrl, "|");
                Expression<Func<RequestUrlStatisticEntity, bool>> expression = call => call.PartitionKey == cleanAzureTableKey;

                //execute search
                var recordFound = requestUrlStatisticTableClient.Query(expression).FirstOrDefault();

                //# if existed, update call count
                var isExist = recordFound != null;
                if (isExist)
                {
                    //update row
                    recordFound.CallCount = ++recordFound.CallCount; //increment call count
                    requestUrlStatisticTableClient.UpsertEntity(recordFound);
                }

                //# if not exist, make new log
                else
                {
                    var newRow = new RequestUrlStatisticEntity();

                    newRow.PartitionKey = cleanAzureTableKey;
                    //get month and year in correct format 2019-10
                    newRow.RowKey = DateTime.Now.ToString("yyyy-MM");
                    newRow.CallCount = 1;
                    requestUrlStatisticTableClient.UpsertEntity(newRow);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"Telemetry Request URL logging failed: {e.Message}");
#endif
            }
        }


        public static void LogSubscriber(HttpRequestData incomingRequest)
        {
            try
            {
                //get host address as main ID of record
                var requestHeaderList = incomingRequest.Headers.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
                requestHeaderList.TryGetValue("Host", out var hostValues);
                var host = hostValues?.FirstOrDefault() ?? "no host";

                //get date that this record would be in (Row Key)
                var currentDate = DateTime.Now.ToString("yyyy-MM");

                //# check if URL already exist
                //make a search for ip address stored under row key
                var cleanHostAddress = Tools.CleanAzureTableKey(host, "|");
                Expression<Func<RequestUrlStatisticEntity, bool>> expression = call =>
                        call.PartitionKey == cleanHostAddress &&
                        call.RowKey == currentDate;

                //execute search
                var recordFound = subscriberStatisticTableClient.Query(expression).FirstOrDefault();

                //# if existed, update call count
                var isExist = recordFound != null;
                if (isExist)
                {
                    //update row
                    recordFound.CallCount = ++recordFound.CallCount; //increment call count
                    subscriberStatisticTableClient.UpsertEntity(recordFound);
                }

                //# if not exist, make new log
                else
                {
                    var newRow = new SubscriberStatisticEntity();
                    newRow.PartitionKey = cleanHostAddress;
                    //get month and year in correct format 2019-10
                    newRow.RowKey = currentDate;
                    newRow.CallCount = 1; //start with 1
                    //save to db
                    subscriberStatisticTableClient.UpsertEntity(newRow);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"Telemetry Subscriber logging failed: {e.Message}");
#endif
            }
        }

        public static void LogUserAgent(HttpRequestData incomingRequest)
        {
            try
            {
                //get host address as main ID of record
                var requestHeaderList = incomingRequest.Headers.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
                requestHeaderList.TryGetValue("User-Agent", out var userAgentValues);
                var userAgent = userAgentValues?.FirstOrDefault() ?? "no User-Agent";

                //get date that this record would be in (Row Key)
                var currentDate = DateTime.Now.ToString("yyyy-MM");

                //# check if User-Agent already exist
                //make a search for ip address stored under row key
                var cleanUserAgent = Tools.CleanAzureTableKey(userAgent, "|");
                Expression<Func<UserAgentStatisticEntity, bool>> expression = call => call.PartitionKey == cleanUserAgent;

                //execute search
                var recordFound = userAgentStatisticTableClient.Query(expression).FirstOrDefault();

                //# if existed, update call count
                var isExist = recordFound != null;
                if (isExist)
                {
                    //update row
                    recordFound.CallCount = ++recordFound.CallCount; //increment call count
                    userAgentStatisticTableClient.UpsertEntity(recordFound);
                }

                //# if not exist, make new log
                else
                {
                    var newRow = new UserAgentStatisticEntity();
                    newRow.PartitionKey = cleanUserAgent;
                    //get month and year in correct format 2019-10
                    newRow.RowKey = currentDate;
                    newRow.CallCount = 1; //start with 1
                    //save to db
                    userAgentStatisticTableClient.UpsertEntity(newRow);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"Telemetry User Agent logging failed: {e.Message}");
#endif
            }
        }

        /// <summary>
        /// Makes raw full header log of what ever that comes in
        /// NOTE: high cost carefully use
        /// </summary>
        public static void LogRawRequest(HttpRequestData incomingRequest)
        {
            try
            {
                //step 1: extract needed data from request
                var newRow = new RawRequestStatisticEntity();

                //convert to list
                var requestHeaderList = incomingRequest.Headers.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

                for (int i = 0; i < requestHeaderList.Count; i++)
                {
                    var currentHeader = requestHeaderList.ElementAt(i);
                    var currentHeaderKey = currentHeader.Key;
                    string currentValue = Tools.ListToString(currentHeader.Value.ToList());

                    //debug print
                    //Console.WriteLine($"{currentHeaderKey}:{currentValue}");

                    //match with correct header based on attribute and fill in the value
                    // Get all properties of the current instance
                    var properties = newRow.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        var attribute = (DescriptionAttribute)property.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                        if (attribute?.Description.Equals(currentHeaderKey, StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            property.SetValue(newRow, currentValue);
                            break;
                        }
                    }
                }

                //step 2: generate hash to identify the data
                newRow.PartitionKey = incomingRequest?.GetCallerIp()?.ToString() ?? "no ip";
                //newRow.PartitionKey = newRow.CalculateCombinedHash();
                var url = incomingRequest.Url.ToString() ?? "no URL";
                newRow.RowKey = Tools.CleanAzureTableKey(url, "|"); //place url

                //step 3: add entry to database
                //TODO check if exist before overwrite
                rawRequestStatisticTableClient.UpsertEntity(newRow);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"Telemetry Raw Request logging failed: {e.Message}");
#endif
            }
        }

        public static void LogFail(HttpRequestData incomingRequest)
        {
            try
            {
                //# get ip address out
                var ipAddress = incomingRequest?.GetCallerIp()?.ToString() ?? "0.0.0.0";

                //# check if ip address already exist
                //make a search for ip address stored under row key
                Expression<Func<IpAddressStatisticEntity, bool>> expression = call => call.PartitionKey == ipAddress;

                //execute search
                var recordFound = ipAddressStatisticTableClient.Query(expression).FirstOrDefault();

                //# if existed, update call count
                var isExist = recordFound != null;
                if (isExist)
                {
                    //update row
                    recordFound.CallCount = ++recordFound.CallCount; //increment call count
                    ipAddressStatisticTableClient.UpsertEntity(recordFound);
                }

                //# if not exist, make new log
                else
                {
                    var newRow = new IpAddressStatisticEntity();
                    newRow.PartitionKey = Tools.CleanAzureTableKey(ipAddress);
                    //get month and year in correct format 2019-10
                    newRow.RowKey = DateTime.Now.ToString("yyyy-MM");
                    newRow.CallCount = 1;
                    ipAddressStatisticTableClient.UpsertEntity(newRow);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"Telemetry Fail logging failed: {e.Message}");
#endif
            }
        }

    }

}
