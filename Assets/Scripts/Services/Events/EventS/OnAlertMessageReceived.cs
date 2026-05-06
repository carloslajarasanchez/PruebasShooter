public class OnAlertMessageReceived : OwnEventBase
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool ShowTitle => !string.IsNullOrEmpty(Title);

    public OnAlertMessageReceived(string description, string title = null)
    {
        this.Description = description;
        this.Title = title;
    }
}
