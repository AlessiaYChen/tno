using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("user")]
public class User : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = "";

    [Column("email")]
    public string Email { get; set; } = "";

    [Column("key")]
    public Guid Key { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; } = "";

    [Column("first_name")]
    public string FirstName { get; set; } = "";

    [Column("last_name")]
    public string LastName { get; set; } = "";

    [Column("is_enabled")]
    public bool IsEnabled { get; set; }

    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("last_login_on")]
    public DateTime? LastLoginOn { get; set; }

    public List<Role> Roles { get; } = new List<Role>();

    public List<Content> Contents { get; } = new List<Content>();

    public List<TonePool> TonePools { get; } = new List<TonePool>();

    public List<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();
    #endregion

    #region Constructors
    protected User() { }

    public User(string username, string email, Guid key)
    {
        if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("Parameter is required, not null, empty or whitespace", nameof(username));
        if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("Parameter is required, not null, empty or whitespace", nameof(email));

        this.Username = username;
        this.Email = email;
        this.Key = key;
        this.DisplayName = username;
    }
    #endregion
}