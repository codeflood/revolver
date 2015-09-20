using System.Text;

namespace Revolver.Core.Commands
{
  [Command("ps")]
  public class JobManager : BaseCommand
  {
    public override CommandResult Run()
    {
      var output = new StringBuilder();
      var jobs = Sitecore.Jobs.JobManager.GetJobs();

      if (jobs != null)
      {
        PrintLine(output, "Job Handle", "Status", "Processed", "Name");
        PrintLine(output, "----------", "------", "---------", "----");

        foreach(var job in jobs)
        {
          var processedCount = job.Status.Processed;
          PrintLine(
            output,
            job.Handle.ToString(),
            job.Status.State.ToString(),
            processedCount > 0 ? processedCount.ToString() : string.Empty,
            job.Name
            );
        }

        Formatter.PrintLine(string.Empty, output);
        output.Append(jobs.Length.ToString() + " jobs found");
      }

      return new CommandResult(CommandStatus.Success, output.ToString());
    }

    protected void PrintLine(StringBuilder output, string handle, string status, string count, string name)
    {
      Formatter.PrintTable(new string[] { handle, status, count, name }, new[] { 70, 11, 11, 0 }, output);
    }

    public override string Description()
    {
      return "Lists the current jobs";
    }

    public override void Help(HelpDetails details)
    {
      details.AddExample(string.Empty);
    }
  }
}
