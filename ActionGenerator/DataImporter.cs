using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ConsoleApp2;

namespace ActionGenerator
{
    public static class DataImporter
    {
        private static readonly SqlBulkCopy _sqlBulkCopy = new SqlBulkCopy("Data Source=.;Integrated Security=true;Initial Catalog=dbTrackAction", SqlBulkCopyOptions.Default)
        {
            DestinationTableName = "[tbl_action_participant]",
            BatchSize = 500
        };
        
        public static Task BulkInsertAsync(ActionParticipant[] items)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(nameof(ActionParticipant.action_id), typeof(int));
            dt.Columns.Add(nameof(ActionParticipant.account_id), typeof(int));
            dt.Columns.Add(nameof(ActionParticipant.contact_id), typeof(int));
            dt.Columns.Add(nameof(ActionParticipant.action_dtm), typeof(DateTime));
            dt.Columns.Add(nameof(ActionParticipant.company_id), typeof(int));
            dt.Columns.Add(nameof(ActionParticipant.recent_ind), typeof(bool));

            foreach (var item in items)
            {
                var row = dt.NewRow();

                row[nameof(ActionParticipant.action_id)] = item.action_id;
                SetValue(row, nameof(ActionParticipant.account_id), item.account_id);
                SetValue(row, nameof(ActionParticipant.contact_id), item.contact_id);
                row[nameof(ActionParticipant.action_dtm)] = item.action_dtm;
                row[nameof(ActionParticipant.company_id)] = item.company_id;
                row[nameof(ActionParticipant.recent_ind)] = item.recent_ind;

                dt.Rows.Add(row);
            }

            return _sqlBulkCopy.WriteToServerAsync(dt);
        }

        private static void SetValue(DataRow row, string column, int? itemValue)
        {
            if (itemValue.HasValue)
            {
                row[column] = itemValue.Value;
            }
            else
            {
                row[column] = DBNull.Value;
            }
        }
    }
}
