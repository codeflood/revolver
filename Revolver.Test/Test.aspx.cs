using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;

namespace Codeflood.Testing
{
  public partial class TestRunner : System.Web.UI.Page, EventListener
  {
    #region Member Variables
    DataTable _results = new DataTable();
    private int _executedCount = 0;
    private int _failedCount = 0;
    private TestPackage _testPackage = null;
    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
      // Initialise data table to hold test results
      _results.Columns.Add("test");
      _results.Columns.Add("result");
      _results.Columns.Add("time");
      _results.Columns.Add("message");
      _results.Columns.Add("class");

      // Initialise controls
      lblResult.Text = "";
      ltlStats.Text = "";

      // Initialise NUnit
      CoreExtensions.Host.InitializeService();

      // Find tests in current assembly
      _testPackage = new TestPackage(Assembly.GetExecutingAssembly().Location);

      if (!IsPostBack)
      {
        var testSuite = new TestSuiteBuilder().Build(_testPackage);
        var categoryManager = new CategoryManager();
        categoryManager.AddAllCategories(testSuite);

        cblCategories.DataSource = (from string cat in categoryManager.Categories select cat).OrderBy(x => x);
        cblCategories.DataBind();
      }
    }

    protected void RunClick(object sender, EventArgs args)
    {
      var categories = from ListItem item in cblCategories.Items
                       where item.Selected
                       select item.Value;

      if(!categories.Any())
        categories = from ListItem item in cblCategories.Items
                       select item.Value;

      // Create a category filter
      var filter = new CategoryFilter(categories.ToArray());

      var runner = new SimpleTestRunner();
      runner.Load(_testPackage);

      var result = runner.Run(this, filter, true, LoggingThreshold.All);

      // Bind results to presentation
      gvResults.DataSource = _results;
      gvResults.DataBind();

      // Display statistics
      ltlStats.Text = string.Format("{0} out of {1} tests run in {2} seconds.", _executedCount, result.Test.TestCount, result.Time);

      if (_failedCount > 0)
        ltlStats.Text += string.Format("<br/>{0} {1} failed", _failedCount, _failedCount == 1 ? "test" : "tests");

      var skipped = result.Test.TestCount - _executedCount;
      if (skipped > 0)
        ltlStats.Text += string.Format("<br/>{0} {1} skipped", skipped, skipped == 1 ? "test" : "tests");

      lblResult.Text = "Suite " + (result.IsSuccess ? "Passed" : "Failed");
      if (result.IsSuccess)
        lblResult.CssClass = "passLabel";
      else
        lblResult.CssClass = "failLabel";
    }

    #region EventListener Members

    public void RunFinished(Exception exception)
    {
    }

    public void RunFinished(TestResult result)
    {
    }

    public void RunStarted(string name, int testCount)
    {
    }

    public void SuiteFinished(TestResult result)
    {
    }

    public void SuiteStarted(TestName testName)
    {
    }

    public void TestFinished(TestResult result)
    {
      // Put results into data table
      var dr = _results.NewRow();
      dr["test"] = result.Test.TestName;
      dr["message"] = result.Message;
      if (result.IsFailure)
        dr["message"] += result.StackTrace;
      dr["class"] = "notRun";
      dr["time"] = result.Time;

      if (result.IsSuccess && result.Executed)
      {
        dr["result"] = "Pass";
        dr["class"] = "pass";
      }

      if (result.IsFailure && result.Executed)
      {
        dr["result"] = "Fail";
        dr["class"] = "fail";
        _failedCount++;
      }

      if (result.Executed)
        _executedCount++;

      _results.Rows.Add(dr);
    }

    public void TestOutput(TestOutput testOutput)
    {
    }

    public void TestStarted(TestName testName)
    {
    }

    public void UnhandledException(Exception exception)
    {
    }

    #endregion
  }
}
