using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MomentumDiscordBot.Models.Data
{
    [Table("message_count")]
    public class DailyMessageCount
    {
        [Required]
        [Column(TypeName = "BIGINT UNSIGNED NOT NULL")]
        public ulong UserId { get; set; }

        [Required]
        [Column(TypeName = "BIGINT UNSIGNED NOT NULL")]
        public ulong ChannelId { get; set; }

        [Required]
        [Column(TypeName = "DATE NOT NULL")]
        public DateTime Date { get; set; }

        [Column(TypeName = "MEDIUMINT UNSIGNED")]
        public uint MessageCount { get; set; }
    }
}