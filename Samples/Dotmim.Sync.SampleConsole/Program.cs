﻿using Dotmim.Sync;
using Dotmim.Sync.Data;
using Dotmim.Sync.Data.Surrogate;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.SampleConsole;
using Dotmim.Sync.Serialization;
using Dotmim.Sync.Sqlite;
using Dotmim.Sync.SqlServer;
using Dotmim.Sync.Tests.Models;
using Dotmim.Sync.Web.Client;
using Dotmim.Sync.Web.Server;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;


internal class Program
{
    public static string serverDbName = "AdventureWorks";
    public static string clientDbName = "Client";
    public static string[] allTables = new string[] {"ProductCategory",
                                                    "ProductModel", "Product",
                                                    "Address", "Customer", "CustomerAddress",
                                                    "SalesOrderHeader", "SalesOrderDetail" };
    private static void Main(string[] args)
    {
        SyncHttpThroughKestellAsync().GetAwaiter().GetResult();

 
        Console.ReadLine();
    }


    private static void TestSerializers()
    {
        var dslight = new DmSetLight(GetSet());

        var serializer1 = new ContractSerializer<DmSetLight>();
        var serializer2 = new JsonConverter<DmSetLight>();
        var serializer3 = new CustomMessagePackSerializer<DmSetLight>();

        var bin1 = serializer1.Serialize(dslight);
        var bin2 = serializer2.Serialize(dslight);
        var bin3 = serializer3.Serialize(dslight);

        var json3 = MessagePack.MessagePackSerializer.ToJson(bin3);
        string json2;
        using (var ms = new MemoryStream(bin2))
        {
            using (var sr = new StreamReader(ms))
            {
                json2 = sr.ReadToEnd();
            }
        }



        DmSetLight newDmSetLight1;
        DmSetLight newDmSetLight2;
        DmSetLight newDmSetLight3;
        DmSet newDmSet1;
        DmSet newDmSet2;
        DmSet newDmSet3;
        using (var ms1 = new MemoryStream(bin1))
        {
            newDmSetLight1 = serializer1.Deserialize(ms1);
            newDmSet1 = CreateDmSet();
            newDmSetLight1.WriteToDmSet(newDmSet1);
        }
        using (var ms2 = new MemoryStream(bin2))
        {
            newDmSetLight2 = serializer2.Deserialize(ms2);
            newDmSet2 = CreateDmSet();
            newDmSetLight2.WriteToDmSet(newDmSet2);
        }
        using (var ms3 = new MemoryStream(bin3))
        {
            newDmSetLight3 = serializer3.Deserialize(ms3);
            newDmSet3 = CreateDmSet();
            newDmSetLight3.WriteToDmSet(newDmSet3);
        }

    }


    private static DmSet CreateDmSet()
    {
        var set = new DmSet("ClientDmSet");

        var tbl = new DmTable("ServiceTickets");
        set.Tables.Add(tbl);
        var id = new DmColumn<Guid>("ServiceTicketID");
        tbl.Columns.Add(id);
        var key = new DmKey(new DmColumn[] { id });
        tbl.PrimaryKey = key;
        tbl.Columns.Add(new DmColumn<string>("Title"));
        tbl.Columns.Add(new DmColumn<string>("Description"));
        tbl.Columns.Add(new DmColumn<int>("StatusValue"));
        tbl.Columns.Add(new DmColumn<int>("EscalationLevel"));
        tbl.Columns.Add(new DmColumn<DateTime>("Opened"));
        tbl.Columns.Add(new DmColumn<DateTime>("Closed"));
        tbl.Columns.Add(new DmColumn<int>("CustomerID"));

        return set;
    }

