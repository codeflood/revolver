using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using System.Collections;
using System.Text;
using Sitecore.Layouts;

namespace Revolver.Core.Commands
{
  [Command("cpp")]
  public class CopyPresentation : BaseCommand
  {
    [NamedParameter("sd", "sourcedevice")]
    [Description("The source device to copy presentation from. If not specified all devices are copied.")]
    [Optional]
    public string SourceDeviceName { get; set; }

    [NamedParameter("td", "targetdevice")]
    [Description("The target device to copy presentation to.")]
    [Optional]
    public string TargetDeviceName { get; set; }

    [NumberedParameter(0, "targetitem")]
    [Description("The path of the target item to copy presentation to. If not specified the current item is used.")]
    [Optional]
    public string TargetPath { get; set; }

    [NumberedParameter(1, "path")]
    [Description("The path of the source item to copy presentation from. If not specified the current item is used.")]
    [Optional]
    public string SourcePath { get; set; }

    public override CommandResult Run()
    {
      // Resolve device names into device objects
      DeviceItem sourceDevice = null;
      DeviceItem targetDevice = null;

      if (!string.IsNullOrEmpty(SourceDeviceName) || !string.IsNullOrEmpty(TargetDeviceName))
      {
        if (string.IsNullOrEmpty(SourceDeviceName) && string.IsNullOrEmpty(TargetDeviceName))
          return new CommandResult(CommandStatus.Failure, "If either source or target device is specified the other must be as well.");
        else
        {
          var devices = Context.CurrentDatabase.Resources.Devices.GetAll();

          foreach (var device in devices)
          {
            if(ID.IsID(SourceDeviceName) && device.ID == ID.Parse(SourceDeviceName))
              sourceDevice = device;
            else if (string.Compare(device.Name, SourceDeviceName, true) == 0)
              sourceDevice = device;

            if (ID.IsID(TargetDeviceName) && device.ID == ID.Parse(TargetDeviceName))
              targetDevice = device;
            else if (string.Compare(device.Name, TargetDeviceName, true) == 0)
              targetDevice = device;
          }

          if (sourceDevice == null)
            return new CommandResult(CommandStatus.Failure, "Failed to find source device '" + SourceDeviceName + "'");

          if (targetDevice == null)
            return new CommandResult(CommandStatus.Failure, "Failed to find target device '" + TargetDeviceName + "'");
        }
      }

      var source = Context.CurrentItem;

      using (var sourcecs = new ContextSwitcher(Context, SourcePath))
      {
        if (sourcecs.Result.Status != CommandStatus.Success)
          return sourcecs.Result;

        source = Context.CurrentItem;
      } 

      using (var targetcs = new ContextSwitcher(Context, TargetPath))
      {
        if (targetcs.Result.Status != CommandStatus.Success)
          return targetcs.Result;

        var target = Context.CurrentItem;
        target = target.Versions.AddVersion();

        if (sourceDevice != null && targetDevice != null)
        {
          // get the layout from the current item for the source device
          var sldf = LayoutField.GetFieldValue(source.Fields[Sitecore.FieldIDs.LayoutField]);
          var sld = string.IsNullOrEmpty(sldf) ? new LayoutDefinition() : Sitecore.Layouts.LayoutDefinition.Parse(sldf);
          var sdd = sld.GetDevice(sourceDevice.ID.ToString());

          // todo: verify this works with layout deltas
          var tldf = LayoutField.GetFieldValue(target.Fields[Sitecore.FieldIDs.LayoutField]);
          var tld = string.IsNullOrEmpty(tldf) ? new LayoutDefinition() : Sitecore.Layouts.LayoutDefinition.Parse(tldf);
          var tdd = tld.GetDevice(targetDevice.ID.ToString());

          // Copy the layout
          tdd.Layout = sdd.Layout;

          // Copy the renderings
          if (tdd.Renderings != null)
            tdd.Renderings.Clear();

          foreach (var rendering in sdd.Renderings ?? new ArrayList())
          {
            tdd.AddRendering((Sitecore.Layouts.RenderingDefinition)rendering);
          }

          target.Editing.BeginEdit();
          target[Sitecore.FieldIDs.LayoutField] = tld.ToXml();
          target.Editing.EndEdit();
        }
        else
        {
          // Copy entire layout from source item to target item
          target.Editing.BeginEdit();
          target[Sitecore.FieldIDs.LayoutField] = source[Sitecore.FieldIDs.LayoutField];
          target.Editing.EndEdit();
        }
      }

      return new CommandResult(CommandStatus.Success, "Presentation copied");
    }

    public override string Description()
    {
      return "Copy presentation between items and devices";
    }

    public override void Help(HelpDetails details)
    {
      var comments = new StringBuilder();
      Formatter.PrintLine("If only a single parameter is supplied it is treated as the targetitem parameter. The first example below has the effect of copying the layout from the current item to the item given as the parameter.", comments);
      comments.Append("Both source and target device parameters must be used if either is used.");

      details.Comments = comments.ToString();
      details.AddExample("item2");
      details.AddExample("item1 ../item2");
      details.AddExample("-sd default -td printer");
      details.AddExample("-sd default -td printer item2");
    }
  }
}
