namespace Timesheet.DomainServices.Dto
{
    public class UserPunishmentSnapshotDto
    {
        public bool IsPaid { get; set; }
        public string UserNote { get; set; }
        public string NoteReply { get; set; }
        public long? UserPunishmentPaidId { get; set; }
    }
}
