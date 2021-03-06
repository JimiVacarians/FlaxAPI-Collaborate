using System;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.GUI;
using FlaxEditor.Windows;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Utilities;

namespace CollaboratePlugin
{
    public class CollaborateWindow : CustomEditorWindow
    {
        private SessionSettings _clientSettings = new SessionSettings();
        private SessionSettings _serverSettings = new ServerSessionSettings();
        private LayoutElementsContainer _layout;

        public override void Initialize(LayoutElementsContainer layout)
        {
            _layout = layout;
            Rebuild();
        }

        private void ShowJoin()
        {
            _layout.ContainerControl.DisposeChildren();
            //_layout.ContainerControl.AnchorStyle = AnchorStyle.Upper;
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.Join;
            _layout.ClearLayout();
            _layout.Space(5);
            var label = _layout.Label("Joining Session", TextAlignment.Center);
            var fontReference = label.Label.Font;
            fontReference.Size = 11;
            label.Label.Font = fontReference;
            _layout.Space(5);

            var clientSettingsEditor = new CustomEditorPresenter(null);
            clientSettingsEditor.Panel.Parent = _layout.ContainerControl;
            clientSettingsEditor.Select(_clientSettings);

            var button = _layout.Button("Join");
            var returnButton = _layout.Button("Return");

            button.Button.Clicked += async () =>
            {
                button.Button.Enabled = false;
                var session = new ClientSession();
                bool connected = await session.Start(_clientSettings);
                button.Button.Enabled = true;
                if (connected)
                {
                    EditingSessionPlugin.Instance.Session = session;
                    ShowSession();
                }
            };

            returnButton.Button.Clicked += () => ShowNoSession();
        }

        private void ShowHost()
        {
            _layout.ContainerControl.DisposeChildren();
            //_layout.ContainerControl.AnchorStyle = AnchorStyle.Upper;
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.Host;
            _layout.ClearLayout();
            _layout.Space(5);
            var label = _layout.Label("Hosting Session", TextAlignment.Center);
            var fontReference = label.Label.Font;
            fontReference.Size = 11;
            label.Label.Font = fontReference;
            _layout.Space(5);

            var serverSettingsEditor = new CustomEditorPresenter(null);
            serverSettingsEditor.Panel.Parent = _layout.ContainerControl;
            serverSettingsEditor.Select(_serverSettings);

            var button = _layout.Button("Host");
            var returnButton = _layout.Button("Return");

            button.Button.Clicked += async () =>
            {
                button.Button.Enabled = false;
                var session = new ServerSession();
                bool connected = await session.Start(_serverSettings);
                button.Button.Enabled = true;
                if (connected)
                {
                    EditingSessionPlugin.Instance.Session = session;
                    ShowSession();
                }
            };

            returnButton.Button.Clicked += () =>
            {
                ShowNoSession();
            };
        }

        private void ShowSession()
        {

            var tableHeaderColor = new Color(60, 60, 60);

            _layout.ContainerControl.DisposeChildren();
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.Session;

            var vpanel = _layout.ContainerControl.AddChild<VerticalPanel>();

            vpanel.Width = _layout.ContainerControl.Width;

            var userDropPanel = vpanel.AddChild<DropPanel>();
            userDropPanel.HeaderText = "Users List";
            userDropPanel.EnableDropDownIcon = true;
            userDropPanel.Width = vpanel.Width;

            var userTable = userDropPanel.AddChild<Table>();
            userTable.Width = userDropPanel.Width;

            var nameDef = new ColumnDefinition();
            nameDef.Title = "Display Name";
            nameDef.CellAlignment = TextAlignment.Near;
            nameDef.TitleBackgroundColor = tableHeaderColor;

            var actionDef = new ColumnDefinition();
            actionDef.Title = "Actions";
            actionDef.CellAlignment = TextAlignment.Near;
            actionDef.TitleBackgroundColor = tableHeaderColor;

            userTable.Columns = new[] { nameDef, actionDef };

            EditingSessionPlugin.Instance.Session.Users.ForEach((user) =>
            {
                var name = user.Name;

                if (user.IsServer)
                    name += " (Server)";

                if (user.Id == EditingSessionPlugin.Instance.Session.User.Id)
                    name += " (You)";

                var row = new Row()
                {
                    Values = new object[]
                    {
                          user.Name,
                          "No actions"
                    },
                    Parent = userTable,

                };
            });

            var history = vpanel.AddChild<DropPanel>();
            history.HeaderText = "History";
            history.EnableDropDownIcon = true;

            var settings = vpanel.AddChild<DropPanel>();
            settings.HeaderText = "Settings";
            settings.EnableDropDownIcon = true;

            var p = vpanel.AddChild<Panel>();
            p.BackgroundColor = Color.Transparent;
            p.Height = 10;

            var disconnectButton = vpanel.AddChild<Button>();
            disconnectButton.Text = "Disconnect";
            disconnectButton.Clicked += () =>
            {
                EditingSessionPlugin.Instance.Session.Close();
                ShowNoSession();
            };

            userDropPanel.PerformLayout(true);
            _layout.ContainerControl.PerformLayout(true);
        }

        private void ShowNoSession()
        {
            _layout.ContainerControl.DisposeChildren();
            EditingSessionPlugin.Instance.SessionState = EditingSessionPlugin.State.NoSession;

            var p = new Panel()
            {
                Parent = _layout.ContainerControl,
                AnchorPreset = AnchorPresets.StretchAll
            };
            var hp = new HorizontalPanel()
            {
                Parent = p,
                AnchorPreset = AnchorPresets.MiddleCenter,
                Height = 70,
                Spacing = 15
            };
            
            var joinButton = new Button()
            {
                Parent = hp,
                Text = "   Join   ",
                Width = 120,
                AnchorPreset = AnchorPresets.MiddleCenter
            };
            joinButton.Clicked += () => ShowJoin();
            var hostButton = new Button()
            {
                Parent = hp,
                Text = "   Host   ",
                Width = 120,
                AnchorPreset = AnchorPresets.MiddleCenter
            };
            hostButton.Clicked += () => ShowHost();
            hp.Location -= hp.Size / 2;
        }

        public void Rebuild()
        {
            if (Window != null)
                Window.Title = "Collaborate";
            switch (EditingSessionPlugin.Instance.SessionState)
            {
                case EditingSessionPlugin.State.Join:
                    ShowJoin();
                    break;

                case EditingSessionPlugin.State.Host:
                    ShowHost();
                    break;

                case EditingSessionPlugin.State.Session:
                    ShowSession();
                    break;

                case EditingSessionPlugin.State.NoSession:
                    ShowNoSession();
                    break;
            }
        }
    }
}