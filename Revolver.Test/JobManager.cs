using NUnit.Framework;
using Revolver.Core;
using Sitecore.Jobs;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using Cmd = Revolver.Core.Commands;

namespace Revolver.Test
{
  [TestFixture]
  [Category("JobManager")]
  public class JobManager : BaseCommandTest
  {
    [Test]
    public void ListJobsNoneRunning()
    {
      var cmd = new Cmd.JobManager();
      base.InitCommand(cmd);

      var jobCount = Sitecore.Jobs.JobManager.GetJobs().Length;

      var result = cmd.Run();

      Assert.AreEqual(CommandStatus.Success, result.Status);
      Assert.IsTrue(result.Message.ToLower().Contains(jobCount.ToString() + " jobs found"), "Wrong message detected: " + result.Message);
    }

    [Test]
    public void ListJobsWithAdditionalJob()
    {
      var cmd = new Cmd.JobManager();
      base.InitCommand(cmd);

      var jobs = Sitecore.Jobs.JobManager.GetJobs();
      int jobCount = jobs.Length;
      int runningJobCount = 0;
      for (int i = 0; i < jobs.Length; i++)
      {
        if (jobs[i].Status.State == JobState.Running)
          runningJobCount++;
      }

      var job = new Job(new JobOptions("testing", "unit tests", "test", this, "JobBody"));
      Sitecore.Jobs.JobManager.Start(job);

      var result = cmd.Run();
      Assert.AreEqual(CommandStatus.Success, result.Status);

      // Match regex on guid which forms part of the job handle (or the entire job handle on older Sitecore versions)
      Assert.AreEqual(jobCount + 1, Regex.Matches(result.Message, @"[\da-z]{8}-[\da-z]{4}-[\da-z]{4}-[\da-z]{4}-[\da-z]{12}").Count);

      MatchCollection matches = Regex.Matches(result.Message, "Running");
      Assert.AreEqual(runningJobCount + 1, matches.Count);
      Assert.IsTrue(Regex.IsMatch(result.Message, "job[s]? found"), "Wrong message detected: " + result.Message);
    }

    protected void JobBody()
    {
      Thread.Sleep(2000);
    }
  }
}
