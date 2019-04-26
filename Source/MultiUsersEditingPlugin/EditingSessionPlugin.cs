using System.Linq;
using System.Net;
using System.Reflection;
using FlaxEditor;
using FlaxEditor.GUI;
using FlaxEngine;
using FlaxEngine.GUI;

namespace MultiUsersEditingPlugin
{
    public class EditingSessionPlugin : EditorPlugin
    {
        public EditingSession Session;

        private ContextMenuButton _collaborateButton;
        private Label _labelConnected;

        public override void InitializeEditor()
        {
            base.InitializeEditor();
            Instance = this;
            _collaborateButton = Editor.UI.MainMenu.GetButton("Window").ContextMenu.AddButton("Collaborate");
            _collaborateButton.Clicked += () => { new CollaborateWindow().Show();};
            
            _labelConnected = Editor.UI.StatusBar.AddChild<Label>();
            _labelConnected.X = Editor.UI.StatusBar.Width - Editor.UI.StatusBar.Width * 0.3f;
            _labelConnected.DockStyle = DockStyle.None;
            _labelConnected.OnChildControlResized += (control) => { _labelConnected.X = Editor.UI.StatusBar.Width - Editor.UI.StatusBar.Width * 0.3f;};
            _labelConnected.Text = "Disconnected";

            Editor.Undo.ActionDone += (IUndoAction action) =>
            {            
                if (Session == null)
                    return;

                if (action as SelectionChangeAction != null)
                {
                    
                }
                else
                {
                    Packet p = new GenericUndoActionPacket(action);
                    Session.SendPacket(p);
                }
            };
        }

        public override void Deinitialize()
        {
            Session?.Close();
            Session = null;
            _collaborateButton.Dispose();
            _labelConnected.Dispose();
            base.Deinitialize();
        }

        private static EditingSessionPlugin _instance;

        public static EditingSessionPlugin Instance
        {
            get
            {
                return _instance;
            }

            protected set { _instance = value; }
        }
    }
}