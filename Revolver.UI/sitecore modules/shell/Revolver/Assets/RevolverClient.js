var Revolver = {

  // Member variables
  cmdStack: new Array(),
  cmdInd: 0,
  outputbuffer: 10000000,
  inputDefaultHeight: "20px",
  inputField: null,
  outputField: null,

  keyPress: function (e) {
    if (e.keyCode === 13) {
      if (e.shiftKey) //allows you to create a breakline
        return true;

      var command = Revolver.inputField.value;

      if (command === "clear") {
        Revolver.inputField.value = "";
        Revolver.outputField.innerHTML = "";
        return false;
      }

      Revolver.cmdStack.push(command);
      Revolver.execute(command);
      Revolver.cmdInd = 0;
      return false;
    }

    if (e.keyCode === 38) { //presses the up arrow to scroll command cache
      Revolver.cmdInd++;
      if (Revolver.cmdInd >= Revolver.cmdStack.length)
        Revolver.cmdInd = Revolver.cmdStack.length;
      Revolver.useCachedCommand();

      return false;
    }

    if (e.keyCode === 40) { //presses the down arrow to scroll command cache
      Revolver.cmdInd--;
      if (Revolver.cmdInd < 0)
        Revolver.cmdInd = 0;
      Revolver.useCachedCommand();

      return false;
    }

    return true;
  },

  useCachedCommand: function () {
    if (Revolver.cmdInd === 0)
      Revolver.inputField.value = "";
    else
      Revolver.inputField.value = Revolver.cmdStack[Revolver.cmdStack.length - Revolver.cmdInd];
  },

  execute: function (command) {
    var status = $("status");
    status.innerHTML = "communicating with server";

    var request = new window.scRequest();
    Revolver.inputField.disabled = true;

    var prepCommand = encodeURIComponent(command);

    // ensure the parenthesis are also encoded to the Sitecore command handler doesn't choke when trying to parse the command
    prepCommand = prepCommand.replace(/\(/g, "%28").replace(/\)/g, "%29");

    var parameters = "sessionId=" + $("hdnSessionId").value + "&command=" + prepCommand;

    if ($("__CSRFTOKEN"))
      request.form = "__CSRFTOKEN=" + $("__CSRFTOKEN").value;

    request.build("", "", "", "r:" + parameters, true, Revolver.contextmenu, Revolver.modified);

    request.callback = function (response) {
      status.innerHTML = "awaiting input";

      var result;

      if (typeof (JSON) != "undefined")
        result = JSON.parse(response);
      else
        result = response.evalJSON(true);

      if (result.exit != null && result.exit) {
        parent.scWin.closeWindow();
        return;
      }

      if (result.prompt != null) {
        $("prompt").innerHTML = result.prompt;

        var promptOutput = document.createElement("div");
        promptOutput.innerHTML = "<pre>\r\n\r\n" + result.prompt + "</pre>";
        Revolver.outputField.appendChild(promptOutput);
      }

      // todo, limit the output buffer size
      if (result.outputBuffer != null)
        Revolver.outputbuffer = parseInt(result.outputBuffer);

      var commandOutput = document.createElement("div");
      commandOutput.innerHTML = "<pre>" + command.escapeHTML() + "</pre>";
      Revolver.outputField.appendChild(commandOutput);

      var outel = document.createElement("div");
      Element.extend(outel);

      var content = "";
      var cssClass = "";

      if (result.error) {
        cssClass = "error";          
        content = result.error.escapeHTML();
      }
      else {
        cssClass = "output";

        if (result.output)
          content = result.output.escapeHTML();
      }

      outel.addClassName(cssClass);
      outel.innerHTML = "<pre>\r\n\r\n" + content + "</pre>";

      Revolver.outputField.appendChild(outel);
      outel.scrollIntoView(false);

      Revolver.inputField.value = "";
      Revolver.inputField.disabled = false;
      Revolver.checkInputDimension();
      Revolver.inputField.focus();
    };

    request.async = true;
    request.execute();
  },

  checkInputDimension: function () {
    if (Revolver.inputField.value === "")
      Revolver.inputField.style.height = Revolver.inputDefaultHeight;
    else if (Revolver.inputField.scrollHeight !== Revolver.inputField.clientHeight)
      Revolver.inputField.style.height = Revolver.inputField.scrollHeight + "px";

    Revolver.outputField.lastChild.scrollIntoView(false);
  },

  setupCursorFocus: function () {
    $("scrollbox").observe("click", Revolver.setFocusToInputIfNoSelectedText);
    $("prompt").observe("click", Revolver.setFocusToInputIfNoSelectedText);
    Revolver.inputField.observe("click", Revolver.setFocusToInputIfNoSelectedText);
    $("statusBorder").observe("click", Revolver.setFocusToInputIfNoSelectedText);
  },

  setFocusToInputIfNoSelectedText: function () {
    if (window.getSelection().toString().length === 0) {
        Revolver.inputField.focus();
    }
  },

  bindFields: function() {
    Revolver.inputField = $("input");
    if (!Revolver.inputField)
      throw new Error("Failed to find input field on page");

    Revolver.outputField = $("output");
    if (!Revolver.outputField)
      throw new Error("Failed to find output field on page");
  }
};

document.observe("dom:loaded", function () {
  Revolver.bindFields();

  Revolver.inputDefaultHeight = Revolver.inputField.style.height;

  Revolver.checkInputDimension();
  Revolver.setupCursorFocus();

  // Fix percentage height issue for output box in FF
  if (navigator.userAgent.indexOf("Gecko") > -1) {
    Revolver.outputField.style.height = "200px";
  }

  Revolver.setFocusToInputIfNoSelectedText();
});

scIEOnSelectStart = function () {
  return;
}