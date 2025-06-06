using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        VisualElement rootVisualElement;
        Button hostButton;
        Button clientButton;
        Label statusLabel;

        public GameObject wayPoints;
        public GameObject enemies;
        public GameObject prefabghost;
        public GameObject prefabgargoyle;
        public GameObject prefabBoss;
        [SerializeField] bool testingBoss;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            rootVisualElement = uiDocument.rootVisualElement;

            hostButton = CreateButton("HostButton", "Host");
            clientButton = CreateButton("ClientButton", "Client");
            statusLabel = CreateLabel("StatusLabel", "Not Connected");

            rootVisualElement.Clear();
            rootVisualElement.Add(hostButton);
            rootVisualElement.Add(clientButton);
            rootVisualElement.Add(statusLabel);

            hostButton.clicked += OnHostButtonClicked;
            clientButton.clicked += OnClientButtonClicked;
        }

        void Update()
        {
            UpdateUI();
        }

        void OnDisable()
        {
            hostButton.clicked -= OnHostButtonClicked;
            clientButton.clicked -= OnClientButtonClicked;
        }

        void OnHostButtonClicked()
        {
            NetworkManager.Singleton.StartHost();
            SpawnEnemy();
        }

        void OnClientButtonClicked() => NetworkManager.Singleton.StartClient();

        // Disclaimer: This is not the recommended way to create and stylize the UI elements, it is only utilized for the sake of simplicity.
        // The recommended way is to use UXML and USS. Please see this link for more information: https://docs.unity3d.com/Manual/UIE-USS.html
        private Button CreateButton(string name, string text)
        {
            var button = new Button();
            button.name = name;
            button.text = text;
            button.style.width = 240;
            button.style.backgroundColor = Color.white;
            button.style.color = Color.black;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            return button;
        }

        private Label CreateLabel(string name, string content)
        {
            var label = new Label();
            label.name = name;
            label.text = content;
            label.style.color = Color.black;
            label.style.fontSize = 18;
            return label;
        }

        void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
                SetStartButtons(false);
                SetStatusText("NetworkManager not found");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                SetStartButtons(true);
                SetStatusText("Not connected");
            }
            else
            {
                SetStartButtons(false);
                UpdateStatusLabels();
            }
        }

        void SetStartButtons(bool state)
        {
            hostButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void SetStatusText(string text) => statusLabel.text = text;

        void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText = "Mode: " + mode;
            SetStatusText($"{transport}\n{modeText}");
        }

        void SpawnEnemy()
        {
            //Positions taken from the existing ghosts
            Vector3[] positionArray = new[] { new Vector3(-5.3f, 0f, -3.1f),
                                            new Vector3(1.5f, 0f, 4f),
                                            new Vector3(3.2f, 0f, 6.5f),
                                            new Vector3(7.4f, 0f, -3f)};

            Vector3[] positionArray2 = new[] { new Vector3(-2.65f, 0, -8.5f),
                                               new Vector3(-4.8f,0,10.5f),
                                               new Vector3(-15.1f,0,0.48f)};


            //loop through the array position
            for (int i = 0; i < positionArray.Length; ++i)
            {
                // Create a ghost prefab
                var instance = Instantiate(prefabghost);
                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
                instanceNetworkObject.Spawn();
                instanceNetworkObject.transform.parent = enemies.transform;

                // get the waypoint component
                WaypointPatrol patrolPoints = instanceNetworkObject.GetComponent<WaypointPatrol>();

                //assign the values
                patrolPoints.waypoints.Add(wayPoints.transform.GetChild(i * 2));
                patrolPoints.waypoints.Add(wayPoints.transform.GetChild(i * 2 + 1));

                patrolPoints.StartAI();

                //obj.GetComponentInChildren<Observer>().gameEnding = endingScript;
            }

            foreach(Vector3 c in positionArray2)
            {
                var instance = Instantiate(prefabgargoyle);
                instance.transform.position = c;
                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
                instanceNetworkObject.Spawn();
                instanceNetworkObject.transform.parent = enemies.transform;
            }

            if(testingBoss)
            {
                var instance = Instantiate(prefabBoss);
                instance.transform.position = new Vector3(-13.9f, -0.52f, -1.78f);
                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
                instanceNetworkObject.Spawn();
            }
        }
    }
}