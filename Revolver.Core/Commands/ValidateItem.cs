using Sitecore.Data.Validators;
using System.Text;

namespace Revolver.Core.Commands
{
	[Command("ival")]
  public class ValidateItem : BaseCommand
	{
    [FlagParameter("g")]
    [Description("Run validators defined for the 'gutter'.")]
    [Optional]
    public bool ModeGutter { get; set; }

    [FlagParameter("btn")]
    [Description("Run validators defined for the 'validation button'.")]
    [Optional]
    public bool ModeButton { get; set; }

    [FlagParameter("b")]
    [Description("Run validators defined for the 'validation bar'.")]
    [Optional]
    public bool ModeBar { get; set; }

    [FlagParameter("w")]
    [Description("Run validators defined for 'workflow'.")]
    [Optional]
    public bool ModeWorkflow { get; set; }

    [FlagParameter("v")]
    [Description("Verbose. Output all validation results.")]
    [Optional]
    public bool Verbose { get; set; }

    [NumberedParameter(0, "path")]
    [Description("The path to execute the command on.")]
    [Optional]
    public string Path { get; set; }

		public override CommandResult Run()
		{
      // Check both item level rules and global rules. Looks like page editor may run global rules separatley.

      var all = !(ModeGutter || ModeButton || ModeBar || ModeWorkflow);

      var output = new StringBuilder();
      var count = 0;

      var result = true;

      using (var cs = new ContextSwitcher(Context, Path))
      {
        if (cs.Result.Status != CommandStatus.Success)
          return cs.Result;

        if (all || ModeButton)
          result &= RunValidation(ValidatorsMode.ValidateButton, output, ref count);

        if (all || ModeGutter)
          result &= RunValidation(ValidatorsMode.Gutter, output, ref count);

        if (all || ModeBar)
          result &= RunValidation(ValidatorsMode.ValidatorBar, output, ref count);

        if (all || ModeWorkflow)
          result &= RunValidation(ValidatorsMode.Workflow, output, ref count);
      }

      output.AppendLine();

      if(Verbose)
        output.AppendLine(string.Format("Ran {0} validator{1}", count, count == 1 ? string.Empty : "s"));

      if (result)
        output.AppendLine("Validation passed");
      else
        output.AppendLine(string.Format("FAILED: Validation failed for '{0}'", Context.CurrentItem.Name)); ;

      var status = result ? CommandStatus.Success : CommandStatus.Failure;
      return new CommandResult(status, output.ToString());
		}

    private bool RunValidation(ValidatorsMode mode, StringBuilder output, ref int count)
    {
      var validators = ValidatorManager.BuildValidators(mode, Context.CurrentItem);
      count += validators.Count;

      var options = new ValidatorOptions(true);
      ValidatorManager.Validate(validators, options);

      var success = true;

      foreach(BaseValidator val in validators)
      {
        if (Verbose || val.Result != ValidatorResult.Valid)
          Formatter.PrintDefinition(val.Name, val.Result.ToString(), 60, output);

        if (val.Result != ValidatorResult.Valid)
          success = false;
      }

      return success;
    }

		public override string Description()
		{
			return "Perform validation for the item";
		}

    public override void Help(HelpDetails details)
		{
      details.Comments = "With no 'mode' flags (g, btn, b, w) this command will run all validators defined.";
			//details.Usage = "<cmd> [path]";
		}
	}
}
