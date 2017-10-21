using System;
using System.Collections.Generic;
using System.Linq;

namespace Revolver.Core.Commands
{
  [Command("filler")]
  public class Filler : BaseCommand
  {
    private readonly string[] _loremParagraphs =
    {
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis ligula massa, varius non condimentum sed, accumsan in nibh. Interdum et malesuada fames ac ante ipsum primis in faucibus. Cras finibus, ex vel egestas blandit, purus sem efficitur leo, at fermentum neque purus id neque. Aliquam augue sapien, accumsan at mattis eu, fermentum ut neque. Donec nulla felis, maximus vitae suscipit id, pulvinar in leo. Mauris finibus ac purus commodo viverra. Vivamus eget venenatis quam, sit amet bibendum mi. Donec vel erat nec nulla tempor gravida ut at nulla. Nunc eget libero vitae felis sollicitudin lacinia sit amet ut ipsum. Suspendisse potenti. Donec fermentum elementum cursus. Morbi in sapien purus. Maecenas vulputate dapibus purus ac pellentesque.",
      "Ut finibus sapien dui. Mauris tincidunt blandit eros, et condimentum ante auctor sit amet. In vitae ipsum eu mi venenatis elementum non vitae eros. Aliquam non libero justo. Sed tristique hendrerit velit a sagittis. Proin rutrum laoreet nibh ut ornare. Fusce pellentesque nunc justo, ac egestas nibh tristique in.",
      "Duis bibendum diam augue, non egestas nunc egestas vitae. Nunc ultrices ultrices neque quis eleifend. Sed condimentum turpis quis ante ultricies, fringilla hendrerit dolor fringilla. Ut quis ornare nulla, sed congue lacus. Maecenas augue sapien, malesuada a pretium at, imperdiet finibus justo. Nullam nec metus nulla. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.",
      "Cras eros massa, elementum consequat purus et, ullamcorper pharetra sapien. Sed pulvinar diam a urna convallis, finibus bibendum quam elementum. Suspendisse potenti. Interdum et malesuada fames ac ante ipsum primis in faucibus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Morbi justo elit, viverra ut justo quis, consequat posuere tellus. In ultrices tempus mi. Donec nibh lacus, commodo nec elit non, scelerisque malesuada arcu. Fusce semper neque ut nulla facilisis efficitur. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. In malesuada elit maximus tincidunt placerat. Integer ex orci, laoreet et suscipit eu, maximus ut libero. Nulla non justo quam.",
      "Cras molestie felis id diam sollicitudin, nec pulvinar magna sagittis. Vivamus id ipsum mi. Morbi ullamcorper dolor non sem dapibus, vitae semper tortor sollicitudin. Duis at fermentum enim. Curabitur et turpis ligula. Proin non fermentum arcu. Vivamus at sollicitudin mi. Sed pretium, magna vel porttitor dictum, velit purus vulputate nulla, commodo congue nibh libero ac mauris. Nulla ac lobortis diam. Aliquam in lorem sed nisi ultricies lobortis. Sed tincidunt dignissim fringilla. Aenean quis tortor nunc. Nulla ut feugiat neque.",
      "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Mauris facilisis mollis libero, eget facilisis erat varius id. Sed blandit mi et metus malesuada, ut mollis ligula consequat. Nulla facilisis ipsum in consequat vulputate. Quisque sollicitudin malesuada euismod. In faucibus viverra sodales. Praesent aliquam justo et sem imperdiet, ac bibendum ante vestibulum. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.",
      "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Proin at est et enim placerat egestas non tempor purus. Suspendisse ipsum neque, aliquet vitae laoreet eu, scelerisque vel dui. Pellentesque in odio massa. Sed tempor ornare eros finibus ornare. Sed facilisis auctor augue, eu congue justo dignissim non. Curabitur egestas neque eget tortor mattis, sed porta sem consequat. Cras quis sollicitudin lacus, sit amet faucibus justo.",
      "Sed tempor erat sed faucibus convallis. Nulla porta nulla quis augue pulvinar, in blandit ex sagittis. Donec sit amet justo fringilla, blandit arcu a, condimentum leo. Integer a nisi in sapien lobortis hendrerit ac non sem. Nullam egestas egestas vestibulum. Aliquam varius neque a nisl euismod bibendum. Nullam quam sapien, varius id leo ut, tincidunt tincidunt libero.",
      "Suspendisse non elit vel massa accumsan pretium in sed turpis. Ut pharetra bibendum laoreet. Curabitur quam turpis, tempor eu tortor ut, rutrum porta est. Ut molestie pulvinar leo, eget fermentum magna aliquam non. Integer interdum mollis sollicitudin. Aenean at libero eget tellus volutpat interdum id eget lorem. Donec eleifend efficitur purus ut pulvinar. Nullam sed tellus eget mi efficitur sodales quis in leo. Cras id tincidunt tellus, sed mollis felis. Vivamus ac metus id erat rutrum ultrices sit amet et turpis.",
      "Donec congue erat non justo molestie suscipit. Ut hendrerit mollis urna, et ultrices dui cursus vel. Aenean quis tempor mi, in sodales erat. Vivamus lobortis tellus ligula, sed condimentum dui venenatis sit amet. Praesent posuere neque et pulvinar tincidunt. Nam hendrerit ante quam, in consectetur eros aliquet id. Cras cursus lorem scelerisque lectus feugiat, eu tempus urna sollicitudin. Fusce id risus faucibus, tempus tellus vel, fringilla dui. Suspendisse potenti. Donec mollis ac libero at aliquam. Mauris euismod ligula non pharetra facilisis. Suspendisse vel dolor turpis. Aliquam ultricies, ipsum nec suscipit congue, justo turpis gravida ex, id sagittis nisi purus eget mi. Nullam cursus nulla id justo scelerisque facilisis. Mauris ut neque ac elit sollicitudin scelerisque sed sit amet eros. Sed tristique nisl ipsum, et tempus lorem ullamcorper nec.",
      "Nunc tincidunt volutpat dictum. Quisque eget nisl nec risus viverra hendrerit sed in sapien. Ut fermentum quis massa finibus placerat. Praesent vel justo ante. Aliquam augue ligula, condimentum id condimentum eu, sollicitudin vel quam. Curabitur ligula lectus, elementum non dui eleifend, varius maximus nulla. Nullam tristique tristique ultricies. Ut accumsan risus nec erat varius, sit amet volutpat eros luctus. Curabitur porttitor faucibus mauris, ac luctus purus convallis sed. Phasellus tortor elit, vestibulum id mauris at, sagittis bibendum tortor. Etiam sed quam vitae magna aliquam bibendum. Integer ultrices tincidunt est, in pharetra odio porta ac. Aenean erat ipsum, tincidunt laoreet sagittis non, faucibus in velit. In eget hendrerit magna, at maximus purus. Duis id pulvinar mauris. Vestibulum dignissim urna neque, id finibus magna aliquam eget.",
      "Suspendisse neque risus, egestas in lobortis vel, aliquet non massa. Sed non egestas ante, quis rutrum eros. Maecenas id commodo nibh. Proin sagittis auctor mi vitae condimentum. Sed sed lectus interdum, tincidunt augue ut, volutpat justo. Morbi neque odio, facilisis in tincidunt non, blandit ac velit. Integer in est sed urna fermentum aliquet ac sit amet sem. Phasellus euismod ornare dignissim. Vestibulum a quam sed lorem malesuada ornare. Proin vitae magna sed mi consectetur pulvinar. Donec sed urna urna. Vestibulum ultrices libero nec est placerat mollis.",
      "Suspendisse in odio eleifend, ultrices mi id, elementum tellus. Aliquam dignissim nisl gravida aliquam finibus. Praesent dictum massa odio, et egestas justo ullamcorper at. Nullam malesuada sodales fringilla. Nulla pellentesque gravida ante, sit amet congue elit hendrerit vel. Sed nec ultrices velit, non rhoncus risus. Sed quis nulla metus. Phasellus elit velit, auctor a diam in, ullamcorper pellentesque justo. Cras congue leo odio, sit amet lacinia purus fermentum a. Fusce hendrerit ipsum enim, a tincidunt arcu semper sed. Suspendisse magna lacus, molestie ut gravida vel, tempor et neque. Donec rhoncus faucibus condimentum.",
      "Nulla et lacus ullamcorper, fringilla nisl vitae, gravida arcu. Suspendisse potenti. Donec luctus ac augue ac porta. Etiam dolor mi, tempus eget nibh ac, pulvinar vulputate mauris. Pellentesque luctus non tortor sit amet maximus. Aliquam dapibus sit amet dui ut feugiat. Fusce ut hendrerit arcu. Vestibulum consectetur ipsum tortor, eu auctor orci imperdiet non. Donec finibus mollis efficitur. Nunc pellentesque vestibulum pharetra. Quisque sit amet leo tempus, condimentum libero id, sagittis mi. Etiam non orci placerat, commodo lacus nec, interdum tortor. Donec dapibus nunc at metus aliquet hendrerit. Nam et eleifend ipsum.",
      "Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Curabitur ut tempor augue. Morbi ac nisi ligula. Integer feugiat faucibus elit sit amet ultrices. Nam enim urna, elementum vitae vehicula nec, interdum sed felis. Nulla mattis libero nec porttitor luctus. Proin nisl odio, faucibus et accumsan at, vestibulum quis lacus. In lectus nibh, laoreet id nulla porta, euismod suscipit ligula. Ut sodales, odio aliquam eleifend vestibulum, erat dui imperdiet mauris, in porttitor massa erat et diam. Etiam id nulla lacinia, volutpat ex non, lacinia lectus. Donec pellentesque sed ligula vel suscipit. Cras est turpis, varius sagittis vehicula eu, convallis quis purus. Donec molestie ullamcorper massa at rutrum.",
      "Morbi luctus erat ante, eu mollis nisl eleifend ut. Nulla scelerisque sit amet sem eget pulvinar. Nunc nec iaculis tortor, eu dignissim augue. Etiam sodales tellus id justo efficitur vestibulum. Cras et varius leo, a tempus nunc. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Suspendisse tempor non libero in placerat. Donec vel iaculis magna, sed dapibus metus. Phasellus velit erat, sagittis quis tortor non, facilisis fringilla ante. Quisque ac venenatis diam, sit amet congue leo. Nam dignissim vehicula sapien, id lobortis lectus fermentum at. Morbi odio lacus, vulputate eu felis et, volutpat posuere eros.",
      "Quisque imperdiet, est sed ultrices tristique, sem velit auctor ipsum, non sodales odio urna non ex. Etiam dictum bibendum sapien ac varius. Cras non euismod magna, quis lacinia nisi. Nullam lacus lectus, molestie fermentum mauris ac, molestie finibus nibh. In porta odio quis dolor gravida, nec volutpat enim suscipit. Cras aliquam sapien eu dui molestie, non efficitur urna egestas. Praesent in metus in sapien consectetur aliquam nec id ante. Morbi rutrum vel lectus vel euismod. Duis iaculis nisi in justo blandit, in ullamcorper dui ultricies. Maecenas ac odio vitae lectus porta tempor. Mauris vel vehicula lorem, vel luctus erat. Vivamus gravida elementum massa, at venenatis urna molestie sit amet.",
      "Vivamus tincidunt velit eget neque luctus, eu consectetur quam mattis. Nulla placerat eleifend venenatis. Aliquam erat volutpat. Aliquam non rhoncus metus, ut dapibus risus. Praesent ut tincidunt tellus. Sed vel molestie est. Mauris dignissim elementum libero sit amet efficitur. Praesent et ligula eu libero feugiat lacinia at non mauris. Fusce cursus vel lectus et sodales. Nam sed lectus aliquet, egestas risus sed, molestie enim. Donec lobortis ex a ultricies pulvinar. Nunc ut accumsan orci, a tempor felis. Integer non posuere dui, vel tempor tellus. Nulla tempor elementum neque, nec faucibus dui blandit quis.",
      "Integer lacinia ipsum in diam vestibulum bibendum. Proin sit amet euismod turpis, eget aliquam tortor. Cras eget orci finibus, pharetra augue et, tincidunt lorem. Aliquam vulputate risus nec est posuere, in cursus nulla convallis. Donec vestibulum posuere risus ac dictum. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Morbi vestibulum, massa non hendrerit consectetur, felis ipsum mollis mi, vitae tempor tellus turpis at quam. Integer neque sem, tincidunt vitae rutrum eu, tempor at purus.",
      "Aenean in tellus tellus. Praesent venenatis nisl ut arcu euismod, in faucibus tellus semper. Suspendisse finibus fringilla tempus. Etiam molestie nibh interdum tincidunt bibendum. Nullam vestibulum nisi mauris, quis convallis enim finibus sed. Aenean vitae lacus nec nunc interdum imperdiet. Vivamus imperdiet elit at massa efficitur malesuada in eu leo. Integer vitae turpis semper, ornare augue non, finibus eros. Etiam libero elit, rhoncus id euismod quis, commodo sed enim. In hac habitasse platea dictumst."
    };