    private static DmSet GetSet()
    {
        var set = CreateDmSet();
        var tbl = set.Tables[0];

        #region adding rows
        var st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre AER";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 1;
        st["StatusValue"] = 2;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 1;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre DE";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 3;
        st["StatusValue"] = 2;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 1;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre FF";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 3;
        st["StatusValue"] = 4;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 2;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre AC";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 1;
        st["StatusValue"] = 2;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 2;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre ZDZDZ";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 1;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 2;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre VGH";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 1;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 3;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre ETTG";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 2;
        st["StatusValue"] = 1;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 3;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre SADZD";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 1;
        st["StatusValue"] = 1;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 3;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre AEEE";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 0;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 1;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre CZDADA";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 0;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 3;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre AFBBB";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 3;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 3;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre AZDCV";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 2;
        st["StatusValue"] = 2;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 2;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre UYTR";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 1;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 3;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre NHJK";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 1;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 1;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre XCVBN";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 1;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 2;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre LKNB";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 3;
        st["StatusValue"] = 2;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 3;
        tbl.Rows.Add(st);

        st = tbl.NewRow();
        st["ServiceTicketID"] = Guid.NewGuid();
        st["Title"] = "Titre ADFVB";
        st["Description"] = "Description 2";
        st["EscalationLevel"] = 0;
        st["StatusValue"] = 2;
        st["Opened"] = DateTime.Now;
        st["Closed"] = null;
        st["CustomerID"] = 1;
        tbl.Rows.Add(st);
        #endregion

        tbl.AcceptChanges();

        st.Delete();


        return set;
    }


