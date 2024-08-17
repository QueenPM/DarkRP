namespace TabMenu{
      public struct PanelData {
        public GameObject PrinterPrefab;
        public GameObject PlayerPrefab;

        public PanelData(GameObject printerPrefab, GameObject playerPrefab) {
            PrinterPrefab = printerPrefab;
            PlayerPrefab = playerPrefab;
        }
    }

    interface IPanel {
        void Show();
    }
}