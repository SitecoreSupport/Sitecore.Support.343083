using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Support.Shell.Applications.ContentEditor
{
    public class Image : Sitecore.Shell.Applications.ContentEditor.Image
    {
        protected new void EditImage(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            string attribute = this.XmlValue.GetAttribute("mediaid");
            if (string.IsNullOrEmpty(attribute))
            {
                SheerResponse.Alert("Select an image from the Media Library first.");
            }
            else
            {
                if (args.IsPostBack)
                {
                    if (args.Result != "yes")
                    {
                        args.AbortPipeline();
                        return;
                    }
                }
                else
                {
                    Item obj = Client.ContentDatabase.GetItem(attribute, Language.Parse(this.ItemLanguage));
                    if (obj == null)
                        return;
                    ItemLink[] referrers = Globals.LinkDatabase.GetReferrers(obj);
                    if (referrers != null && referrers.Length > 1)
                    {
                        SheerResponse.Confirm(string.Format(
                          "This media item is referenced by {0} other items.\n\nEditing the media item will change it for all the referencing items.\n\nAre you sure you want to continue?",
                          (object)referrers.Length));
                        args.WaitForPostBack();
                        return;
                    }
                }

                Item obj1 = Client.ContentDatabase.GetItem(attribute, Language.Parse(this.ItemLanguage));
                if (obj1 == null)
                    Sitecore.Shell.Framework.Windows.RunApplication("Media/Imager", "id=" + attribute + "&la=" + this.ItemLanguage);
                string str = "webdav:compositeedit";
                Command command = CommandManager.GetCommand(str);
                if (command == null)
                {
                    SheerResponse.Alert(Translate.Text("Edit command not found."));
                }
                else
                {
                    switch (CommandManager.QueryState(str, obj1))
                    {
                        case CommandState.Disabled:
                        case CommandState.Hidden:
                            Sitecore.Shell.Framework.Windows.RunApplication("Media/Imager", "id=" + attribute + "&la=" + this.ItemLanguage);
                            break;
                    }

                    command.Execute(new CommandContext(obj1));
                }
            }
        }
    }
}