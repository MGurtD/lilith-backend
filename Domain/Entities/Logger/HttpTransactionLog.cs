using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Audit
{
    [Table("LogHttpTransactions", Schema = "audit")]
    public class HttpTransactionLog
    {
        [Key]
        public int Id { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string QueryString { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public long Duration { get; set; }
    }
}
