﻿using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.SqlServer
{
    [DbConfigurationType(typeof(CustomApplicationDbConfiguration))]
    internal class CatalogSyncDbContext : CatalogDbContextBase
    {
        public CatalogSyncDbContext(string file) : base(GetConnectionString(file, false, string.Empty))
        {

        }

        private CatalogSyncDbContext(string file, string initCatalog) : base(GetConnectionString(file, initCatalog))
        {

        }

        private CatalogSyncDbContext(string file, bool isInitCatalog, string initCatalogName) : base(GetConnectionString(file, isInitCatalog, initCatalogName)) { }

        private CatalogSyncDbContext(string file, bool initCatalog) : base(GetConnectionString(file, initCatalog))
        {

        }

        private static string GetConnectionString(string filePath, bool initCatalog, string initCatalogName)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            if (initCatalog)
                sqlBuilder.InitialCatalog = initCatalogName;

            sqlBuilder.SetDataSource();
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;
            Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.DEBUG, sqlBuilder.ToString());
            return sqlBuilder.ToString();
        }

        private static string GetConnectionString(string filePath, bool initCatalog)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            //sqlBuilder.InitialCatalog = initCatalog;

            sqlBuilder.SetDataSource();
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;
            Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.DEBUG, sqlBuilder.ToString());
            return sqlBuilder.ToString();
        }

        private static string GetConnectionString(string filePath, string initCatalog)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            sqlBuilder.InitialCatalog = initCatalog;

            sqlBuilder.SetDataSource();
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;
            Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.DEBUG, sqlBuilder.ToString());
            return sqlBuilder.ToString();
        }

        private static string GetConnectionString(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
            sqlBuilder.InitialCatalog = Path.GetFileNameWithoutExtension(filePath);

            sqlBuilder.SetDataSource();
            sqlBuilder.AttachDBFilename = filePath;
            sqlBuilder.IntegratedSecurity = true;
            Log.LogFactory.LogInstance.WriteLog(Log.LogLevel.DEBUG, sqlBuilder.ToString());
            return sqlBuilder.ToString();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<CaseSensitiveAttribute, CaseSensitiveAttribute>(
            "CaseSensitive",
            (property, attributes) => attributes.Single()));
            base.OnModelCreating(modelBuilder);
        }
        
    }

    public static class SqlConnectStringBuilderEx
    {
        public static void SetDataSource(this SqlConnectionStringBuilder sb)
        {
            if (CloudConfig.Instance.SqlServerVersion == "2012")
            {
                sb.DataSource = @"(LocalDb)\v11.0";
            }
            else if (CloudConfig.Instance.SqlServerVersion == "2014")
            {
                sb.DataSource = @"(LocalDb)\MSSQLLocalDB";
            }
            else
                throw new NotSupportedException("Not support the sql server version");
        }
    }
}