    private readonly System.Random _random = new System.Random();

    [NumberedParameter(0, "Lower Limit")]
    [Description("The lower limit of the number of objects to generate.")]
    [Optional]
    public int FirstLimit { get; set; }

    [NumberedParameter(1, "Upper Limit")]
    [Description("The upper limit of the number of objects to generate.")]
    [Optional]
    public int SecondLimit { get; set; }

    [FlagParameter("s")]
    [Description("Generate sentences instead of words.")]
    [Optional]
    public bool Sentences { get; set; }

    [FlagParameter("p")]
    [Description("Generate paragraphs instead of words.")]
    [Optional]
    public bool Paragraphs { get; set; }

    [NamedParameter("pd")]
    [Description("The paragraph delimiter to use for generating paragraphs.")]
    [Optional]
    public string ParagraphDelimiter { get; set; }

    public Filler()
    {
      ParagraphDelimiter = Environment.NewLine;
    }

    public override CommandResult Run()
    {
      if(Sentences && Paragraphs)
        return new CommandResult(CommandStatus.Failure, "Only one of -s or -p can be used at once.");

      var lower = SecondLimit == 0 ? 5 : FirstLimit;
      var upper = SecondLimit == 0 ? FirstLimit : SecondLimit;

      if (upper == 0)
        upper = 20;

      var count = _random.Next(lower, upper);
      var output = string.Empty;

      if (Sentences)
        output = GenerateSentences(count);
      else if(Paragraphs)
        output = GenerateParagraphs(count);
      else
        output = GenerateWords(count);

      return new CommandResult(CommandStatus.Success, output);
    }