    private static async Task SynchronizeWithSyncAgent2Async()
    {
        // Create 2 Sql Sync providers
        var serverProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString("AdventureWorks"));
        var clientProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString("Client"));

        // Tables involved in the sync process:
        var tables = new string[] { "ProductCategory", "ProductModel", "Product" };

        // Creating an agent that will handle all the process
        var agent = new SyncAgent(clientProvider, serverProvider, tables);

        // Using the Progress pattern to handle progession during the synchronization
        var progress = new SynchronousProgress<ProgressArgs>(s =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{s.Context.SyncStage}:\t{s.Message}");
            Console.ResetColor();
        });

        var remoteProgress = new SynchronousProgress<ProgressArgs>(s =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{s.Context.SyncStage}:\t{s.Message}");
            Console.ResetColor();
        });
        agent.AddRemoteProgress(remoteProgress);

        // Setting configuration options
        agent.SetSchema(s =>
        {
            s.StoredProceduresPrefix = "s";
            s.StoredProceduresSuffix = "";
            s.TrackingTablesPrefix = "t";
            s.TrackingTablesSuffix = "";
        });

        agent.SetOptions(opt =>
        {
            opt.ScopeInfoTableName = "tscopeinfo";
            opt.BatchDirectory = Path.Combine(SyncOptions.GetDefaultUserBatchDiretory(), "sync");
            opt.BatchSize = 100;
            opt.CleanMetadatas = true;
            opt.UseBulkOperations = true;
            opt.UseVerboseErrors = false;
        });



        agent.LocalOrchestrator.OnTransactionOpen(to =>
        {
            var dt = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Transaction Opened\t {dt.ToLongTimeString()}.{dt.Millisecond}");
            Console.ResetColor();
        });
        agent.LocalOrchestrator.OnTransactionCommit(to =>
        {
            var dt = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Transaction Commited\t {dt.ToLongTimeString()}.{dt.Millisecond}");
            Console.ResetColor();
        });


        agent.RemoteOrchestrator.OnTransactionOpen(to =>
        {
            var dt = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Transaction Opened\t {dt.ToLongTimeString()}.{dt.Millisecond}");
            Console.ResetColor();
        });
        agent.RemoteOrchestrator.OnTransactionCommit(to =>
        {
            var dt = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Transaction Commited\t {dt.ToLongTimeString()}.{dt.Millisecond}");
            Console.ResetColor();
        });

        do
        {
            Console.Clear();
            Console.WriteLine("Sync Start");
            try
            {
                // Launch the sync process
                var s1 = await agent.SynchronizeAsync(SyncType.Normal, CancellationToken.None, progress);

                // Write results
                Console.WriteLine(s1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            //Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        } while (Console.ReadKey().Key != ConsoleKey.Escape);

        Console.WriteLine("End");
    }


    private async static Task RunAsync()
    {
        // Create databases 
        await DbHelper.EnsureDatabasesAsync(serverDbName);
        await DbHelper.CreateDatabaseAsync(clientDbName);

        // Launch Sync
        await SynchronizeAsync();
    }


    private static async Task TrySyncAzureSqlDbAsync()
    {
        //// Sql Server provider, the master.
        //var serverProvider = new SqlSyncProvider(
        //    @"Data Source=sebpertus.database.windows.net;Initial Catalog=AdventureWorks;User Id=YOUR_ID;Password=YOUR_PASSWORD;");

        //// Sqlite Client provider for a Sql Server <=> Sqlite sync
        //var clientProvider = new SqliteSyncProvider("advfromazure.db");

        //// Tables involved in the sync process:
        //var tables = new string[] { "Address" };

        //// Sync orchestrator
        //var agent = new SyncAgent(clientProvider, serverProvider, tables);

        //do
        //{
        //    var s = await agent.SynchronizeAsync();
        //    Console.WriteLine($"Total Changes downloaded : {s.TotalChangesDownloaded}");

        //} while (Console.ReadKey().Key != ConsoleKey.Escape);
    }

    private static async Task SyncAdvAsync()
    {
        //// Sql Server provider, the master.
        //var serverProvider = new SqlSyncProvider(
        //    @"Data Source=.;Initial Catalog=AdventureWorks;User Id=sa;Password=Password12!;");

        //// Sqlite Client provider for a Sql Server <=> Sqlite sync
        //var clientProvider = new SqliteSyncProvider("advworks2.db");

        //// Tables involved in the sync process:
        //var tables = new string[] {"ProductCategory",
        //        "ProductDescription", "ProductModel",
        //        "Product", "ProductModelProductDescription",
        //        "Address", "Customer", "CustomerAddress",
        //        "SalesOrderHeader", "SalesOrderDetail" };

        //// Sync orchestrator
        //var agent = new SyncAgent(clientProvider, serverProvider, tables);


        //do
        //{
        //    var s = await agent.SynchronizeAsync();
        //    Console.WriteLine($"Total Changes downloaded : {s.TotalChangesDownloaded}");

        //} while (Console.ReadKey().Key != ConsoleKey.Escape);
    }


    /// <summary>
    /// Launch a simple sync, over TCP network, each sql server (client and server are reachable through TCP cp
    /// </summary>
    /// <returns></returns>
    private static async Task SynchronizeAsync()
    {
        // Create 2 Sql Sync providers
        var serverProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString("TestServer"));
        var clientProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString("TestClient"));
        //var clientProvider = new SqliteSyncProvider("advworks.db");

        // Creating an agent that will handle all the process
        var agent = new SyncAgent(clientProvider, serverProvider, new string[] { "Product" });

        // Using the Progress pattern to handle progession during the synchronization
        var progress = new Progress<ProgressArgs>(s => Console.WriteLine($"[client]: {s.Context.SyncStage}:\t{s.Message}"));


        // Setting configuration options
        agent.SetSchema(s =>
        {
            s.StoredProceduresPrefix = "s";
            s.StoredProceduresSuffix = "";
            s.TrackingTablesPrefix = "t";
            s.TrackingTablesSuffix = "";
        });

        agent.SetOptions(opt =>
        {
            opt.BatchDirectory = Path.Combine(SyncOptions.GetDefaultUserBatchDiretory(), "sync");
            opt.BatchSize = 100;
            opt.CleanMetadatas = true;
            opt.UseBulkOperations = true;
            opt.UseVerboseErrors = false;
            opt.ScopeInfoTableName = "tscopeinfo";
        });


        do
        {
            Console.Clear();
            Console.WriteLine("Sync Start");
            try
            {
                // Launch the sync process
                var s1 = await agent.SynchronizeAsync(progress);

                // Write results
                Console.WriteLine(s1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            //Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        } while (Console.ReadKey().Key != ConsoleKey.Escape);

        Console.WriteLine("End");
    }

    /// <summary>
    /// Launch a simple sync, over TCP network, each sql server (client and server are reachable through TCP cp
    /// </summary>
    /// <returns></returns>
    private static async Task SynchronizeExistingTablesAsync()
    {
        //string serverName = "ServerTablesExist";
        //string clientName = "ClientsTablesExist";

        //await DbHelper.EnsureDatabasesAsync(serverName);
        //await DbHelper.EnsureDatabasesAsync(clientName);

        //// Create 2 Sql Sync providers
        //var serverProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString(serverName));
        //var clientProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString(clientName));

        //// Tables involved in the sync process:
        //var tables = allTables;

        //// Creating an agent that will handle all the process
        //var agent = new SyncAgent(clientProvider, serverProvider, tables);

        //// Using the Progress pattern to handle progession during the synchronization
        //var progress = new Progress<ProgressArgs>(s => Console.WriteLine($"[client]: {s.Context.SyncStage}:\t{s.Message}"));




        //// Setting configuration options
        //agent.SetConfiguration(s =>
        //{
        //    s.ScopeInfoTableName = "tscopeinfo";
        //    s.SerializationFormat = Dotmim.Sync.Enumerations.SerializationFormat.Binary;
        //    s.StoredProceduresPrefix = "s";
        //    s.StoredProceduresSuffix = "";
        //    s.TrackingTablesPrefix = "t";
        //    s.TrackingTablesSuffix = "";
        //});

        //agent.SetOptions(opt =>
        //{
        //    opt.BatchDirectory = Path.Combine(SyncOptions.GetDefaultUserBatchDiretory(), "sync");
        //    opt.BatchSize = 100;
        //    opt.CleanMetadatas = true;
        //    opt.UseBulkOperations = true;
        //    opt.UseVerboseErrors = false;
        //});


        //var remoteProvider = agent.RemoteProvider as CoreProvider;

        //var dpAction = new Action<DatabaseProvisionedArgs>(args =>
        //{
        //    Console.WriteLine($"-- [InterceptDatabaseProvisioned] -- ");

        //    var sql = $"Update tscopeinfo set scope_last_sync_timestamp = 0 where [scope_is_local] = 1";

        //    var cmd = args.Connection.CreateCommand();
        //    cmd.Transaction = args.Transaction;
        //    cmd.CommandText = sql;

        //    cmd.ExecuteNonQuery();

        //});

        //remoteProvider.OnDatabaseProvisioned(dpAction);

        //agent.LocalProvider.OnDatabaseProvisioned(dpAction);

        //do
        //{
        //    Console.Clear();
        //    Console.WriteLine("Sync Start");
        //    try
        //    {
        //        // Launch the sync process
        //        var s1 = await agent.SynchronizeAsync(progress);

        //        // Write results
        //        Console.WriteLine(s1);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }


        //    //Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        //} while (Console.ReadKey().Key != ConsoleKey.Escape);

        //Console.WriteLine("End");
    }



    private static async Task SynchronizeOSAsync()
    {
        // Create 2 Sql Sync providers
        //var serverProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString("OptionsServer"));
        //var clientProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString("OptionsClient"));

        //// Tables involved in the sync process:
        //var tables = new string[] { "ObjectSettings", "ObjectSettingValues" };

        //// Creating an agent that will handle all the process
        //var agent = new SyncAgent(clientProvider, serverProvider, tables);

        //// Using the Progress pattern to handle progession during the synchronization
        //var progress = new Progress<ProgressArgs>(s => Console.WriteLine($"[client]: {s.Context.SyncStage}:\t{s.Message}"));

        //do
        //{
        //    Console.Clear();
        //    Console.WriteLine("Sync Start");
        //    try
        //    {
        //        // Launch the sync process
        //        var s1 = await agent.SynchronizeAsync(progress);

        //        // Write results
        //        Console.WriteLine(s1);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }


        //    //Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        //} while (Console.ReadKey().Key != ConsoleKey.Escape);

        //Console.WriteLine("End");
    }


    public static async Task SyncHttpThroughKestellAsync()
    {
        // server provider
        // Create 2 Sql Sync providers
        var serverProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString(serverDbName));
        var clientProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString(clientDbName));

        var proxyClientProvider = new WebClientOrchestrator();

        // Tables involved in the sync process:
        //var tables = allTables;
        var tables = new string[] { "ProductCategory" };

        // Creating an agent that will handle all the process
        var agent = new SyncAgent(clientProvider, proxyClientProvider);


        // ----------------------------------
        // Client side
        // ----------------------------------
        agent.SetOptions(opt =>
        {
            opt.ScopeInfoTableName = "client_scopeinfo";
            opt.BatchDirectory = Path.Combine(SyncOptions.GetDefaultUserBatchDiretory(), "sync_client");
            opt.BatchSize = 0;
            opt.CleanMetadatas = true;
            opt.UseBulkOperations = true;
            opt.UseVerboseErrors = false;
            //opt.Serializers.Add(new CustomMessagePackSerializerFactory(), true);
        });

        // ----------------------------------
        // Server side
        // ----------------------------------
        var schema = new Action<SyncSchema>(s =>
        {
            s.Add(tables);
            s.StoredProceduresPrefix = "s";
            s.StoredProceduresSuffix = "";
            s.TrackingTablesPrefix = "t";
            s.TrackingTablesSuffix = "";
        });

        var optionsServer = new Action<SyncOptions>(opt =>
        {
            opt.BatchDirectory = Path.Combine(SyncOptions.GetDefaultUserBatchDiretory(), "sync_server");
            opt.BatchSize = 0;
            opt.CleanMetadatas = true;
            opt.UseBulkOperations = true;
            opt.UseVerboseErrors = false;
            opt.Serializers.Add(new CustomMessagePackSerializerFactory());

        });


        var serverHandler = new RequestDelegate(async context =>
        {
            var proxyServerProvider = WebProxyServerOrchestrator.Create(context, serverProvider, schema, optionsServer);

            await proxyServerProvider.HandleRequestAsync(context);
        });
        using (var server = new KestrellTestServer())
        {
            var clientHandler = new ResponseDelegate(async (serviceUri) =>
            {
                proxyClientProvider.ServiceUri = new Uri(serviceUri);
                do
                {
                    Console.Clear();
                    Console.WriteLine("Sync Start");
                    try
                    {
                        var cts = new CancellationTokenSource();

                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("1 : Normal synchronization.");
                        Console.WriteLine("2 : Synchronization with reinitialize");
                        Console.WriteLine("3 : Synchronization with upload and reinitialize");
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine("What's your choice ? ");
                        Console.WriteLine("--------------------------------------------------");
                        var choice = Console.ReadLine();

                        if (int.TryParse(choice, out var choiceNumber))
                        {
                            Console.WriteLine($"You choose {choice}. Start operation....");
                            switch (choiceNumber)
                            {
                                case 1:
                                    var s1 = await agent.SynchronizeAsync(SyncType.Normal, cts.Token);
                                    Console.WriteLine(s1);
                                    break;
                                case 2:
                                    s1 = await agent.SynchronizeAsync(SyncType.Reinitialize, cts.Token);
                                    Console.WriteLine(s1);
                                    break;
                                case 3:
                                    s1 = await agent.SynchronizeAsync(SyncType.ReinitializeWithUpload, cts.Token);
                                    Console.WriteLine(s1);
                                    break;

                                default:
                                    break;

                            }
                        }
                    }
                    catch (SyncException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("UNKNOW EXCEPTION : " + e.Message);
                    }


                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("Press a key to choose again, or Escapte to end");

                } while (Console.ReadKey().Key != ConsoleKey.Escape);


            });
            await server.Run(serverHandler, clientHandler);
        }

    }

    /// <summary>
    /// Test a client syncing through a web api
    /// </summary>
    private static async Task TestSyncThroughWebApi()
    {
        //var clientProvider = new SqlSyncProvider(DbHelper.GetDatabaseConnectionString(clientDbName));

        //var proxyClientProvider = new WebProxyClientProvider(
        //    new Uri("http://localhost:52288/api/Sync"));

        //var agent = new SyncAgent(clientProvider, proxyClientProvider);

        //Console.WriteLine("Press a key to start (be sure web api is running ...)");
        //Console.ReadKey();
        //do
        //{
        //    Console.Clear();
        //    Console.WriteLine("Web sync start");
        //    try
        //    {
        //        var progress = new Progress<ProgressArgs>(pa => Console.WriteLine($"{pa.Context.SessionId} - {pa.Context.SyncStage}\t {pa.Message}"));

        //        var s = await agent.SynchronizeAsync(progress);

        //        Console.WriteLine(s);

        //    }
        //    catch (SyncException e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("UNKNOW EXCEPTION : " + e.Message);
        //    }


        //    Console.WriteLine("Sync Ended. Press a key to start again, or Escapte to end");
        //} while (Console.ReadKey().Key != ConsoleKey.Escape);

        //Console.WriteLine("End");

    }



}


[DataContract(Namespace = "http://Microsoft.ServiceModel.Samples")]
internal class Record
{
    private double n1;
    private double n2;
    private string operation;
    private double result;

    internal Record(double n1, double n2, string operation, double result)
    {
        this.n1 = n1;
        this.n2 = n2;
        this.operation = operation;
        this.result = result;
    }

    [DataMember]
    internal double OperandNumberOne
    {
        get { return n1; }
        set { n1 = value; }
    }

    [DataMember]
    internal double OperandNumberTwo
    {
        get { return n2; }
        set { n2 = value; }
    }

    [DataMember]
    internal string Operation
    {
        get { return operation; }
        set { operation = value; }
    }

    [DataMember]
    internal double Result
    {
        get { return result; }
        set { result = value; }
    }

    public override string ToString()
    {
        return $"Record: {n1} {operation} {n2} = {result}";
    }
}