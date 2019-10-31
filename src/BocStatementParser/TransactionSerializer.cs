﻿using System.Globalization;
using System.Text;

namespace BocStatementParser
{
    public class TransactionSerializer
    {
        public string Serialize(Transaction[] transactions)
        {
            var result = new StringBuilder("Date,Description,Amount")
                .AppendLine();
            foreach (var trxn in transactions)
            {
                result.Append(trxn.Date?.ToString("dd/MM/yyyy")).Append(",");
                if (trxn.Description.Contains(","))
                    result.Append("\"").Append(trxn.Description?.Replace("\"", "\"\"")).Append("\",");
                else
                    result.Append(trxn.Description).Append(",");
                result.Append(trxn.Amount?.ToString("G29", CultureInfo.InvariantCulture))
                    .AppendLine();
            }

            return result.ToString();
        }
    }
}