    private string GenerateWords(int count)
    {
      var output = new List<string>();
      while (output.Count < count)
      {
        var paragraph = _random.Next(_loremParagraphs.Length);
        var offset = _random.Next(10);
        var paragraphWords = _loremParagraphs[paragraph].Split(' ');
        var wordsToTake = paragraphWords.Skip(offset).ToArray();

        // Capitolize the start word
        var capitolFirstLetter = char.ToUpper(wordsToTake[0][0]);
        output.Add(capitolFirstLetter + wordsToTake[0].Remove(0, 1));

        output.AddRange(wordsToTake.Skip(1).Take(count - 2));

        var lastWord = wordsToTake.Last();
        if (!lastWord.EndsWith("."))
          lastWord = lastWord + ".";

        output.Add(lastWord);
      }

      return string.Join(" ", output);
    }

    private string GenerateParagraphs(int count)
    {
      var output = new List<string>();
      while (output.Count < count)
      {
        var paragraph = _random.Next(_loremParagraphs.Length);
        var paragraphToTake = _loremParagraphs[paragraph];
        output.Add(paragraphToTake);
      }

      return string.Join(ParagraphDelimiter, output);
    }

    private string GenerateSentences(int count)
    {
      var output = new List<string>();
      while (output.Count < count)
      {
        var paragraph = _random.Next(_loremParagraphs.Length);
        var offset = _random.Next(10);
        var paragraphWords = _loremParagraphs[paragraph].Split('.');
        var wordsToTake = paragraphWords.Skip(offset).ToArray();

        // Capitolize the start word
        var capitolFirstLetter = char.ToUpper(wordsToTake[0][0]);
        output.Add(capitolFirstLetter + wordsToTake[0].Remove(0, 1));

        output.AddRange(wordsToTake.Skip(1).Take(count - 1));
      }

      return string.Join(". ", output) + ".";
    }

    public override string Description()
    {
      return "Generates filler content.";
    }

    public override void Help(HelpDetails details)
    {
      details.Comments = Formatter.JoinLines(new[]
      {
        "Only one of -s or -p can be used at once.",
        "If only one limit is specified, it is the upper limit.",
      });

      details.AddExample("");
      details.AddExample("20");
      details.AddExample("10 20");
      details.AddExample("-s 1 3");
      details.AddExample("-p 1 2");
    }
  }
}