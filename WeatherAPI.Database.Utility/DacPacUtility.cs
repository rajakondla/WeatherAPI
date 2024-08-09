using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace Collette.Neo.Database.Utility
{
    public static class DacPacUtility
    {
        public static void DeployDacPac(string connString, string dacPacPath, string targetDbName, DacDeployOptions dbDeployOptions)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    retryCount++;
                    DeployDacPacInternal(connString, dacPacPath, targetDbName, dbDeployOptions);

                    return;
                }
                catch (SqlException ex) when (ex.ErrorCode == 1205)
                {
                    if (retryCount == 3)
                        throw;

                    DeployDacPacInternal(connString, dacPacPath, targetDbName, dbDeployOptions);
                }
            }
        }

        public static void DeployDacPac(string testNeoDbConnectionString, object value, string testNeoDatabaseName, DacDeployOptions neoDbDeployOptions)
        {
            throw new NotImplementedException();
        }

        private static void DeployDacPacInternal(string connString, string dacPacPath, string targetDbName, DacDeployOptions dbDeployOptions) {
            using (var dbPackage = DacPackage.Load(dacPacPath, DacSchemaModelStorageType.Memory))
            {
                dbDeployOptions.IncludeCompositeObjects = true;
                dbDeployOptions.BlockOnPossibleDataLoss = false;
                dbDeployOptions.DropObjectsNotInSource = true;
                dbDeployOptions.BlockWhenDriftDetected = false;
                dbDeployOptions.ScriptDatabaseCompatibility = true;
                dbDeployOptions.ScriptDatabaseCollation = true;
                dbDeployOptions.GenerateSmartDefaults = true;
                dbDeployOptions.CreateNewDatabase = true;
                dbDeployOptions.ExcludeObjectTypes = new[] { ObjectType.ColumnMasterKeys, ObjectType.ColumnEncryptionKeys };

                var dbServices = new DacServices(connString);
                dbServices.Deploy(dbPackage, targetDbName, true, dbDeployOptions);
            }
        }
    }
}
