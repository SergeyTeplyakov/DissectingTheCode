namespace DtC.Episode10;

public class CodeForDemo
{
    // Windows Forms: synchronous version
    public void ProcessData()
    {
        UpdateProgressBar("Start");
        // Blocking call! Not good!
        string result = GetData();
        UpdateTextBox(result);
    }

    // Windows Forms: asynchronous version
    public async Task ProcessDataAsync()
    {
        UpdateProgressBar("Start");
        // Blocking call! Not good!
        string result = await GetDataAsync();
        UpdateTextBox(result);
    }

    private Task<string> GetDataAsync()
    {
        return Task.FromResult("42");
    }

    private void UpdateTextBox(string result)
    {
        // Updating a text box.
    }

    private string GetData()
    {
        return "42";
    }

    private void UpdateProgressBar(string data)
    {
        // Updates the progress bar
    }
}